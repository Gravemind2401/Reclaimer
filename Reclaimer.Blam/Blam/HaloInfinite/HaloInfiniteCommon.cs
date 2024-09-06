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
    }
}
