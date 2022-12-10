using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    internal static class Halo4Common
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
                var rmsh = s.MaterialReference.Tag?.ReadMetadata<material>();
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
                var tag = shaders[i].MaterialReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = Utils.GetFileName(tag.FullPath)
                };

                var shader = tag?.ReadMetadata<material>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var subMaterials = new List<ISubmaterial>();
                var props = shader.ShaderProperties[0];
                foreach (var map in props.ShaderMaps)
                {
                    var bitmTag = map.BitmapReference.Tag;
                    if (bitmTag == null)
                        continue;

                    MaterialUsage usage;
                    var name = bitmTag.FileName;
                    if (name.EndsWith("_detail_normal") || name.EndsWith("_detail_bump"))
                        usage = MaterialUsage.NormalDetail;
                    else if (name.EndsWith("_detail"))
                        usage = MaterialUsage.DiffuseDetail;
                    else if (name.EndsWith("_normal") || name.EndsWith("_bump"))
                        usage = MaterialUsage.Normal;
                    else if (name.EndsWith("_diff") || name.EndsWith("_color") || name.StartsWith("watersurface_"))
                        usage = MaterialUsage.Diffuse;
                    else if (props.ShaderMaps.Count == 1)
                        usage = MaterialUsage.Diffuse;
                    else
                        continue;

                    //maybe map.TilingIndex has the wrong offset? can sometimes be out of bounds (other than 0xFF)
                    var tile = props.TilingData.Cast<RealVector4D?>().ElementAtOrDefault(map.TilingIndex);

                    try
                    {
                        subMaterials.Add(new SubMaterial
                        {
                            Usage = usage,
                            Bitmap = bitmTag.ReadMetadata<bitmap>(),
                            Tiling = new RealVector2D(tile?.X ?? 1, tile?.Y ?? 1)
                        });
                    }
                    catch { }
                }

                if (subMaterials.Count == 0)
                {
                    yield return material;
                    continue;
                }

                material.Submaterials = subMaterials;

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

            var vertexBuilder = new XmlVertexBuilder(cache.Metadata.IsMcc ? Resources.MccHalo4VertexBuffer : Resources.Halo4VertexBuffer);
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
                        var indexBuffer = new IndexBuffer(data, vInfo.VertexCount > ushort.MaxValue ? typeof(int) : typeof(ushort));
                        ib.Add(section.IndexBufferIndex, indexBuffer);
                    }
                }
            }

            if (cache.Metadata.Architecture != PlatformArchitecture.x86)
            {
                foreach (var b in vb.Values)
                    b.SwapEndianness();
                foreach (var b in ib.Values)
                    b.SwapEndianness();
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
                    IndexFormat = indexBufferInfo[section.IndexBufferIndex].IndexFormat,
                    VertexWeights = VertexWeights.None,
                    NodeIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vb[section.VertexBufferIndex],
                    IndexBuffer = ib[section.IndexBufferIndex]
                };

                if (mesh.VertexBuffer.HasBlendIndices)
                    mesh.VertexWeights = mesh.VertexBuffer.HasBlendWeights ? VertexWeights.Skinned : VertexWeights.Rigid;
                else if (section.NodeIndex < byte.MaxValue)
                    mesh.VertexWeights = VertexWeights.Rigid;

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
