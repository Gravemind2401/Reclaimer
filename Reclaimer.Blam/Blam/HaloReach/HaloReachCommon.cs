using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

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

    public class HaloReachGeometryArgs
    {
        public ICacheFile Cache { get; init; }
        public IReadOnlyList<ShaderBlock> Shaders { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public ResourceIdentifier ResourcePointer { get; init; }
    }

    internal static class HaloReachCommon
    {
        public static IEnumerable<Material> GetMaterials(IReadOnlyList<ShaderBlock> shaders)
        {
            for (var i = 0; i < shaders.Count; i++)
            {
                var tag = shaders[i].ShaderReference.Tag;
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
                for (var j = 0; j < template.Usages.Count; j++)
                {
                    var usage = template.Usages[j].Value;
                    var matUsage = ShaderParameters.UsageLookup.FirstOrNull(p => usage.StartsWith(p.Key))?.Value;
                    if (matUsage == null)
                        continue;

                    var map = props.ShaderMaps[j];
                    var bitmTag = map.BitmapReference.Tag;
                    if (bitmTag == null)
                        continue;

                    var tile = map.TilingIndex >= props.TilingData.Count
                        ? (RealVector4?)null
                        : props.TilingData[map.TilingIndex];

                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = matUsage,
                        Tiling = new Vector2(tile?.X ?? 1, tile?.Y ?? 1),
                        Texture = new Texture
                        {
                            Id = bitmTag.Id,
                            ContentProvider = bitmTag.ReadMetadata<bitmap>()
                        }
                    });
                }

                for (var j = 0; j < template.Arguments.Count; j++)
                {
                    if (!ShaderParameters.TintLookup.TryGetValue(template.Arguments[j].Value, out var tintUsage))
                        continue;

                    material.Tints.Add(new MaterialTint
                    {
                        Usage = tintUsage,
                        Color = System.Drawing.Color.FromArgb(
                            (byte)(props.TilingData[j].W * byte.MaxValue),
                            (byte)(props.TilingData[j].X * byte.MaxValue),
                            (byte)(props.TilingData[j].Y * byte.MaxValue),
                            (byte)(props.TilingData[j].Z * byte.MaxValue)
                        )
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

        public static List<Mesh> GetMeshes(HaloReachGeometryArgs args, out List<Material> materials)
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

            var vertexBuilder = new XmlVertexBuilder(args.Cache.Metadata.IsMcc ? Resources.MccHaloReachVertexBuffer : Resources.HaloReachVertexBuffer);
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
                    if (section.IndexBufferIndex >= 0 && !ib.ContainsKey(section.IndexBufferIndex))
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
