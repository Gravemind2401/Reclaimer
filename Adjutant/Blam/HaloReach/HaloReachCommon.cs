using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.HaloReach
{
    [FixedSize(28)]
    public struct VertexBufferInfo
    {
        [Offset(0)]
        public int VertexCount { get; set; }

        [Offset(8)]
        public int DataLength { get; set; }
    }

    [FixedSize(28)]
    public struct IndexBufferInfo
    {
        [Offset(0)]
        public IndexFormat IndexFormat { get; set; }

        [Offset(8)]
        public int DataLength { get; set; }
    }

    internal static class HaloReachCommon
    {
        private static readonly Dictionary<string, MaterialUsage> usageLookup = new Dictionary<string, MaterialUsage>
        {
            { "blend_map", MaterialUsage.BlendMap },
            { "base_map", MaterialUsage.Diffuse },
            { "detail_map", MaterialUsage.DiffuseDetail },
            { "detail_map_overlay", MaterialUsage.DiffuseDetail },
            { "change_color_map", MaterialUsage.ColourChange },
            { "bump_map", MaterialUsage.Normal },
            { "bump_detail_map", MaterialUsage.NormalDetail },
            { "self_illum_map", MaterialUsage.SelfIllumination },
            { "specular_map", MaterialUsage.Specular },
            { "foam_texture", MaterialUsage.Diffuse }
        };

        private static readonly Dictionary<string, TintUsage> tintLookup = new Dictionary<string, TintUsage>
        {
            { "albedo_color", TintUsage.Albedo },
            { "self_illum_color", TintUsage.SelfIllumination },
            { "specular_tint", TintUsage.Specular }
        };

        public static IEnumerable<GeometryMaterial> GetMaterials(IList<ShaderBlock> shaders)
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                var tag = shaders[i].ShaderReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = Utils.GetFileName(tag.FullPath)
                };

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var subMaterials = new List<ISubmaterial>();
                var template = shader.ShaderProperties[0].TemplateReference.Tag.ReadMetadata<render_method_template>();
                for (int j = 0; j < template.Usages.Count; j++)
                {
                    var usage = template.Usages[j].Value;
                    var entry = usageLookup.FirstOrNull(p => usage.StartsWith(p.Key));
                    if (!entry.HasValue)
                        continue;

                    var map = shader.ShaderProperties[0].ShaderMaps[j];
                    var bitmTag = map.BitmapReference.Tag;
                    if (bitmTag == null)
                        continue;

                    var tile = map.TilingIndex == byte.MaxValue
                        ? (RealVector4D?)null
                        : shader.ShaderProperties[0].TilingData[map.TilingIndex];

                    subMaterials.Add(new SubMaterial
                    {
                        Usage = entry.Value.Value,
                        Bitmap = bitmTag.ReadMetadata<bitmap>(),
                        Tiling = new RealVector2D(tile?.X ?? 1, tile?.Y ?? 1)
                    });
                }

                if (subMaterials.Count == 0)
                {
                    yield return material;
                    continue;
                }

                material.Submaterials = subMaterials;

                for (int j = 0; j < template.Arguments.Count; j++)
                {
                    if (!tintLookup.ContainsKey(template.Arguments[j].Value))
                        continue;

                    material.TintColours.Add(new TintColour
                    {
                        Usage = tintLookup[template.Arguments[j].Value],
                        R = (byte)(shader.ShaderProperties[0].TilingData[j].X * byte.MaxValue),
                        G = (byte)(shader.ShaderProperties[0].TilingData[j].Y * byte.MaxValue),
                        B = (byte)(shader.ShaderProperties[0].TilingData[j].Z * byte.MaxValue),
                        A = (byte)(shader.ShaderProperties[0].TilingData[j].W * byte.MaxValue),
                    });
                }

                if (tag.ClassCode == "rmtr")
                    material.Flags |= MaterialFlags.TerrainBlend;
                else if (tag.ClassCode != "rmsh")
                    material.Flags |= MaterialFlags.Transparent;

                if (subMaterials.Any(m => m.Usage == MaterialUsage.ColourChange) && !subMaterials.Any(m => m.Usage == MaterialUsage.Diffuse))
                    material.Flags |= MaterialFlags.ColourChange;

                yield return material;
            }
        }

        public static IEnumerable<GeometryMesh> GetMeshes(ICacheFile cache, ResourceIdentifier resourcePointer, IList<SectionBlock> sections, Func<SectionBlock, short?> boundsIndex)
        {
            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
            var entry = resourceGestalt.ResourceEntries[resourcePointer.ResourceIndex];
            using (var cacheReader = cache.CreateReader(cache.DefaultAddressTranslator))
            using (var reader = cacheReader.CreateVirtualReader(resourceGestalt.FixupDataPointer.Address))
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
                doc.LoadXml(Adjutant.Properties.Resources.HaloReachVertexBuffer);

                var lookup = doc.FirstChild.ChildNodes.Cast<XmlNode>()
                    .ToDictionary(n => Convert.ToInt32(n.Attributes["type"].Value, 16));

                foreach (var section in sections)
                {
                    if (section.VertexBufferIndex < 0 || section.IndexBufferIndex < 0)
                    {
                        yield return new GeometryMesh();
                        continue;
                    }

                    var node = lookup[section.VertexFormat];
                    var vInfo = vertexBufferInfo[section.VertexBufferIndex];
                    var iInfo = indexBufferInfo[section.IndexBufferIndex];

                    Func<XmlNode, string, bool> hasUsage = (n, u) =>
                    {
                        return n.ChildNodes.Cast<XmlNode>().Any(c => c.Attributes["usage"]?.Value == u);
                    };

                    var skinType = VertexWeights.None;
                    if (hasUsage(node, "blendindices"))
                        skinType = hasUsage(node, "blendweight") ? VertexWeights.Skinned : VertexWeights.Rigid;
                    else if (section.NodeIndex < byte.MaxValue)
                        skinType = VertexWeights.Rigid;

                    var mesh = new GeometryMesh
                    {
                        IndexFormat = iInfo.IndexFormat,
                        Vertices = new IVertex[vInfo.VertexCount],
                        VertexWeights = skinType,
                        NodeIndex = section.NodeIndex == byte.MaxValue ? (byte?)null : section.NodeIndex,
                        BoundsIndex = boundsIndex(section)
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
                    if (vInfo.VertexCount > ushort.MaxValue)
                        mesh.Indicies = reader.ReadEnumerable<int>(totalIndices).ToArray();
                    else mesh.Indicies = reader.ReadEnumerable<ushort>(totalIndices).Select(i => (int)i).ToArray();

                    yield return mesh;
                }
            }
        }
    }
}
