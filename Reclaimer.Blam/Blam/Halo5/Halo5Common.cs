using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    [FixedSize(80)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct VertexBufferInfo
    {
        [Offset(4)]
        public int VertexCount { get; set; }

        private readonly string GetDebuggerDisplay() => new { VertexCount }.ToString();
    }

    [FixedSize(72)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct IndexBufferInfo
    {
        [Offset(4)]
        public int IndexCount { get; set; }

        private readonly string GetDebuggerDisplay() => new { IndexCount }.ToString();
    }

    public class Halo5GeometryArgs
    {
        public Module Module { get; init; }
        public ResourcePackingPolicy ResourcePolicy { get; init; }
        public IReadOnlyList<RegionBlock> Regions { get; init; }
        public IReadOnlyList<MaterialBlock> Materials { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public int ResourceIndex { get; init; }
        public int ResourceCount { get; init; }
    }

    internal static class Halo5Common
    {
        public static IEnumerable<Material> GetMaterials(IReadOnlyList<MaterialBlock> materials)
        {
            for (var i = 0; i < materials?.Count; i++)
            {
                var tag = materials[i].MaterialReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = tag.GlobalTagId,
                    Name = tag.FileName
                };

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, tag.TagName);

                var mat = tag?.ReadMetadata<material>();
                if (mat == null)
                {
                    yield return material;
                    continue;
                }

                var map = mat.PostprocessDefinitions.FirstOrDefault()?.Textures.FirstOrDefault();
                var bitmTag = map?.BitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                //var tile = map.TilingIndex == byte.MaxValue
                //    ? (Vector4?)null
                //    : shader.ShaderProperties[0].TilingData[map.TilingIndex];

                try
                {
                    var texture = new Texture
                    {
                        Id = bitmTag.GlobalTagId,
                        ContentProvider = bitmTag.ReadMetadata<bitmap>()
                    };

                    texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, bitmTag.TagName);

                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Diffuse,
                        Tiling = Vector2.One,
                        Texture = texture
                    });
                }
                catch { }

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo5GeometryArgs args, out List<Material> materials)
        {
            //TODO: implement an LOD selector one day
            const int lod = 0;
            var lodFlag = (LodFlags)(1 << lod);

            var totalVertexBufferCount = 1 + args.Sections.SelectMany(s => s.SectionLods).Max(lod => lod.VertexBufferIndex);
            var totalIndexBufferCount = 1 + args.Sections.SelectMany(s => s.SectionLods).Max(lod => lod.IndexBufferIndex);

            var vertexBufferInfo = new List<VertexBufferInfo>(totalVertexBufferCount);
            var indexBufferInfo = new List<IndexBufferInfo>(totalIndexBufferCount);

            var rawVertexBuffers = new List<byte[]>(totalVertexBufferCount);
            var rawIndexBuffers = new List<byte[]>(totalIndexBufferCount);

            materials = new List<Material>(args.Materials?.Count ?? default);
            var meshList = new List<Mesh>(args.Sections.Count);

            var vb = new Dictionary<int, VertexBuffer>(totalVertexBufferCount);
            var ib = new Dictionary<int, IndexBuffer>(totalIndexBufferCount);

            if (args.ResourcePolicy == ResourcePackingPolicy.SingleResource)
            {
                if (args.ResourceCount > 1)
                    System.Diagnostics.Debugger.Break();

                AppendBufferData(args.ResourceIndex);
            }
            else
            {
                //when there is a 1:1 relationship between permutation and resource chunk,
                //the resource chunks are stored in the same order that the permutations appear in (ie chunk 0 is for permutation 0 and so on)
                //however they must be loaded in order of which mesh section they correspond to because the vertex/index buffer indicies are based on that order.
                var chunkSortLookup = args.Regions
                    .SelectMany(r => r.Permutations)
                    .Select((p, i) => (ResourceIndex: args.ResourceIndex + i, p.SectionIndex))
                    .ToDictionary(t => t.ResourceIndex, t => t.SectionIndex);

                foreach (var resourceIndex in Enumerable.Range(args.ResourceIndex, args.ResourceCount).OrderBy(i => chunkSortLookup[i]))
                    AppendBufferData(resourceIndex);
            }

            var vertexBuilder = new XmlVertexBuilder(Resources.Halo5VertexBuffer);
            foreach (var section in args.Sections)
            {
                if (section.SectionLods[0].LodFlags > 0 && (section.SectionLods[0].LodFlags & lodFlag) == 0)
                    continue;

                var lodData = section.SectionLods[Math.Min(lod, section.SectionLods.Count - 1)];

                var vInfo = vertexBufferInfo.ElementAtOrDefault(lodData.VertexBufferIndex);
                var iInfo = indexBufferInfo.ElementAtOrDefault(lodData.IndexBufferIndex);

                if (vInfo.VertexCount == 0) // || iInfo.IndexCount == 0)
                    continue;

                try
                {
                    if (!vb.ContainsKey(lodData.VertexBufferIndex))
                    {
                        var data = rawVertexBuffers[lodData.VertexBufferIndex];
                        var vertexBuffer = vertexBuilder.CreateVertexBuffer(section.VertexFormat, vInfo.VertexCount, data);
                        if (section.VertexFormat != 38) //skinned 8 weights
                            vertexBuffer.HasImpliedBlendWeights = true;
                        vb.Add(lodData.VertexBufferIndex, vertexBuffer);
                    }

                    if (!ib.ContainsKey(lodData.IndexBufferIndex))
                    {
                        // if this is a particle model, then we need to create our own index buffer
                        if (iInfo.IndexCount == 0) // if particle vertices
                        {
                            var indexBuffer = BlamUtils.CreateDecoratorIndexBuffer(vInfo.VertexCount);
                            ib.Add(lodData.IndexBufferIndex, indexBuffer);
                        }
                        else // otherwise regular index buffer
                        {
                            var data = rawIndexBuffers[lodData.IndexBufferIndex];
                            var indexBuffer = new IndexBuffer(data, vInfo.VertexCount > ushort.MaxValue ? typeof(int) : typeof(ushort)) { Layout = section.IndexFormat };
                            ib.Add(lodData.IndexBufferIndex, indexBuffer);
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            var matLookup = materials;
            materials.AddRange(GetMaterials(args.Materials));

            meshList.AddRange(args.Sections.Select((section, sectionIndex) =>
            {
                var lodData = section.SectionLods[Math.Min(lod, section.SectionLods.Count - 1)];
                if (!vb.ContainsKey(lodData.VertexBufferIndex) || !ib.ContainsKey(lodData.IndexBufferIndex))
                    return null;

                var mesh = new Mesh
                {
                    BoneIndex = section.NodeIndex == byte.MaxValue ? null : section.NodeIndex,
                    VertexBuffer = vb[lodData.VertexBufferIndex],
                    IndexBuffer = ib[lodData.IndexBufferIndex]
                };

                // have alternate function for if there are no defined parts, where we just do the whole buffer
                if (lodData.Submeshes.Count > 0)
                {
                    mesh.Segments.AddRange(
                        lodData.Submeshes.Select(s => new MeshSegment
                        {
                            Material = matLookup.ElementAtOrDefault(s.ShaderIndex),
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        })
                    );
                }
                else
                {
                    //create an implied submesh that covers all indices
                    mesh.Segments.Add(
                        new MeshSegment
                        {
                            Material = matLookup.ElementAtOrDefault(0),
                            IndexStart = 0,
                            IndexLength = mesh.IndexBuffer.Count
                        }
                    );
                }

                return mesh;
            }));

            return meshList;

            void AppendBufferData(int resourceIndex)
            {
                var itemIndex = args.Module.Resources[resourceIndex];
                var resource = args.Module.Items[itemIndex]; //this will be the [mesh resource!*] tag
                if (resource.ResourceCount > 0)
                    System.Diagnostics.Debugger.Break();

                using (var blockReader = resource.CreateReader())
                {
                    int vertexBufferCount, indexBufferCount;

                    var header = new MetadataHeader(blockReader);
                    using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(1)))
                    {
                        //DataBlock 0: mostly padding, buffer counts
                        //DataBlock 1: vertex buffer infos
                        //DataBlock 2: index buffer infos // may not be present if there are no additional index data blocks
                        //DataBlock 3+: additional vertex data block for each buffer
                        //DataBlock n+: additional index data block for each buffer

                        var block = header.DataBlocks[0];
                        reader.Seek(block.Offset + 16, SeekOrigin.Begin);
                        vertexBufferCount = reader.ReadInt32();
                        reader.Seek(block.Offset + 44, SeekOrigin.Begin);
                        indexBufferCount = reader.ReadInt32();

                        if (vertexBufferCount == 0 && indexBufferCount == 0)
                            return;
                        else if (vertexBufferCount == 0) // || indexBufferCount == 0)
                            System.Diagnostics.Debugger.Break(); //decorators have vertices but no indices

                        block = header.DataBlocks[1];
                        reader.Seek(block.Offset, SeekOrigin.Begin);
                        vertexBufferInfo.AddRange(reader.ReadArray<VertexBufferInfo>(vertexBufferCount));

                        if (indexBufferCount > 0)
                        {
                            block = header.DataBlocks[2];
                            reader.Seek(block.Offset, SeekOrigin.Begin);
                            indexBufferInfo.AddRange(reader.ReadArray<IndexBufferInfo>(indexBufferCount));
                        }
                    }

                    //if there was no index buffer then the vertex buffer blocks will start one index earlier
                    var vertexBufferStart = indexBufferCount == 0 ? 2 : 3;
                    var indexBufferStart = vertexBufferStart + vertexBufferCount;

                    using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(2)))
                    {
                        foreach (var blockIndex in Enumerable.Range(vertexBufferStart, vertexBufferCount))
                        {
                            var block = header.DataBlocks[blockIndex];
                            reader.Seek(block.Offset, SeekOrigin.Begin);
                            rawVertexBuffers.Add(reader.ReadBytes(block.Size));
                        }

                        foreach (var blockIndex in Enumerable.Range(indexBufferStart, indexBufferCount))
                        {
                            var block = header.DataBlocks[blockIndex];
                            reader.Seek(block.Offset, SeekOrigin.Begin);
                            rawIndexBuffers.Add(reader.ReadBytes(block.Size));
                        }
                    }
                }
            }
        }
    }
}
