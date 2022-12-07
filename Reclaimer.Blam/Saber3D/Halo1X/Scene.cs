using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Scene : ContentItemDefinition, INodeGraph
    {
        public List<DataBlock> Blocks { get; } = new List<DataBlock>();

        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }

        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;
        public UnknownBoundsBlock0x2002 Bounds => Blocks.OfType<UnknownListBlock0x1F01>().SingleOrDefault().Bounds;
        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();

        List<BoneBlock> INodeGraph.Bones { get; }

        public Scene(PakItem item)
            : base(item)
        {
            using (var x = CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);

                while (reader.Position < item.Size)
                    Blocks.Add(reader.ReadBlock(this));

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

        PakItem INodeGraph.Item => Item;

        int IRenderGeometry.LodCount => 1;

        IGeometryModel IRenderGeometry.ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            using var x = CreateReader();
            using var reader = x.CreateVirtualReader(Item.Address);

            var model = new GeometryModel(Item.Name) { CoordinateSystem = CoordinateSystem.HaloCEX };
            var defaultTransform = CoordinateSystem.GetTransform(CoordinateSystem.HaloCEX, CoordinateSystem.Default);

            model.Materials.AddRange(Materials.Select(GetMaterial));

            var region = new GeometryRegion { Name = Item.Name };

            foreach (var node in NodeGraph.AllDescendants.Where(n => n.ObjectType is ObjectType.StandardMesh or ObjectType.DeferredSceneMesh))
            {
                var perm = new GeometryPermutation
                {
                    Name = node.MeshName,
                    MeshIndex = model.Meshes.Count,
                    MeshCount = 1,
                    Transform = defaultTransform,
                    TransformScale = 50
                };

                if (node.ObjectType == ObjectType.StandardMesh)
                    model.Meshes.Add(GetMesh(node));
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes)
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

            GeometryMesh GetMesh(NodeGraphBlock0xF000 block)
            {
                var mesh = new GeometryMesh
                {
                    VertexBuffer = new VertexBuffer(),
                    IndexBuffer = block.Faces.IndexBuffer
                };

                mesh.VertexBuffer.PositionChannels.Add(block.Positions.PositionBuffer);
                if (block.VertexData?.Count > 0)
                    mesh.VertexBuffer.TextureCoordinateChannels.Add(block.VertexData.TexCoordsBuffer);

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
                var source = NodeLookup[host.MeshDataSource.SourceMeshId];

                var (faceStart, faceCount) = (host.MeshDataSource.FaceOffset + segment.FaceRange.Offset, segment.FaceRange.Count);
                var (vertStart, vertCount) = (host.MeshDataSource.VertexOffset + segment.VertexRange.Offset, segment.VertexRange.Count);

                var sourceIndices = source.Faces.IndexBuffer;
                var sourceVertices = new VertexBuffer();

                sourceVertices.PositionChannels.Add(source.Positions.PositionBuffer);
                if (source.VertexData?.Count > 0)
                    sourceVertices.TextureCoordinateChannels.Add(source.VertexData.TexCoordsBuffer);

                var indices = Enumerable.Range(faceStart * 3, faceCount * 3)
                    .Select(i => sourceIndices[i]);

                var mesh = new GeometryMesh
                {
                    VertexBuffer = sourceVertices.Slice(vertStart, vertCount),
                    IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList)
                };

                mesh.Submeshes.Add(new GeometrySubmesh
                {
                    MaterialIndex = (short)segment.Materials[0].MaterialIndex,
                    IndexStart = 0,
                    IndexLength = mesh.IndexBuffer.Count
                });

                return mesh;
            }
        }

        #endregion
    }
}
