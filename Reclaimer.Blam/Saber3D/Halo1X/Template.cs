using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;
using System.Numerics;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Template : CeaModelBase, INodeGraph
    {
        public string Name => Blocks.OfType<StringBlock0xE502>().SingleOrDefault()?.Value;
        public List<BoneBlock> Bones => Blocks.OfType<BoneListBlock>().SingleOrDefault()?.Bones;
        public string UnknownString => Blocks.OfType<StringBlock0x0403>().SingleOrDefault()?.Value;
        public MatrixListBlock0x0D03 MatrixList => Blocks.OfType<TransformBlock0x0503>().SingleOrDefault()?.MatrixList;
        public BoundsBlock0x0803 Bounds => Blocks.OfType<BoundsBlock0x0803>().SingleOrDefault();

        public Template(PakItem item)
            : base(item)
        {
            using (var x = CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);
                var root = reader.ReadBlock(this);

                Blocks = (root as TemplateBlock)?.ChildBlocks;
                NodeLookup = NodeGraph.AllDescendants.Where(n => n.MeshId.HasValue).ToDictionary(n => n.MeshId.Value);
            }
        }

        protected override Model GetModelContent()
        {
            var model = new Model { Name = Item.Name };

            Matrix4x4 GetBoneTransform(int boneIndex)
            {
                var bone = Bones.ElementAtOrDefault(boneIndex);
                if (bone == null)
                    return Matrix4x4.Identity;

                var block = bone.GetNodeBlock();
                var transform = block.Transform.Value;

                if (block.MeshId < MatrixList?.MatrixCount)
                {
                    var offsetMatrix = MatrixList.Matrices[block.MeshId.Value].Inverse();
                    if (!offsetMatrix.IsIdentity)
                        return offsetMatrix;
                }

                var parent = GetBoneTransform(bone.ParentIndex);
                return transform * parent;
            }

            model.Bones.AddRange(Bones.Select(n =>
            {
                var transform = CoordinateSystem.GetTransform(GetBoneTransform(n.ParentIndex), GetBoneTransform(n.Index));

                return new Bone
                {
                    Name = n.Name,
                    Transform = transform,
                    ParentIndex = n.ParentIndex
                };
            }));

            var materials = GetMaterials();

            var defaultRegion = new ModelRegion { Name = Name };
            var regionLookup = NodeGraph.AllDescendants.Where(n => n.ObjectType == ObjectType.SkinCompound)
                .ToDictionary(n => n.MeshId.Value, n => new ModelRegion { Name = n.MeshName });

            var compoundVertexBuffers = new Dictionary<int, VertexBuffer>();
            var compoundIndexBuffers = new Dictionary<int, IndexBuffer>();
            var compoundRanges = new Dictionary<(int, int), List<Range>>();
            var skinCompounds = from n in NodeGraph.AllDescendants
                                where n.ObjectType == ObjectType.SkinCompound
                                select n;

            //create shared vertex buffers for each skin compound and identify distribution of vertices among dependents
            foreach (var compound in skinCompounds)
            {
                var indexData = compound.BlendData.IndexData;

                var vb = new VertexBuffer();
                vb.PositionChannels.Add(compound.Positions.PositionBuffer);
                if (compound.VertexData?.Count > 0)
                    vb.TextureCoordinateChannels.Add(compound.VertexData.TexCoordsBuffer);

                if (indexData?.BlendIndexBuffer?.Count > 0)
                {
                    try
                    {
                        var indexBuffer = indexData.CreateMappedIndexBuffer();
                        vb.BlendIndexChannels.Add(indexBuffer);

                        var weightBuffer = new VectorBuffer<UByteN4>(indexBuffer.Count);
                        for (var i = 0; i < weightBuffer.Count; i++)
                            weightBuffer[i] = new UByteN4(1f, 0, 0, 0);
                        vb.BlendWeightChannels.Add(weightBuffer);
                    }
                    catch
                    {
                        //TODO: sometimes the blend indices point to non-bone objects, even on models with no bones
                        //probably need to create implied bones for them
                    }
                }

                compoundVertexBuffers.Add(compound.MeshId.Value, vb);
                compoundIndexBuffers.Add(compound.MeshId.Value, compound.Faces.IndexBuffer);

                //the blend indices point to the dependent meshes that make use of the corresponding vertices
                //however the indices are not always contiguous, so we need to find all sets of indices per dependent

                var position = 0;
                while (position < indexData.BlendIndexBuffer.Count)
                {
                    var offset = position;
                    while (position < indexData.BlendIndexBuffer.Count && indexData.BlendIndexBuffer[position].X == indexData.BlendIndexBuffer[offset].X)
                        position++;

                    var range = offset..position;
                    var id = indexData.FirstNodeId + indexData.BlendIndexBuffer[offset].X;
                    var key = (compound.MeshId.Value, id);

                    if (!compoundRanges.ContainsKey(key))
                        compoundRanges.Add(key, new List<Range>());

                    compoundRanges[key].Add(range);
                }
            }

            foreach (var node in NodeGraph.AllDescendants.Where(n => n.ObjectType is ObjectType.StandardMesh or ObjectType.DeferredSkinMesh))
            {
                var meshCount = node.ObjectType == ObjectType.StandardMesh ? 1 : node.SubmeshData.Submeshes.Count;

                var perm = new ModelPermutation
                {
                    Name = node.MeshName,
                    MeshRange = (model.Meshes.Count, meshCount),
                    Transform = node.GetFinalTransform()
                };

                if (node.ObjectType == ObjectType.StandardMesh)
                {
                    model.Meshes.Add(GetMesh(node, materials));
                    defaultRegion.Permutations.Add(perm);
                }
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes.Where(s => compoundVertexBuffers.ContainsKey(s.CompoundSourceId.Value)))
                        model.Meshes.Add(GetCompoundMesh(node, submesh));
                    regionLookup[node.SubmeshData.Submeshes[0].CompoundSourceId.Value].Permutations.Add(perm);
                }
            }

            model.Regions.AddRange(regionLookup.Values.Prepend(defaultRegion).Where(r => r.Permutations.Any()));
            return model;

            Mesh GetCompoundMesh(NodeGraphBlock0xF000 host, SubmeshInfo segment)
            {
                var compoundId = segment.CompoundSourceId.Value;
                var sourceIndices = compoundIndexBuffers[compoundId];
                var sourceVertices = compoundVertexBuffers[compoundId];

                var rangeKey = (compoundId, host.MeshId.Value);
                var ranges = compoundRanges[rangeKey].Select(r => r.GetOffsetAndLength(sourceVertices.Count));

                //no way to get exact index offet+count, plus there can be multiple ranges, so we need to collate our own triangle list
                var indices = ranges.SelectMany(r => sourceIndices.Where(i => i >= r.Offset && i < r.Offset + r.Length));

                //TODO: only include vertices that are actually used by the new triangle list
                var mesh = new Mesh
                {
                    VertexBuffer = sourceVertices,
                    IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList)
                };

                mesh.Segments.Add(new MeshSegment
                {
                    Material = materials.ElementAtOrDefault(segment.Materials[0].MaterialIndex),
                    IndexStart = 0,
                    IndexLength = mesh.IndexBuffer.Count
                });

                return mesh;
            }
        }
    }
}
