using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Properties;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Halo5
{
    internal static class Halo5Common
    {
        public static IEnumerable<GeometryMaterial> GetMaterials(IList<MaterialBlock> materials)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                var tag = materials[i].MaterialReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = Utils.GetFileName(tag.FullPath)
                };

                var mat = tag?.ReadMetadata<material>();
                if (mat == null)
                {
                    yield return material;
                    continue;
                }

                var subMaterials = new List<ISubmaterial>();
                var map = mat.PostprocessDefinitions.FirstOrDefault()?.Textures.FirstOrDefault();
                var bitmTag = map?.BitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                //var tile = map.TilingIndex == byte.MaxValue
                //    ? (RealVector4D?)null
                //    : shader.ShaderProperties[0].TilingData[map.TilingIndex];

                try
                {
                    subMaterials.Add(new SubMaterial
                    {
                        Usage = MaterialUsage.Diffuse,
                        Bitmap = bitmTag.ReadMetadata<bitmap>(),
                        //Tiling = new RealVector2D(tile?.X ?? 1, tile?.Y ?? 1)
                        Tiling = new RealVector2D(1, 1)
                    });
                }
                catch { }

                if (subMaterials.Count == 0)
                {
                    yield return material;
                    continue;
                }

                material.Submaterials = subMaterials;

                yield return material;
            }
        }

        public static IEnumerable<GeometryMesh> GetMeshes(Module module, ModuleItem item, IList<SectionBlock> sections, int lod, Func<SectionBlock, short?> boundsIndex, Func<int, int, int> mapNode = null)
        {
            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var resourceIndex = module.Resources[item.ResourceIndex];
            var resource = module.Items[resourceIndex]; //this will be the [mesh resource!*] tag
            if (resource.ResourceCount > 0)
                System.Diagnostics.Debugger.Break();

            using (var blockReader = resource.CreateReader())
            {

                var header = new MetadataHeader(blockReader);
                using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(1)))
                {
                    //DataBlock 0: ?
                    //DataBlock 1: vertex buffer infos
                    //DataBlock 2: index buffer infos
                    //DataBlock 3+: additional vertex data block for each buffer
                    //DataBlock n+: additional index data block for each buffer
                    var bufferCount = (header.DataBlocks.Count - 3) / 2;

                    var block = header.DataBlocks[1];
                    reader.Seek(block.Offset, SeekOrigin.Begin);
                    vertexBufferInfo = reader.ReadEnumerable<VertexBufferInfo>(bufferCount).ToArray();

                    block = header.DataBlocks[2];
                    reader.Seek(block.Offset, SeekOrigin.Begin);
                    indexBufferInfo = reader.ReadEnumerable<IndexBufferInfo>(bufferCount).ToArray();
                }

                using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(2)))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(Resources.Halo5VertexBuffer);

                    var lookup = doc.DocumentElement.ChildNodes.Cast<XmlNode>()
                        .ToDictionary(n => Convert.ToInt32(n.Attributes[XmlVertexField.Type].Value, 16));

                    var sectionIndex = -1;
                    foreach (var section in sections)
                    {
                        sectionIndex++;
                        var lodData = section.SectionLods[Math.Min(lod, section.SectionLods.Count - 1)];

                        if (lodData.VertexBufferIndex < 0 || lodData.IndexBufferIndex < 0 || !lookup.ContainsKey(section.VertexFormat))
                        {
                            yield return new GeometryMesh();
                            continue;
                        }

                        var node = lookup[section.VertexFormat];
                        var vInfo = vertexBufferInfo[lodData.VertexBufferIndex];
                        var iInfo = indexBufferInfo[lodData.IndexBufferIndex];

                        var mesh = new GeometryMesh
                        {
                            IndexFormat = section.IndexFormat,
                            Vertices = new IVertex[vInfo.VertexCount],
                            VertexWeights = VertexWeights.None,
                            NodeIndex = section.NodeIndex == byte.MaxValue ? (byte?)null : section.NodeIndex,
                            BoundsIndex = 0
                        };

                        try
                        {
                            mesh.Submeshes.AddRange(
                                lodData.Submeshes.Select(s => new GeometrySubmesh
                                {
                                    MaterialIndex = s.ShaderIndex,
                                    IndexStart = s.IndexStart,
                                    IndexLength = s.IndexLength
                                })
                            );

                            var block = header.DataBlocks[3 + lodData.VertexBufferIndex];
                            reader.Seek(block.Offset, SeekOrigin.Begin);
                            for (int i = 0; i < vInfo.VertexCount; i++)
                            {
                                var vert = new XmlVertex(reader, node);
                                mesh.Vertices[i] = vert;
                            }

                            block = header.DataBlocks[3 + vertexBufferInfo.Length + lodData.IndexBufferIndex];
                            reader.Seek(block.Offset, SeekOrigin.Begin);
                            if (vInfo.VertexCount > ushort.MaxValue)
                                mesh.Indicies = reader.ReadEnumerable<int>(iInfo.IndexCount).ToArray();
                            else mesh.Indicies = reader.ReadEnumerable<ushort>(iInfo.IndexCount).Select(i => (int)i).ToArray();

                        }
                        catch
                        {
                            System.Diagnostics.Debugger.Break();
                        }

                        yield return mesh;
                    }
                }
            }
        }
    }
}
