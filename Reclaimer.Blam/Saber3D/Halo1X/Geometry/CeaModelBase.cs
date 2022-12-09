using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class CeaModelBase : ContentItemDefinition
    {
        public List<DataBlock> Blocks { get; protected init; }
        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; protected init; }

        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();
        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;

        protected CeaModelBase(PakItem item)
            : base(item)
        { }

        protected EndianReader CreateReader()
        {
            var reader = Container.CreateReader();
            reader.RegisterInstance(this);
            reader.RegisterInstance(Item);
            return reader;
        }

        public IEnumerable<IBitmap> GetAllBitmaps()
        {
            return from m in Materials
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        public  IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            return from m in shaderIndexes.Select(i => Materials[i])
                   let i = Container.FindItem(PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        protected GeometryMaterial GetMaterial(MaterialReferenceBlock block)
        {
            var index = Materials.IndexOf(block);
            var material = new GeometryMaterial { Name = block.Value };

            material.Submaterials.Add(new SubMaterial
            {
                Usage = MaterialUsage.Diffuse,
                Bitmap = GetBitmaps(Enumerable.Repeat(index, 1)).FirstOrDefault(),
                Tiling = new RealVector2(1, 1)
            });

            return material;
        }

        protected static GeometryMesh GetMesh(NodeGraphBlock0xF000 block)
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
    }
}
