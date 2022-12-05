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

                var perm = new GeometryPermutation
                {
                    Name = node.MeshName,
                    MeshIndex = model.Meshes.Count,
                    MeshCount = meshCount,
                    Transform = defaultTransform,
                    //Transform = transform,
                    TransformScale = 1
                };

                if (node.Positions.Count > 0)
                    model.Meshes.Add(GetMesh(node, node.Bounds, null));
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes)
                        model.Meshes.Add(GetMesh(submesh.CompoundSource, node.Bounds, submesh));
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

            GeometryMesh GetMesh(NodeGraphBlock0xF000 block, BoundsBlock0x1D01 boundsBlock, SubmeshInfo segment)
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

                var unknown = new int[mesh.VertexBuffer.Count];
                if (block.BlendData?.IndexData != null)
                {
                    reader.Seek(block.BlendData.IndexData.Header.StartOfBlock + 6 + 4, SeekOrigin.Begin);
                    for (var i = 0; i < mesh.VertexBuffer.Count; i++)
                        unknown[i] = reader.ReadInt32();
                }

                var unknown2 = new int[mesh.VertexBuffer.Count];
                if (block.BlendData?.WeightData != null)
                {
                    reader.Seek(block.BlendData.WeightData.Header.StartOfBlock + 6 + 0, SeekOrigin.Begin);
                    for (var i = 0; i < mesh.VertexBuffer.Count; i++)
                        unknown2[i] = reader.ReadInt32();
                }

                if (segment != null)
                {
                    if (!compoundOffsets.ContainsKey(block))
                        compoundOffsets.Add(block, 0);

                    var indexCount = segment.UnknownMeshDetails?.IndexCount ?? block.SubmeshData.Submeshes[0].UnknownMeshDetails.IndexCount;
                    mesh.Submeshes.Add(new GeometrySubmesh
                    {
                        MaterialIndex = (short)segment.Materials[0].MaterialIndex,
                        IndexStart = segment.UnknownMeshDetails == null ? 0 : compoundOffsets[block],
                        IndexLength = indexCount
                    });

                    if (segment.UnknownMeshDetails != null)
                        compoundOffsets[block] += indexCount;
                }
                else
                {
                    foreach (var submesh in block.SubmeshData.Submeshes)
                    {
                        mesh.Submeshes.Add(new GeometrySubmesh
                        {
                            MaterialIndex = (short)submesh.Materials[0].MaterialIndex,
                            IndexStart = submesh.FaceRange.Offset * 3,
                            IndexLength = submesh.FaceRange.Count * 3
                        });
                    }
                }

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
