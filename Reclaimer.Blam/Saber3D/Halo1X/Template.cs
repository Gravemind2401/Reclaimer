using Adjutant.Spatial;
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

            var region = new ModelRegion { Name = Name };

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
                    Transform = Matrix4x4.Transpose(node.Transform ?? Matrix4x4.Identity)
                };

                if (node.ObjectType == ObjectType.StandardMesh)
                {
                    var mesh = GetMesh(node, materials);

                    var bounds = node.Bounds.IsEmpty ? new DummyBounds(30) : new DummyBounds(node.Bounds.MinBound, node.Bounds.MaxBound);
                    SetBounds(mesh, bounds);

                    model.Meshes.Add(mesh);
                }
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes.Where(s => compoundVertexBuffers.ContainsKey(s.CompoundSourceId.Value)))
                        model.Meshes.Add(GetCompoundMesh(node, submesh));
                }

                region.Permutations.Add(perm);
            }

            model.Regions.Add(region);

            return model;

            void SetBounds(Mesh mesh, DummyBounds bounds)
            {
                var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
                (mesh.PositionBounds, mesh.TextureBounds) = (posBounds, texBounds);
            }

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

                SetBounds(mesh, new DummyBounds(30));

                mesh.Segments.Add(new MeshSegment
                {
                    Material = materials.ElementAtOrDefault(segment.Materials[0].MaterialIndex),
                    IndexStart = 0,
                    IndexLength = mesh.IndexBuffer.Count
                });

                return mesh;
            }
        }

        private class DummyBounds : IRealBounds5D
        {
            public RealBounds XBounds { get; set; } = (0, 1);
            public RealBounds YBounds { get; set; } = (0, 1);
            public RealBounds ZBounds { get; set; } = (0, 1);
            public RealBounds UBounds { get; set; } = (0, 1);
            public RealBounds VBounds { get; set; } = (0, 1);

            public DummyBounds(float scale)
            {
                var (min, max) = (-scale / 2, scale / 2);
                XBounds = YBounds = ZBounds = (min, max);
            }

            public DummyBounds(RealVector3 min, RealVector3 max)
            {
                XBounds = (min.X, max.X);
                YBounds = (min.Y, max.Y);
                ZBounds = (min.Z, max.Z);
            }
        }
    }
}
