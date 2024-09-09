using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
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

    public class HaloInfiniteGeometryArgs
    {
        public Module Module { get; init; }
        public ResourcePackingPolicy ResourcePolicy { get; init; }
        public IReadOnlyList<RegionBlock> Regions { get; init; }
        public IReadOnlyList<MaterialBlock> Materials { get; init; }
        public IReadOnlyList<SectionBlock> Sections { get; init; }
        public IReadOnlyList<NodeMapBlock> NodeMaps { get; init; }
        public IReadOnlyList<MeshResourceGroupBlock> MeshResourceGroups { get; init; }
        public int ResourceIndex { get; init; }
        public int ResourceCount { get; init; }
    }

    internal static class HaloInfiniteCommon
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

        public static List<Mesh> GetMeshes(HaloInfiniteGeometryArgs args, out List<Material> materials)
        {
            //TODO: implement an LOD selector one day
            const int lod = 0;
            var lodFlag = (LodFlags)(1 << lod);

            var resourceBuffers = new List<(byte[] Data, int VirtualBaseAddress)>(args.ResourceCount);
            for (var i = 0; i < args.ResourceCount; i++)
            {
                //the offsets given in the RenderGeometryApiResource data are relative to the start of all resource data, not relative to any individual resource tag
                //ie it treats it as if all the resource data was concatenated into a single byte array
                var virtualBaseAddress = resourceBuffers.Sum(x => x.Data.Length);

                var itemIndex = args.Module.Resources[args.ResourceIndex + i];
                var resource = args.Module.Items[itemIndex];
                using (var reader = resource.CreateReader())
                    resourceBuffers.Add((reader.ReadBytes(resource.TotalUncompressedSize), virtualBaseAddress));
            }

            var indexBuffers = new Dictionary<int, IndexBuffer>();
            var vectorBuffers = new Dictionary<int, IVectorBuffer>();
            var vertexBuffers = new Dictionary<int, VertexBuffer>();

            foreach (var section in args.Sections)
            {
                if (section.SectionLods[0].LodFlags > 0 && (section.SectionLods[0].LodFlags & lodFlag) == 0)
                    continue;

                var lodIndex = Math.Min(lod, section.SectionLods.Count - 1);
                var lodData = section.SectionLods[lodIndex];

                if (!indexBuffers.ContainsKey(lodData.IndexBufferIndex))
                {
                    //TODO: will there ever be more than one MeshResourceGroup?
                    var indexBufferInfo = args.MeshResourceGroups[0].RenderGeometryApiResource.PcIndexBuffers[lodData.IndexBufferIndex];
                    var (resourceBuffer, virtualBaseAddress) = resourceBuffers.Last(b => b.VirtualBaseAddress < indexBufferInfo.Offset);

                    var buffer = new IndexBuffer(resourceBuffer, indexBufferInfo.Count, indexBufferInfo.Offset - virtualBaseAddress, indexBufferInfo.Stride, 0, indexBufferInfo.Stride);
                    buffer.Layout = section.IndexFormat;

                    indexBuffers.Add(lodData.IndexBufferIndex, buffer);
                }

                foreach (var vertexBufferIndex in lodData.VertexBufferIndicies.ValidIndicies.Where(vi => !vectorBuffers.ContainsKey(vi)))
                {
                    //TODO: add vector buffer to lookup
                }

                //TODO: build vertex buffer from referenced vector sets
            }

            materials = new List<Material>(args.Materials?.Count ?? default);
            var meshList = new List<Mesh>(args.Sections.Count);

            return meshList;
        }
    }
}
