using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public class scenario_structure_bsp : ContentTagDefinition, IRenderGeometry
    {
        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        [Offset(68)]
        public RealBounds XBounds { get; set; }

        [Offset(76)]
        public RealBounds YBounds { get; set; }

        [Offset(84)]
        public RealBounds ZBounds { get; set; }

        [Offset(172)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(180)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(328)]
        public BlockCollection<BspSectionBlock> Sections { get; set; }

        [Offset(336)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);

            var model = new GeometryModel(Item.FileName) { CoordinateSystem = CoordinateSystem.Default };
            model.Materials.AddRange(Halo2Common.GetMaterials(Shaders));

            #region Clusters
            var clusterRegion = new GeometryRegion { Name = "Clusters" };

            foreach (var section in Clusters.Where(s => s.VertexCount > 0))
            {
                var sectionIndex = Clusters.IndexOf(section);

                var data = section.DataPointer.ReadData(section.DataSize);
                var baseAddress = section.HeaderSize + 8;

                using (var ms = new MemoryStream(data))
                using (var reader = new EndianReader(ms, ByteOrder.LittleEndian))
                {
                    var sectionInfo = reader.ReadObject<MeshResourceDetailsBlock>();

                    var mesh = new GeometryMesh();

                    var indexFormat = section.FaceCount * 3 == sectionInfo.IndexCount
                        ? IndexFormat.TriangleList
                        : IndexFormat.TriangleStrip;

                    PopulateMeshData(reader, mesh, baseAddress, sectionInfo.IndexCount, section.VertexCount, section.Resources, indexFormat);

                    var perm = new GeometryPermutation
                    {
                        SourceIndex = Clusters.IndexOf(section),
                        Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                        MeshIndex = model.Meshes.Count,
                        MeshCount = 1
                    };

                    clusterRegion.Permutations.Add(perm);
                    model.Meshes.Add(mesh);
                }
            }

            model.Regions.Add(clusterRegion);
            #endregion

            #region Instances
            foreach (var section in Sections.Where(s => s.VertexCount > 0))
            {
                var sectionIndex = Sections.IndexOf(section);
                var sectionRegion = new GeometryRegion { Name = Utils.CurrentCulture($"Instances {sectionIndex:D3}") };

                var data = section.DataPointer.ReadData(section.DataSize);
                var baseAddress = section.HeaderSize + 8;

                using (var ms = new MemoryStream(data))
                using (var reader = new EndianReader(ms, ByteOrder.LittleEndian))
                {
                    var sectionInfo = reader.ReadObject<MeshResourceDetailsBlock>();

                    var mesh = new GeometryMesh { IsInstancing = true };

                    var indexFormat = section.FaceCount * 3 == sectionInfo.IndexCount
                        ? IndexFormat.TriangleList
                        : IndexFormat.TriangleStrip;

                    PopulateMeshData(reader, mesh, baseAddress, sectionInfo.IndexCount, section.VertexCount, section.Resources, indexFormat);

                    var perms = GeometryInstances
                        .Where(i => i.SectionIndex == sectionIndex)
                        .Select(i => new GeometryPermutation
                        {
                            SourceIndex = GeometryInstances.IndexOf(i),
                            Name = i.Name,
                            Transform = i.Transform,
                            TransformScale = i.TransformScale,
                            MeshIndex = model.Meshes.Count,
                            MeshCount = 1
                        }).ToList();

                    sectionRegion.Permutations.AddRange(perms);
                    model.Meshes.Add(mesh);
                }

                model.Regions.Add(sectionRegion);
            }
            #endregion

            return model;
        }

        private static void PopulateMeshData(EndianReader reader, GeometryMesh mesh, int baseAddress, int indexCount, int vertexCount, IReadOnlyList<ResourceInfoBlock> resourceBlocks, IndexFormat indexFormat)
        {
            var submeshResource = resourceBlocks[0];
            var indexResource = resourceBlocks.FirstOrDefault(r => r.Type0 == 32);
            var vertexResource = resourceBlocks.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 0);
            var uvResource = resourceBlocks.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 1);
            var normalsResource = resourceBlocks.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 2);

            reader.Seek(baseAddress + submeshResource.Offset, SeekOrigin.Begin);
            var submeshes = reader.ReadArray<SubmeshDataBlock>(submeshResource.Size / 72);

            mesh.Submeshes.AddRange(
                submeshes.Select(s => new GeometrySubmesh
                {
                    MaterialIndex = s.ShaderIndex,
                    IndexStart = s.IndexStart,
                    IndexLength = s.IndexLength
                })
            );

            reader.Seek(baseAddress + indexResource.Offset, SeekOrigin.Begin);
            mesh.IndexBuffer = IndexBuffer.FromArray(reader.ReadArray<ushort>(indexCount), indexFormat);

            var positionBuffer = new VectorBuffer<RealVector3>(vertexCount);
            var texCoordsBuffer = new VectorBuffer<RealVector2>(vertexCount);
            var normalBuffer = new VectorBuffer<HenDN3>(vertexCount);

            mesh.VertexBuffer = new VertexBuffer();
            mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
            mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);
            mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

            var vertexSize = vertexResource.Size / vertexCount;
            for (var i = 0; i < vertexCount; i++)
            {
                reader.Seek(baseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                positionBuffer[i] = new RealVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            }

            for (var i = 0; i < vertexCount; i++)
            {
                reader.Seek(baseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                texCoordsBuffer[i] = new RealVector2(reader.ReadSingle(), reader.ReadSingle());
            }

            for (var i = 0; i < vertexCount; i++)
            {
                reader.Seek(baseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                normalBuffer[i] = new HenDN3(reader.ReadUInt32());
            }
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo2Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo2Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [FixedSize(176)]
    public class ClusterBlock
    {
        [Offset(0)]
        public ushort VertexCount { get; set; }

        [Offset(2)]
        public ushort FaceCount { get; set; }

        [Offset(24)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(40)]
        public DataPointer DataPointer { get; set; }

        [Offset(44)]
        public int DataSize { get; set; }

        [Offset(48)]
        public int HeaderSize { get; set; }

        [Offset(56)]
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(200)]
    public class BspSectionBlock : ClusterBlock
    {

    }

    [FixedSize(88)]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public float TransformScale { get; set; }

        [Offset(4)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }

        [Offset(80)]
        public StringId Name { get; set; }

        public override string ToString() => Name;
    }
}
