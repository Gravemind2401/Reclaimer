using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.HaloReach
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
        public static IEnumerable<IBitmap> GetBitmaps(IReadOnlyList<ShaderBlock> shaders) => GetBitmaps(shaders, Enumerable.Range(0, shaders?.Count ?? 0));
        public static IEnumerable<IBitmap> GetBitmaps(IReadOnlyList<ShaderBlock> shaders, IEnumerable<int> shaderIndexes)
        {
            var selection = shaderIndexes?.Distinct().Where(i => i >= 0 && i < shaders?.Count).Select(i => shaders[i]);
            if (selection?.Any() != true)
                yield break;

            var complete = new List<int>();
            foreach (var s in selection)
            {
                var rmsh = s.ShaderReference.Tag?.ReadMetadata<shader>();
                if (rmsh == null)
                    continue;

                foreach (var map in rmsh.ShaderProperties.SelectMany(p => p.ShaderMaps))
                {
                    if (map.BitmapReference.Tag == null || complete.Contains(map.BitmapReference.TagId))
                        continue;

                    complete.Add(map.BitmapReference.TagId);
                    yield return map.BitmapReference.Tag.ReadMetadata<bitmap>();
                }
            }
        }

        public static IEnumerable<GeometryMaterial> GetMaterials(IReadOnlyList<ShaderBlock> shaders)
        {
            for (var i = 0; i < shaders.Count; i++)
            {
                var tag = shaders[i].ShaderReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = tag.FileName
                };

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var subMaterials = new List<ISubmaterial>();
                var props = shader.ShaderProperties[0];
                var template = props.TemplateReference.Tag.ReadMetadata<render_method_template>();
                for (var j = 0; j < template.Usages.Count; j++)
                {
                    var usage = template.Usages[j].Value;
                    var entry = BlamConstants.Gen3Materials.UsageLookup.FirstOrNull(p => usage.StartsWith(p.Key));
                    if (!entry.HasValue)
                        continue;

                    var map = props.ShaderMaps[j];
                    var bitmTag = map.BitmapReference.Tag;
                    if (bitmTag == null)
                        continue;

                    var tile = map.TilingIndex >= props.TilingData.Count
                        ? (RealVector4?)null
                        : props.TilingData[map.TilingIndex];

                    subMaterials.Add(new SubMaterial
                    {
                        Usage = entry.Value.Value,
                        Bitmap = bitmTag.ReadMetadata<bitmap>(),
                        Tiling = new RealVector2(tile?.X ?? 1, tile?.Y ?? 1)
                    });
                }

                if (subMaterials.Count == 0)
                {
                    yield return material;
                    continue;
                }

                material.Submaterials = subMaterials;

                for (var j = 0; j < template.Arguments.Count; j++)
                {
                    if (!BlamConstants.Gen3Materials.TintLookup.TryGetValue(template.Arguments[j].Value, out var tintUsage))
                        continue;

                    material.TintColours.Add(new TintColour
                    {
                        Usage = tintUsage,
                        R = (byte)(props.TilingData[j].X * byte.MaxValue),
                        G = (byte)(props.TilingData[j].Y * byte.MaxValue),
                        B = (byte)(props.TilingData[j].Z * byte.MaxValue),
                        A = (byte)(props.TilingData[j].W * byte.MaxValue),
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

        public static IEnumerable<GeometryMesh> GetMeshes(ICacheFile cache, ResourceIdentifier resourcePointer, IEnumerable<SectionBlock> sections, Action<SectionBlock, GeometryMesh> setProps, Func<int, int, int> mapNode = null)
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
                vertexBufferInfo = reader.ReadArray<VertexBufferInfo>(vertexBufferCount);
                reader.Seek(12 * vertexBufferCount, SeekOrigin.Current); //12 byte struct here for each vertex buffer
                indexBufferInfo = reader.ReadArray<IndexBufferInfo>(indexBufferCount);
                //12 byte struct here for each index buffer
                //4x 12 byte structs here
            }

            var vertexBuilder = new XmlVertexBuilder(cache.Metadata.IsMcc ? Resources.MccHaloReachVertexBuffer : Resources.HaloReachVertexBuffer);
            var vb = new Dictionary<int, VertexBuffer>();
            var ib = new Dictionary<int, IndexBuffer>();

            using (var ms = new MemoryStream(resourcePointer.ReadData(PageType.Auto)))
            using (var reader = new EndianReader(ms, cache.ByteOrder))
            {
                foreach (var section in sections)
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
                    if (section.IndexBufferIndex >= 0 && !ib.ContainsKey(section.IndexBufferIndex))
                    {
                        reader.Seek(address, SeekOrigin.Begin);
                        var data = reader.ReadBytes(iInfo.DataLength);
                        var indexBuffer = new IndexBuffer(data, vInfo.VertexCount > ushort.MaxValue ? typeof(int) : typeof(ushort)) { Layout = indexBufferInfo[section.IndexBufferIndex].IndexFormat };
                        ib.Add(section.IndexBufferIndex, indexBuffer);
                    }
                }
            }

            if (cache.Metadata.Architecture != PlatformArchitecture.x86)
            {
                foreach (var b in vb.Values)
                    b.ReverseEndianness();
                foreach (var b in ib.Values)
                    b.ReverseEndianness();
            }

            var sectionIndex = -1;
            foreach (var section in sections)
            {
                sectionIndex++;

                if (!vb.ContainsKey(section.VertexBufferIndex) || !ib.ContainsKey(section.IndexBufferIndex))
                {
                    yield return new GeometryMesh();
                    continue;
                }

                var mesh = new GeometryMesh
                {
                    NodeIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vb[section.VertexBufferIndex],
                    IndexBuffer = ib[section.IndexBufferIndex]
                };

                setProps(section, mesh);

                mesh.Submeshes.AddRange(
                    section.Submeshes.Select(s => new GeometrySubmesh
                    {
                        MaterialIndex = s.ShaderIndex,
                        IndexStart = s.IndexStart,
                        IndexLength = s.IndexLength
                    })
                );

                if (mapNode != null && (mesh.VertexWeights == VertexWeights.Skinned || mesh.VertexWeights == VertexWeights.Rigid))
                {
                    foreach (var v in mesh.VertexBuffer.BlendIndexChannels)
                    {
                        var buf = v as VectorBuffer<UByte4>;
                        for (var i = 0; i < v.Count; i++)
                        {
                            var bi = buf[i];
                            buf[i] = new UByte4
                            {
                                X = (byte)mapNode(sectionIndex, bi.X),
                                Y = (byte)mapNode(sectionIndex, bi.Y),
                                Z = (byte)mapNode(sectionIndex, bi.Z),
                                W = (byte)mapNode(sectionIndex, bi.W)
                            };
                        }
                    }
                }

                yield return mesh;
            }
        }
    }
}
