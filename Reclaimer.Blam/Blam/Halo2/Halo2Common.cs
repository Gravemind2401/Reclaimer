using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    [FixedSize(16)]
    public class ResourceInfoBlock
    {
        [Offset(4)]
        public short Type0 { get; set; }

        [Offset(6)]
        public short Type1 { get; set; }

        [Offset(8)]
        public int Size { get; set; }

        [Offset(12)]
        public int Offset { get; set; }
    }

    internal static class Halo2Common
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
                if (rmsh == null) continue;

                foreach (var tagRef in rmsh.ShaderMaps.SelectMany(m => m.EnumerateBitmapReferences()))
                {
                    if (tagRef.Tag == null || complete.Contains(tagRef.TagId))
                        continue;

                    complete.Add(tagRef.TagId);
                    yield return tagRef.Tag.ReadMetadata<bitmap>();
                }
            }
        }

        public static IEnumerable<GeometryMaterial> GetMaterials(IReadOnlyList<ShaderBlock> shaders)
        {
            for (int i = 0; i < shaders.Count; i++)
            {
                var tag = shaders[i].ShaderReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = Utils.GetFileName(tag.FullPath)
                };

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var bitmTag = shader.ShaderMaps[0].DiffuseBitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                material.Submaterials.Add(new SubMaterial
                {
                    Usage = MaterialUsage.Diffuse,
                    Bitmap = bitmTag.ReadMetadata<bitmap>(),
                    Tiling = new RealVector2D(1, 1)
                });

                yield return material;
            }
        }
    }
}
