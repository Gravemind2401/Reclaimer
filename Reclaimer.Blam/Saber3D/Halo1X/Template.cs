using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Template : ContentItemDefinition, INodeGraph, IRenderGeometry
    {
        public List<DataBlock> Blocks { get; }
        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }

        public string Name => Blocks.OfType<StringBlock0xE502>().SingleOrDefault()?.Value;
        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;
        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();
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

        private EndianReader CreateReader()
        {
            var reader = Container.CreateReader();
            reader.RegisterInstance(this);
            reader.RegisterInstance(Item);
            return reader;
        }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        IGeometryModel IRenderGeometry.ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);

            using var x = CreateReader();
            using var reader = x.CreateVirtualReader(Item.Address);

            var model = new GeometryModel(Item.Name) { CoordinateSystem = CoordinateSystem.HaloCEX };
            var defaultTransform = CoordinateSystem.GetTransform(CoordinateSystem.HaloCEX, CoordinateSystem.Default);

            model.Materials.AddRange(Materials.Select(GetMaterial));

            var compoundOffsets = new Dictionary<NodeGraphBlock0xF000, int>();

            var region = new GeometryRegion { Name = Name };

            var compoundVertexBuffers = new Dictionary<int, VertexBuffer>();
            var compoundIndexBuffers = new Dictionary<int, IndexBuffer>();
            var skinCompounds = from n in NodeGraph.AllDescendants
                                where n.ParentId == NodeGraph.AllDescendants[0].MeshId
                                && n.SubmeshData?.Submeshes?.Count == 1
                                && n.Positions?.Count > 0
                                && n.Bounds?.IsEmpty == true
                                select n;

            //populate VertexRange of each SkinCompound component for later use, also create buffers to pull from
            foreach (var compound in skinCompounds)
            {
                var indexData = compound.BlendData.IndexData;
                var blendIndices = MemoryMarshal.Cast<byte, int>(indexData.BlendIndexBuffer.GetBuffer().AsSpan());

                var vb = new VertexBuffer();
                vb.PositionChannels.Add(compound.Positions.PositionBuffer);
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

            foreach (var node in NodeGraph.AllDescendants.Where(n => n.SubmeshData != null && !n.Bounds.IsEmpty))
            {
                var meshCount = node.Positions.Count > 0 ? 1 : node.SubmeshData.Submeshes.Count;

                var tlist = new List<Matrix4x4>();
                var next = node;
                do
                {
                    var tform = MatrixList.Matrices.Cast<Matrix4x4?>().ElementAtOrDefault(next.MeshId.Value) ?? Matrix4x4.Identity;
                    var tform2 = next.Transform ?? Matrix4x4.Identity;
                    tlist.Add(tform2);
                    next = next.ParentNode;
                }
                while (next != null);

                var transform = Matrix4x4.Transpose(node.Transform ?? Matrix4x4.Identity);
                var perm = new GeometryPermutation
                {
                    Name = node.MeshName,
                    MeshIndex = model.Meshes.Count,
                    MeshCount = meshCount,
                    Transform = transform * defaultTransform,
                    //Transform = transform,
                    TransformScale = 1
                };

                if (node.Positions.Count > 0)
                    model.Meshes.Add(GetMesh(node, node.Bounds));
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes.Where(s => compoundVertexBuffers.ContainsKey(s.CompoundSourceId.Value)))
                        model.Meshes.Add(GetCompoundMesh(node, submesh));
                }

                region.Permutations.Add(perm);
            }

            model.Regions.Add(region);

            return model;

            GeometryMaterial GetMaterial(MaterialReferenceBlock block)
            {
                var index = Materials.IndexOf(block);
                var material = new GeometryMaterial { Name = block.Value };

                material.Submaterials.Add(new SubMaterial
                {
                    Usage = MaterialUsage.Diffuse,
                    Bitmap = ((IRenderGeometry)this).GetBitmaps(Enumerable.Repeat(index, 1)).FirstOrDefault(),
                    Tiling = new RealVector2(1, 1)
                });

                return material;
            }

            GeometryMesh GetMesh(NodeGraphBlock0xF000 block, BoundsBlock0x1D01 boundsBlock)
            {
                var mesh = new GeometryMesh
                {
                    VertexBuffer = new VertexBuffer(),
                    IndexBuffer = block.Faces.IndexBuffer,
                    BoundsIndex = (short)model.Bounds.Count
                };

                mesh.VertexBuffer.PositionChannels.Add(block.Positions.PositionBuffer);

                var bounds = boundsBlock.IsEmpty ? new DummyBounds(30) : new DummyBounds(boundsBlock.MinBound, boundsBlock.MaxBound);
                model.Bounds.Add(bounds);

                foreach (var submesh in block.SubmeshData.Submeshes)
                {
                    mesh.Submeshes.Add(new GeometrySubmesh
                    {
                        MaterialIndex = (short)submesh.Materials[0].MaterialIndex,
                        IndexStart = submesh.FaceRange.Offset * 3,
                        IndexLength = submesh.FaceRange.Count * 3
                    });
                }

                return mesh;
            }

            GeometryMesh GetCompoundMesh(NodeGraphBlock0xF000 host, SubmeshInfo segment)
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

                var mesh = new GeometryMesh
                {
                    VertexBuffer = sourceVertices.Slice(segment.VertexRange.Offset, segment.VertexRange.Count),
                    IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList),
                    BoundsIndex = (short)model.Bounds.Count
                };

                model.Bounds.Add(new DummyBounds(30));

                mesh.Submeshes.Add(new GeometrySubmesh
                {
                    MaterialIndex = (short)segment.Materials[0].MaterialIndex,
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

        IEnumerable<IBitmap> IRenderGeometry.GetAllBitmaps()
        {
            return from m in Materials
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        IEnumerable<IBitmap> IRenderGeometry.GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            return from m in shaderIndexes.Select(i => Materials[i])
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        #endregion
    }
}
