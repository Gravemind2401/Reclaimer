using Reclaimer.Geometry;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Scene : CeaModelBase
    {
        public UnknownBoundsBlock0x2002 Bounds => Blocks.OfType<UnknownListBlock0x1F01>().SingleOrDefault().Bounds;

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

        protected override Model GetModelContent()
        {
            var model = new Model { Name = Item.Name };
            var materials = GetMaterials();

            var region = new ModelRegion { Name = Item.Name };

            foreach (var node in NodeGraph.AllDescendants.Where(n => n.ObjectType is ObjectType.StandardMesh or ObjectType.DeferredSceneMesh))
            {
                var perm = new ModelPermutation
                {
                    Name = node.MeshName,
                    MeshRange = (model.Meshes.Count, 1),
                    UniformScale = 50
                };

                if (node.ObjectType == ObjectType.StandardMesh)
                    model.Meshes.Add(GetMesh(node, materials));
                else
                {
                    foreach (var submesh in node.SubmeshData.Submeshes)
                        model.Meshes.Add(GetCompoundMesh(node, submesh));
                }

                region.Permutations.Add(perm);
            }

            model.Regions.Add(region);
            return model;

            Mesh GetCompoundMesh(NodeGraphBlock0xF000 host, SubmeshInfo segment)
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

                var mesh = new Mesh
                {
                    VertexBuffer = sourceVertices.Slice(vertStart, vertCount),
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
