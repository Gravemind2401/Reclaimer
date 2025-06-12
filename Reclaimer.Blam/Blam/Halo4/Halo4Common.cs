using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo4
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

    public class Halo4GeometryArgs
    {
        public ICacheFile Cache { get; init; }
        public IReadOnlyList<ShaderBlock> Shaders { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public ResourceIdentifier ResourcePointer { get; init; }
    }

    internal static class Halo4Common
    {
        public static IEnumerable<Material> GetMaterials(Halo4GeometryArgs args)
        {
            var bitmapCache = new Dictionary<int, bitmap>();
            var materialCache = new Dictionary<int, Material>();

            for (var i = 0; i < args.Shaders.Count; i++)
            {
                var tag = args.Shaders[i].MaterialReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                if (materialCache.TryGetValue(tag.Id, out var material))
                {
                    yield return material;
                    continue;
                }

                materialCache.Add(tag.Id, material = new Material
                {
                    Id = tag.Id,
                    Name = tag.FileName
                });

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, tag.TagName);

                var shader = tag?.ReadMetadata<material>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                if (!MaterialHelper.PopulateTextureMappings(args, bitmapCache, material, shader))
                {
                    //legacy method: guess texture purpose based on file name
                    var props = shader.ShaderProperties[0];
                    foreach (var map in props.ShaderMaps)
                    {
                        var bitmTag = map.BitmapReference.Tag;
                        if (bitmTag == null)
                            continue;

                        string usage;
                        var name = bitmTag.FileName;
                        if (name.EndsWith("_detail_normal") || name.EndsWith("_detail_bump"))
                            usage = TextureUsage.NormalDetail;
                        else if (name.EndsWith("_detail"))
                            usage = TextureUsage.DiffuseDetail;
                        else if (name.EndsWith("_normal") || name.EndsWith("_bump"))
                            usage = TextureUsage.Normal;
                        else if (name.EndsWith("_diff") || name.EndsWith("_color") || name.StartsWith("watersurface_"))
                            usage = TextureUsage.Diffuse;
                        else if (props.ShaderMaps.Count == 1)
                            usage = TextureUsage.Diffuse;
                        else
                            continue;

                        var tile = props.TilingData[props.ShaderMaps.IndexOf(map)];

                        try
                        {
                            var texture = new Texture
                            {
                                Id = bitmTag.Id,
                                ContentProvider = bitmTag.ReadMetadata<bitmap>()
                            };

                            texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, bitmTag.TagName);

                            material.TextureMappings.Add(new TextureMapping
                            {
                                Usage = usage,
                                Tiling = new Vector2(tile.X, tile.Y),
                                Texture = texture
                            });
                        }
                        catch { }
                    }
                }

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo4GeometryArgs args, out List<Material> materials)
        {
            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var resourceGestalt = args.Cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
            var entry = resourceGestalt.ResourceEntries[args.ResourcePointer.ResourceIndex];
            using (var cacheReader = args.Cache.CreateReader(args.Cache.DefaultAddressTranslator))
            using (var reader = cacheReader.CreateVirtualReader(resourceGestalt.FixupDataPointer.Address))
            {
                var fixupOffset = entry.FixupOffsets.FirstOrDefault();
                reader.Seek(fixupOffset + (entry.FixupSize - 24), SeekOrigin.Begin);
                var vertexBufferCount = reader.ReadInt32();
                reader.Seek(8, SeekOrigin.Current);
                var indexBufferCount = reader.ReadInt32();

                reader.Seek(fixupOffset, SeekOrigin.Begin);
                vertexBufferInfo = reader.ReadArray<VertexBufferInfo>(vertexBufferCount);
                reader.Seek(12 * vertexBufferCount, SeekOrigin.Current); //12 byte struct here for each vertex buffer
                indexBufferInfo = reader.ReadArray<IndexBufferInfo>(indexBufferCount);
                //12 byte struct here for each index buffer
                //4x 12 byte structs here
            }

            var vertexBuilder = new XmlVertexBuilder(args.Cache.Metadata.IsMcc ? Resources.MccHalo4VertexBuffer : Resources.Halo4VertexBuffer);
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
                foreach (var (_, b) in ib.Where(kv => kv.Key >= 0)) //skip negative keys since those are the implied buffers
                    b.ReverseEndianness();
            }

            var matLookup = materials = new List<Material>(args.Shaders.Count);
            materials.AddRange(GetMaterials(args));

            //since different sections can share the same vertex buffer, we need to track which ones have
            //already had the node mapping processed so it doesnt get processed twice and screw up the indices
            var nodeMappedVertexBuffers = new HashSet<int>();

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

                if (!mesh.VertexBuffer.HasColors && mesh.VertexBuffer.PositionChannels.ElementAtOrDefault(0) is IVectorBuffer b && b.Dimensions > 3)
                    mesh.Flags |= Geometry.MeshFlags.VertexColorFromPosition;

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

                if (args.NodeMaps != null && !nodeMappedVertexBuffers.Contains(section.VertexBufferIndex))
                {
                    nodeMappedVertexBuffers.Add(section.VertexBufferIndex);
                    var indices = args.NodeMaps.ElementAtOrDefault(sectionIndex)?.Indices.Cast<byte?>();

                    UByte4 MapNodeIndices(UByte4 source)
                    {
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
