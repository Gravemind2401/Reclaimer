using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo1
{
    internal static class Halo1Common
    {
        public static IIndexItem GetShaderDiffuse(TagReference tagRef, DependencyReader reader)
        {
            if (tagRef.Tag == null)
                return null;

            var offset = tagRef.Tag.ClassCode switch
            {
                "soso" => 176,
                "senv" => 148,
                "sgla" => 356,
                "schi" => 228,
                "scex" => 900,
                "swat" or "smet" => 88,
                _ => default
            };

            if (offset == default)
                return null;

            try
            {
                reader.Seek(tagRef.Tag.MetaPointer.Address + offset, SeekOrigin.Begin);

                var bitmId = reader.ReadInt16();
                return bitmId == -1 ? null : tagRef.Tag.CacheFile.TagIndex[bitmId];
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<Material> GetMaterials(IEnumerable<TagReference> shaderRefs, DependencyReader reader)
        {
            foreach (var shaderRef in shaderRefs)
            {
                if (shaderRef.Tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = shaderRef.Tag.Id,
                    Name = shaderRef.Tag.FileName
                };

                var bitmTag = GetShaderDiffuse(shaderRef, reader);
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                material.TextureMappings.Add(new TextureMapping
                {
                    Usage = (int)MaterialUsage.Diffuse,
                    Tiling = Vector2.One,
                    Texture = new Texture
                    {
                        Id = bitmTag.Id,
                        Name = bitmTag.TagName,
                        GetDds = () => bitmTag.ReadMetadata<bitmap>().ToDds(0)
                    }
                });

                yield return material;
            }
        }
    }
}
