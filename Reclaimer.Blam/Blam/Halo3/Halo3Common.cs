using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Halo3
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

    public class Halo3GeometryArgs
    {
        public ICacheFile Cache { get; init; }
        public IReadOnlyList<ShaderBlock> Shaders { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public ResourceIdentifier ResourcePointer { get; init; }
    }

    internal static class Halo3Common
    {
        private static readonly Regex UsageRegex = new Regex(@"^(\w+?)(?:_m_(\d))?$");

        public static IEnumerable<Material> GetMaterials(IReadOnlyList<ShaderBlock> shaders)
        {
            foreach (var tag in shaders.Select(s => s.ShaderReference.Tag))
            {
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = tag.Id,
                    Name = tag.FileName
                };

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var props = shader.ShaderProperties[0];
                var template = props.TemplateReference.Tag.ReadMetadata<render_method_template>();

                var textureParams = from index in Enumerable.Range(0, template.Usages.Count)
                                    let usage = template.Usages[index]
                                    let tileIndex = template.Arguments.IndexOf(usage)
                                    let match = UsageRegex.Match(usage)
                                    where ShaderParameters.UsageLookup.ContainsKey(match.Groups[1].Value)
                                    select new
                                    {
                                        Usage = match.Groups[1].Value,
                                        BlendChannel = match.Groups[2].Success ? (ChannelMask)(1 << int.Parse(match.Groups[2].Value)) : default,
                                        props.ShaderMaps[index].BitmapReference.Tag,
                                        TileData = tileIndex >= 0 ? props.TilingData[tileIndex] : new RealVector4(1, 1, 1, 1),
                                    };

                var floatParams = from index in Enumerable.Range(0, template.Arguments.Count)
                                  let usage = template.Arguments[index]
                                  where !template.Usages.Contains(usage)
                                  let match = UsageRegex.Match(usage)
                                  where ShaderParameters.TintLookup.ContainsKey(match.Groups[1].Value)
                                  select new
                                  {
                                      Usage = match.Groups[1].Value,
                                      BlendChannel = match.Groups[2].Success ? (ChannelMask)(1 << int.Parse(match.Groups[2].Value)) : default,
                                      Value = props.TilingData[index]
                                  };

                foreach (var texParam in textureParams)
                {
                    if (texParam.Tag == null)
                        continue;

                    var bitmap = texParam.Tag.ReadMetadata<bitmap>();
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = ShaderParameters.UsageLookup[texParam.Usage],
                        Tiling = new Vector2(texParam.TileData.X, texParam.TileData.Y),
                        BlendChannel = texParam.BlendChannel,
                        Texture = new Texture
                        {
                            Id = texParam.Tag.Id,
                            Name = texParam.Tag.TagName,
                            Gamma = bitmap.Bitmaps[0].Curve switch
                            {
                                ColorSpace.Linear => 1f,
                                ColorSpace.Gamma2 => 2f,
                                ColorSpace.sRGB => 2.2f,
                                _ => 1.95f //xRGB, Unknown
                            },
                            GetDds = () => texParam.Tag.ReadMetadata<bitmap>().ToDds(0)
                        }
                    });
                }

                foreach (var floatParam in floatParams)
                {
                    material.Tints.Add(new MaterialTint
                    {
                        Usage = ShaderParameters.TintLookup[floatParam.Usage],
                        BlendChannel = floatParam.BlendChannel,
                        Color = floatParam.Value.ToArgb()
                    });
                }

                if (tag.ClassCode == "rmtr")
                    material.Flags |= (int)MaterialFlags.TerrainBlend;
                else if (tag.ClassCode != "rmsh")
                    material.Flags |= (int)MaterialFlags.Transparent;

                if (material.TextureMappings.Any(m => m.Usage == MaterialUsage.ColorChange) && !material.TextureMappings.Any(m => m.Usage == MaterialUsage.Diffuse))
                    material.Flags |= (int)MaterialFlags.ColourChange;

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo3GeometryArgs args, out List<Material> materials)
        {
            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var resourceGestalt = args.Cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
            var entry = resourceGestalt.ResourceEntries[args.ResourcePointer.ResourceIndex];
            using (var cacheReader = args.Cache.CreateReader(args.Cache.DefaultAddressTranslator))
            using (var reader = cacheReader.CreateVirtualReader(resourceGestalt.FixupDataPointer.Address))
            {
                reader.Seek(entry.FixupOffset + (entry.FixupSize - 24), SeekOrigin.Begin);
                var vertexBufferCount = reader.ReadInt32();
                reader.Seek(8, SeekOrigin.Current);
                var indexBufferCount = reader.ReadInt32();

                reader.Seek(entry.FixupOffset, SeekOrigin.Begin);
                vertexBufferInfo = reader.ReadArray<VertexBufferInfo>(vertexBufferCount);
                reader.Seek(12 * vertexBufferCount, SeekOrigin.Current); //12 byte struct here for each vertex buffer
                indexBufferInfo = reader.ReadArray<IndexBufferInfo>(indexBufferCount);
                //12 byte struct here for each index buffer
                //4x 12 byte structs here
            }

            var vertexBuilder = new XmlVertexBuilder(args.Cache.Metadata.IsMcc ? Resources.MccHalo3VertexBuffer : Resources.Halo3VertexBuffer);
            var vb = new Dictionary<int, VertexBuffer>();
            var ib = new Dictionary<int, IndexBuffer>();

            using (var ms = new MemoryStream(args.ResourcePointer.ReadData(PageType.Auto)))
            using (var reader = new EndianReader(ms, args.Cache.ByteOrder))
            {
                foreach (var section in args.Sections)
                {
                    var vInfo = vertexBufferInfo.ElementAtOrDefault(section.VertexBufferIndex);
                    var iInfo = indexBufferInfo.ElementAtOrDefault(section.IndexBufferIndex);

                    if (vInfo.VertexCount == 0 || iInfo.DataLength == 0)
                        continue;

                    var address = entry.ResourceFixups[section.VertexBufferIndex].Offset & 0x0FFFFFFF;
                    if (!vb.ContainsKey(section.VertexBufferIndex))
                    {
                        reader.Seek(address, SeekOrigin.Begin);
                        var data = reader.ReadBytes(vInfo.DataLength);
                        var vertexBuffer = vertexBuilder.CreateVertexBuffer(section.VertexFormat, vInfo.VertexCount, data);
                        vb.Add(section.VertexBufferIndex, vertexBuffer);
                    }

                    address = entry.ResourceFixups[vertexBufferInfo.Length * 2 + section.IndexBufferIndex].Offset & 0x0FFFFFFF;
                    if (!ib.ContainsKey(section.IndexBufferIndex))
                    {
                        reader.Seek(address, SeekOrigin.Begin);
                        var data = reader.ReadBytes(iInfo.DataLength);
                        var indexBuffer = new IndexBuffer(data, vInfo.VertexCount > ushort.MaxValue ? typeof(int) : typeof(ushort)) { Layout = indexBufferInfo[section.IndexBufferIndex].IndexFormat };
                        ib.Add(section.IndexBufferIndex, indexBuffer);
                    }
                }
            }

            if (args.Cache.Metadata.Architecture != PlatformArchitecture.x86)
            {
                foreach (var b in vb.Values)
                    b.ReverseEndianness();
                foreach (var b in ib.Values)
                    b.ReverseEndianness();
            }

            var matLookup = materials = new List<Material>(args.Shaders.Count);
            materials.AddRange(GetMaterials(args.Shaders));

            var meshList = new List<Mesh>(args.Sections.Count);
            meshList.AddRange(args.Sections.Select((section, sectionIndex) =>
            {
                if (!vb.ContainsKey(section.VertexBufferIndex) || !ib.ContainsKey(section.IndexBufferIndex))
                    return null;

                var mesh = new Mesh
                {
                    BoneIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vb[section.VertexBufferIndex],
                    IndexBuffer = ib[section.IndexBufferIndex]
                };

                mesh.Segments.AddRange(
                    section.Submeshes.Select(s => new MeshSegment
                    {
                        Material = matLookup.ElementAtOrDefault(s.ShaderIndex),
                        IndexStart = s.IndexStart,
                        IndexLength = s.IndexLength
                    })
                );

                if (args.NodeMaps != null)
                {
                    UByte4 MapNodeIndices(UByte4 source)
                    {
                        var indices = args.NodeMaps.ElementAtOrDefault(sectionIndex)?.Indices.Cast<byte?>();
                        return new UByte4
                        {
                            X = indices?.ElementAtOrDefault(source.X) ?? source.X,
                            Y = indices?.ElementAtOrDefault(source.Y) ?? source.Y,
                            Z = indices?.ElementAtOrDefault(source.Z) ?? source.Z,
                            W = indices?.ElementAtOrDefault(source.W) ?? source.W
                        };
                    }

                    foreach (var v in mesh.VertexBuffer.BlendIndexChannels)
                    {
                        var buf = v as VectorBuffer<UByte4>;
                        for (var i = 0; i < v.Count; i++)
                            buf[i] = MapNodeIndices(buf[i]);
                    }
                }

                return mesh;
            }));

            return meshList;
        }
    }
}
