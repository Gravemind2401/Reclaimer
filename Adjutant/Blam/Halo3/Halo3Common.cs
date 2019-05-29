using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Halo3
{
    [FixedSize(28)]
    public struct VertexBufferInfo
    {
        [Offset(0)]
        public int VertexCount { get; set; }

        [Offset(8)]
        public int DataLength { get; set; }
    }

    [FixedSize(24)]
    public struct IndexBufferInfo
    {
        [Offset(0)]
        public IndexFormat IndexFormat { get; set; }

        [Offset(4)]
        public int DataLength { get; set; }
    }

    public static class Halo3Common
    {
        public static IEnumerable<GeometryMaterial> GetMaterials(IList<ShaderBlock> shaders)
        {
            var shadersMeta = shaders.Select(s => s.ShaderReference.Tag?.ReadMetadata<shader>()).ToList();
            foreach (var shader in shadersMeta)
            {
                if (shader == null)
                {
                    yield return null;
                    continue;
                }

                var template = shader.ShaderProperties[0].TemplateReference.Tag.ReadMetadata<render_method_template>();
                var stringId = template.Usages.FirstOrDefault(s => s.Value == "base_map");

                if (stringId.Value == null)
                {
                    yield return null;
                    continue;
                }

                var diffuseIndex = template.Usages.IndexOf(stringId);
                var map = shader.ShaderProperties[0].ShaderMaps[diffuseIndex];
                var bitmTag = map.BitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return null;
                    continue;
                }

                var tile = map.TilingIndex == byte.MaxValue 
                    ? (RealVector4D?)null 
                    : shader.ShaderProperties[0].TilingData[map.TilingIndex];

                yield return new GeometryMaterial
                {
                    Name = bitmTag.FileName,
                    Diffuse = bitmTag.ReadMetadata<bitmap>(),
                    Tiling = new RealVector2D(tile?.X ?? 1, tile?.Y ?? 1)
                };
            }
        }

        public static IEnumerable<GeometryMesh> GetMeshes(CacheFile cache, ResourceIdentifier resourcePointer, IList<SectionBlock> sections)
        {
            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var entry = cache.ResourceGestalt.ResourceEntries[resourcePointer.ResourceIndex];
            using (var cacheReader = cache.CreateReader(cache.MetadataTranslator))
            using (var reader = cacheReader.CreateVirtualReader(cache.ResourceGestalt.FixupDataPointer.Address))
            {
                reader.Seek(entry.FixupOffset + (entry.FixupSize - 24), SeekOrigin.Begin);
                var vertexBufferCount = reader.ReadInt32();
                reader.Seek(8, SeekOrigin.Current);
                var indexBufferCount = reader.ReadInt32();

                reader.Seek(entry.FixupOffset, SeekOrigin.Begin);
                vertexBufferInfo = reader.ReadEnumerable<VertexBufferInfo>(vertexBufferCount).ToArray();
                reader.Seek(12 * vertexBufferCount, SeekOrigin.Current); //12 byte struct here for each vertex buffer
                indexBufferInfo = reader.ReadEnumerable<IndexBufferInfo>(indexBufferCount).ToArray();
                //12 byte struct here for each index buffer
                //4x 12 byte structs here
            }

            using (var ms = new MemoryStream(resourcePointer.ReadData()))
            using (var reader = new EndianReader(ms, ByteOrder.BigEndian))
            {
                var doc = new XmlDocument();
                doc.LoadXml(Adjutant.Properties.Resources.Halo3VertexBuffer);

                var lookup = doc.FirstChild.ChildNodes.Cast<XmlNode>()
                    .ToDictionary(n => Convert.ToInt32(n.Attributes["type"].Value, 16));

                foreach (var section in sections)
                {
                    if (section.VertexBufferIndex < 0 || section.IndexBufferIndex < 0)
                    {
                        yield return null;
                        continue;
                    }

                    var sectionIndex = sections.IndexOf(section);
                    var node = lookup[section.VertexFormat];
                    var vInfo = vertexBufferInfo[section.VertexBufferIndex];
                    var iInfo = indexBufferInfo[section.IndexBufferIndex];

                    var mesh = new GeometryMesh
                    {
                        IndexFormat = iInfo.IndexFormat,
                        Vertices = new IVertex[vInfo.VertexCount]
                    };

                    mesh.Submeshes.AddRange(
                        section.Submeshes.Select(s => new GeometrySubmesh
                        {
                            MaterialIndex = s.ShaderIndex,
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        })
                    );

                    var address = entry.ResourceFixups[section.VertexBufferIndex].Offset & 0x0FFFFFFF;
                    reader.Seek(address, SeekOrigin.Begin);
                    for (int i = 0; i < vInfo.VertexCount; i++)
                    {
                        var vert = new XmlVertex(reader, node);
                        mesh.Vertices[i] = vert;
                    }

                    var totalIndices = section.Submeshes.Sum(s => s.IndexLength);
                    address = entry.ResourceFixups[vertexBufferInfo.Length * 2 + section.IndexBufferIndex].Offset & 0x0FFFFFFF;
                    reader.Seek(address, SeekOrigin.Begin);
                    if (totalIndices > ushort.MaxValue)
                        mesh.Indicies = reader.ReadEnumerable<int>(totalIndices).ToArray();
                    else mesh.Indicies = reader.ReadEnumerable<ushort>(totalIndices).Select(i => (int)i).ToArray();

                    yield return mesh;
                }
            }
        }
    }
}
