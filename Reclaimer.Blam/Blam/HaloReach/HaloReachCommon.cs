using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
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
            var definitions = new Dictionary<int, render_method_definition>();
            var bitmaps = new Dictionary<int, bitmap>();

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

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, tag.TagName);

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                if (!definitions.TryGetValue(shader.RenderMethodDefinitionReference.TagId, out var rmdf))
                    definitions.Add(shader.RenderMethodDefinitionReference.TagId, rmdf = shader.RenderMethodDefinitionReference.Tag.ReadMetadata<render_method_definition>());

                var shaderOptions = (from t in rmdf.Categories.Zip(shader.ShaderOptions)
                                     where t.Second.OptionIndex >= 0 && t.Second.OptionIndex < t.First.Options.Count
                                     select new
                                     {
                                         Category = t.First.Name.Value,
                                         Option = t.First.Options[t.Second.OptionIndex].Name.Value
                                     }).ToDictionary(o => o.Category, o => o.Option);

                var props = shader.ShaderProperties[0];
                var template = props.TemplateReference.Tag.ReadMetadata<render_method_template>();

                Gen3MaterialHelper.PopulateTextureMappings(bitmaps, material, tag.ClassCode, shaderOptions, template.Usages, template.Arguments, props.TilingData, i => props.ShaderMaps[i].BitmapReference.Tag);

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
                foreach (var (section, sectionIndex) in args.Sections.Select((s, i) => (s, i)))
                {
                    var vInfo = vertexBufferInfo.ElementAtOrDefault(section.VertexBufferIndex);
                    var iInfo = indexBufferInfo.ElementAtOrDefault(section.IndexBufferIndex);

                    if (vInfo.VertexCount == 0)
                        continue;

                    var address = entry.ResourceFixups[section.VertexBufferIndex].Offset & 0x0FFFFFFF;
                    if (!vb.ContainsKey(section.VertexBufferIndex))
                    {
                        reader.Seek(address, SeekOrigin.Begin);
                        var data = reader.ReadBytes(vInfo.DataLength);
                        var vertexBuffer = vertexBuilder.CreateVertexBuffer(section.VertexFormat, vInfo.VertexCount, data);
                        vb.Add(section.VertexBufferIndex, vertexBuffer);
                    }

                    if (section.IsUnindexed)
                    {
                        var indexBuffer = BlamUtils.CreateImpliedIndexBuffer(vInfo.VertexCount, section.IndexFormat);
                        ib.Add(-sectionIndex - 1, indexBuffer); //use negative sectionIndex to ensure it doesnt conflict with actual buffer indexes and is unique per mesh
                    }
                    else
                    {
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
                var indexBufferKey = section.IsUnindexed ? -sectionIndex - 1 : section.IndexBufferIndex;
                if (!vb.TryGetValue(section.VertexBufferIndex, out var vertexBuffer) || !ib.TryGetValue(indexBufferKey, out var indexBuffer))
                    return null;

                var mesh = new Mesh
                {
                    BoneIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vertexBuffer,
                    IndexBuffer = indexBuffer
                };

                mesh.CustomProperties.AddFromFlags(section.Flags);

                mesh.Segments.AddRange(
                    section.Submeshes.Select(s =>
                    {
                        var segment = new MeshSegment
                        {
                            Material = matLookup.ElementAtOrDefault(s.ShaderIndex),
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        };

                        segment.CustomProperties.AddFromFlags(s.Flags);
                        return segment;
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
