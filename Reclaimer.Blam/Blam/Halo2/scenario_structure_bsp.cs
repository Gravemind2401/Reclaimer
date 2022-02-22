using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class scenario_structure_bsp : IRenderGeometry
    {
        private readonly IIndexItem item;

        public scenario_structure_bsp(IIndexItem item)
        {
            this.item = item;
        }

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

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel(item.FileName()) { CoordinateSystem = CoordinateSystem.Default };
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

                    var submeshResource = section.Resources[0];
                    var indexResource = section.Resources.FirstOrDefault(r => r.Type0 == 32);
                    var vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 0);
                    var uvResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 1);
                    var normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 2);

                    reader.Seek(baseAddress + submeshResource.Offset, SeekOrigin.Begin);
                    var submeshes = reader.ReadEnumerable<SubmeshDataBlock>(submeshResource.Size / 72).ToList();

                    var mesh = new GeometryMesh();

                    if (section.FaceCount * 3 == sectionInfo.IndexCount)
                        mesh.IndexFormat = IndexFormat.TriangleList;
                    else mesh.IndexFormat = IndexFormat.TriangleStrip;

                    reader.Seek(baseAddress + indexResource.Offset, SeekOrigin.Begin);
                    mesh.Indicies = reader.ReadEnumerable<ushort>(sectionInfo.IndexCount).Select(i => (int)i).ToArray();

                    #region Vertices
                    mesh.Vertices = new IVertex[section.VertexCount];
                    var vertexSize = vertexResource.Size / section.VertexCount;
                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = new WorldVertex();

                        reader.Seek(baseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                        vert.Position = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        mesh.Vertices[i] = vert;
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (WorldVertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                        vert.TexCoords = new RealVector2D(reader.ReadSingle(), reader.ReadSingle());
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (WorldVertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                        vert.Normal = new HenDN3(reader.ReadUInt32());
                    }
                    #endregion

                    var perm = new GeometryPermutation
                    {
                        SourceIndex = Clusters.IndexOf(section),
                        Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                        MeshIndex = model.Meshes.Count,
                        MeshCount = 1
                    };

                    foreach (var submesh in submeshes)
                    {
                        mesh.Submeshes.Add(new GeometrySubmesh
                        {
                            MaterialIndex = submesh.ShaderIndex,
                            IndexStart = submesh.IndexStart,
                            IndexLength = submesh.IndexLength
                        });
                    }

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

                    var submeshResource = section.Resources[0];
                    var indexResource = section.Resources.FirstOrDefault(r => r.Type0 == 32);
                    var vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 0);
                    var uvResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 1);
                    var normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 2);

                    reader.Seek(baseAddress + submeshResource.Offset, SeekOrigin.Begin);
                    var submeshes = reader.ReadEnumerable<SubmeshDataBlock>(submeshResource.Size / 72).ToList();

                    var mesh = new GeometryMesh { IsInstancing = true };

                    if (section.FaceCount * 3 == sectionInfo.IndexCount)
                        mesh.IndexFormat = IndexFormat.TriangleList;
                    else mesh.IndexFormat = IndexFormat.TriangleStrip;

                    reader.Seek(baseAddress + indexResource.Offset, SeekOrigin.Begin);
                    mesh.Indicies = reader.ReadEnumerable<ushort>(sectionInfo.IndexCount).Select(i => (int)i).ToArray();

                    #region Vertices
                    mesh.Vertices = new IVertex[section.VertexCount];
                    var vertexSize = vertexResource.Size / section.VertexCount;
                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = new WorldVertex();

                        reader.Seek(baseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                        vert.Position = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        mesh.Vertices[i] = vert;
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (WorldVertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                        vert.TexCoords = new RealVector2D(reader.ReadSingle(), reader.ReadSingle());
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (WorldVertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                        vert.Normal = new HenDN3(reader.ReadUInt32());
                    }
                    #endregion

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

                    mesh.Submeshes.AddRange(
                        submeshes.Select(s => new GeometrySubmesh
                        {
                            MaterialIndex = s.ShaderIndex,
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        })
                    );

                    sectionRegion.Permutations.AddRange(perms);
                    model.Meshes.Add(mesh);
                }

                model.Regions.Add(sectionRegion);
            }
            #endregion

            return model;
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
