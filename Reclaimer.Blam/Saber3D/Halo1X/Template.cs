using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Template : CeaModelBase
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
            var materials = GetMaterials();

            var defaultRegion = new ModelRegion { Name = Name };
            var regionLookup = NodeGraph.AllDescendants.Where(n => n.ObjectType == ObjectType.SkinCompound)
                .ToDictionary(n => n.MeshId.Value, n => new ModelRegion { Name = n.MeshName });

            var compoundVertexBuffers = new Dictionary<int, VertexBuffer>();
            var compoundIndexBuffers = new Dictionary<int, IndexBuffer>();
            var skinCompounds = from n in NodeGraph.AllDescendants
                                where n.ObjectType == ObjectType.SkinCompound
                                select n;

            //populate VertexRange of each SkinCompound component for later use, also create buffers to pull from
            foreach (var compound in skinCompounds)
            {
                var indexData = compound.BlendData.IndexData;
                var blendIndices = MemoryMarshal.Cast<byte, int>(indexData.BlendIndexBuffer.GetBuffer().AsSpan());

                var vb = new VertexBuffer();
                vb.PositionChannels.Add(compound.Positions.PositionBuffer);
                if (compound.VertexData?.Count > 0)
                    vb.TextureCoordinateChannels.Add(compound.VertexData.TexCoordsBuffer);

                compoundVertexBuffers.Add(compound.MeshId.Value, vb);
                compoundIndexBuffers.Add(compound.MeshId.Value, compound.Faces.IndexBuffer);

                var components = Enumerable.Range(indexData.FirstNodeId, indexData.NodeCount)
                    .Select((id, ordinal) => (ordinal, NodeLookup[id]));

                foreach (var (ordinal, component) in components)
                {
                    if (!blendIndices.Contains(ordinal))
                        continue; //not all ids within (FirstNodeId..NodeCount) are always referenced

                    var vertexRange = blendIndices.IndexOf(ordinal)..(blendIndices.LastIndexOf(ordinal) + 1);
                    foreach (var submesh in component.SubmeshData.Submeshes.Where(s => s.CompoundSourceId == compound.MeshId))
                        (submesh.VertexRange.Offset, submesh.VertexRange.Count) = vertexRange.GetOffsetAndLength(blendIndices.Length);
                }
            }

            foreach (var node in NodeGraph.AllDescendants.Where(n => n.ObjectType is ObjectType.StandardMesh or ObjectType.DeferredSkinMesh))
            {
                var meshCount = node.ObjectType == ObjectType.StandardMesh ? 1 : node.SubmeshData.Submeshes.Count;

                var tlist = new List<Matrix4x4>();
                var next = node;
                do
                {
                    tlist.Add(next.Transform.HasValue ? Matrix4x4.Transpose(node.Transform.Value) : Matrix4x4.Identity);
                    next = next.ParentNode;
                }
                while (next != null);

                var perm = new ModelPermutation
                {
                    Name = node.MeshName,
                    MeshRange = (model.Meshes.Count, meshCount),
                    Transform = node.Positions.GetTransform()
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

            model.Regions.Add(defaultRegion);
            model.Regions.AddRange(regionLookup.Values.Where(r => r.Permutations.Any()));

            return model;

            Mesh GetCompoundMesh(NodeGraphBlock0xF000 host, SubmeshInfo segment)
            {
                var compound = segment.CompoundSource;
                var compoundId = segment.CompoundSourceId.Value;
                var sourceIndices = compoundIndexBuffers[compoundId];
                var sourceVertices = compoundVertexBuffers[compoundId];

                //doesnt appear to be any way to get exact index offet+count, so this will do
                var indices = sourceIndices
                    .Select(i => i - segment.VertexRange.Offset)
                    .SkipWhile(i => i < 0)
                    .TakeWhile(i => i < segment.VertexRange.Count);

                var mesh = new Mesh
                {
                    VertexBuffer = sourceVertices.Slice(segment.VertexRange.Offset, segment.VertexRange.Count),
                    IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList)
                };

                var transform = compound.Positions.GetTransform();
                var positions = new VectorBuffer<RealVector3>(mesh.VertexBuffer.Count);
                for (var i = 0; i < positions.Count; i++)
                {
                    var raw = mesh.VertexBuffer.PositionChannels[0][i];
                    var vec = Vector3.Transform(new Vector3(raw.X, raw.Y, raw.Z), transform);
                    positions[i] = new RealVector3(vec.X, vec.Y, vec.Z);
                }

                mesh.VertexBuffer.PositionChannels[0] = positions;

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
