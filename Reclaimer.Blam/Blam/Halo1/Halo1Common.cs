using Reclaimer.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo1
{
    internal static class Halo1Common
    {
        public static IIndexItem GetShaderDiffuse(TagReference tagRef, DependencyReader reader)
        {
            if (tagRef.Tag == null)
                return null;

            int offset;
            switch (tagRef.Tag.ClassCode)
            {
                case "soso":
                    offset = 176;
                    break;

                case "senv":
                    offset = 148;
                    break;

                case "sgla":
                    offset = 356;
                    break;

                case "schi":
                    offset = 228;
                    break;

                case "scex":
                    offset = 900;
                    break;

                case "swat":
                case "smet":
                    offset = 88;
                    break;

                default: return null;
            }

            try
            {
                reader.Seek(tagRef.Tag.MetaPointer.Address + offset, SeekOrigin.Begin);

                var bitmId = reader.ReadInt16();

                if (bitmId == -1)
                    return null;
                else return tagRef.Tag.CacheFile.TagIndex[bitmId];
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<GeometryMaterial> GetMaterials(IEnumerable<TagReference> shaderRefs, DependencyReader reader)
        {
            foreach (var shaderRef in shaderRefs)
            {
                if (shaderRef.Tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new GeometryMaterial
                {
                    Name = Utils.GetFileName(shaderRef.Tag.FullPath)
                };

                var bitmTag = GetShaderDiffuse(shaderRef, reader);
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
