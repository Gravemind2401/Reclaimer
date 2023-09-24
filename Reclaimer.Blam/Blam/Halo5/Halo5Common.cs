using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public class Halo5GeometryArgs
    {
        public Module Module { get; init; }
        public IReadOnlyList<MaterialBlock> Materials { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public int ResourceIndex { get; init; }
    }

    internal static class Halo5Common
    {
        public static IEnumerable<Material> GetMaterials(IReadOnlyList<MaterialBlock> materials)
        {
            for (var i = 0; i < materials.Count; i++)
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
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = (int)MaterialUsage.Diffuse,
                        Tiling = Vector2.One,
                        Texture = new Texture
                        {
                            Id = bitmTag.GlobalTagId,
                            Name = bitmTag.FileName,
                            GetDds = () => bitmTag.ReadMetadata<bitmap>().ToDds(0)
                        }
                    });
                }
                catch { }

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo5GeometryArgs args, out List<Material> materials)
        {
            const int lod = 0;

            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var resourceIndex = args.Module.Resources[args.ResourceIndex];
            var resource = args.Module.Items[resourceIndex]; //this will be the [mesh resource!*] tag
            if (resource.ResourceCount > 0)
                System.Diagnostics.Debugger.Break();

            using (var blockReader = resource.CreateReader())
            {
                var header = new MetadataHeader(blockReader);
                using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(1)))
                {
                    //DataBlock 0: mostly padding, buffer counts
                    //DataBlock 1: vertex buffer infos
                    //DataBlock 2: index buffer infos
                    //DataBlock 3+: additional vertex data block for each buffer
                    //DataBlock n+: additional index data block for each buffer

                    var block = header.DataBlocks[0];
                    reader.Seek(block.Offset + 16, SeekOrigin.Begin);
                    var vertexBufferCount = reader.ReadInt32();
                    reader.Seek(block.Offset + 44, SeekOrigin.Begin);
                    var indexBufferCount = reader.ReadInt32();

                    block = header.DataBlocks[1];
                    reader.Seek(block.Offset, SeekOrigin.Begin);
                    vertexBufferInfo = reader.ReadArray<VertexBufferInfo>(vertexBufferCount);

                    block = header.DataBlocks[2];
                    reader.Seek(block.Offset, SeekOrigin.Begin);
                    indexBufferInfo = reader.ReadArray<IndexBufferInfo>(indexBufferCount);
                }

                var vertexBuilder = new XmlVertexBuilder(Resources.Halo5VertexBuffer);
                var vb = new Dictionary<int, VertexBuffer>();
                var ib = new Dictionary<int, IndexBuffer>();

                using (var reader = blockReader.CreateVirtualReader(header.GetSectionOffset(2)))
                {
                    foreach (var section in args.Sections)
                    {
                        var lodData = section.SectionLods[Math.Min(lod, section.SectionLods.Count - 1)];

                        var vInfo = vertexBufferInfo.ElementAtOrDefault(lodData.VertexBufferIndex);
                        var iInfo = indexBufferInfo.ElementAtOrDefault(lodData.IndexBufferIndex);

                        if (vInfo.VertexCount == 0 || iInfo.IndexCount == 0)
                            continue;

                        try
                        {
                            if (!vb.ContainsKey(lodData.VertexBufferIndex))
                            {
                                var block = header.DataBlocks[3 + lodData.VertexBufferIndex];
                                reader.Seek(block.Offset, SeekOrigin.Begin);
                                var data = reader.ReadBytes(block.Size);
                                var vertexBuffer = vertexBuilder.CreateVertexBuffer(section.VertexFormat, vInfo.VertexCount, data);
                                vb.Add(lodData.VertexBufferIndex, vertexBuffer);
                            }

                            if (!ib.ContainsKey(lodData.IndexBufferIndex))
                            {
                                var block = header.DataBlocks[3 + vertexBufferInfo.Length + lodData.IndexBufferIndex];
                                reader.Seek(block.Offset, SeekOrigin.Begin);
                                var data = reader.ReadBytes(block.Size);
                                var indexBuffer = new IndexBuffer(data, vInfo.VertexCount > ushort.MaxValue ? typeof(int) : typeof(ushort)) { Layout = section.IndexFormat };

                                ib.Add(lodData.IndexBufferIndex, indexBuffer);
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }

                    var matLookup = materials = new List<Material>(args.Materials.Count);
                    materials.AddRange(GetMaterials(args.Materials));

                    var meshList = new List<Mesh>(args.Sections.Count);
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

                        mesh.Segments.AddRange(
                            lodData.Submeshes.Select(s => new MeshSegment
                            {
                                Material = matLookup.ElementAtOrDefault(s.ShaderIndex),
                                IndexStart = s.IndexStart,
                                IndexLength = s.IndexLength
                            })
                        );

                        return mesh;
                    }));

                    return meshList;
                }
            }
        }
    }
}
