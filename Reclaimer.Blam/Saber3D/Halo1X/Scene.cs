using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Scene : CeaModelBase, INodeGraph
    {
        public UnknownBoundsBlock0x2002 Bounds => Blocks.OfType<UnknownListBlock0x1F01>().SingleOrDefault().Bounds;

        List<BoneBlock> INodeGraph.Bones { get; }

        public Scene(PakItem item)
            : base(item)
        {
            Blocks = new List<DataBlock>();

            using (var x = CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);

                while (reader.Position < item.Size)
                    Blocks.Add(reader.ReadBlock(this));

                NodeLookup = NodeGraph.AllDescendants.Where(n => n.MeshId.HasValue).ToDictionary(n => n.MeshId.Value);
            }
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
