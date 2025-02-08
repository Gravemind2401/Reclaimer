using Reclaimer.Audio;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.IO;
using System.IO.Compression;

namespace Reclaimer.Blam.Common
{
    public static class ContentFactory
    {
        private const string bitmap = "bitm";
        private const string gbxmodel = "mod2";
        private const string render_model = "mode";
        private const string particle_model = "pmdf";
        private const string scenario = "scnr";
        private const string scenario_structure_bsp = "sbsp";
        private const string structure_lightmap = "stlm";
        private const string sound = "snd!";
        private const string runtime_geo = "rtgo";
        private const string object_customization = "ocgd";
        private const string model = "hlmt";

        #region Cache Files

        public static bool TryGetPrimaryContent(IIndexItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    if (TryGetBitmapContent(item, out var bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case gbxmodel:
                case render_model:
                case scenario:
                case scenario_structure_bsp:
                    if (TryGetGeometryContent(item, out var geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
                case sound:
                    if (TryGetSoundContent(item, out var soundContent))
                    {
                        content = soundContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IIndexItem item, out IContentProvider<IBitmap> content)
        {
            content = null;

            if (item?.ClassCode != bitmap)
                return false;

            try
            {
                var engineType = item.CacheFile.Metadata.Engine;
                var cacheType = item.CacheFile.CacheType;

                content = engineType switch
                {
                    BlamEngine.Halo1 when cacheType != CacheType.Halo1AE => item.ReadMetadata<Halo1.BitmapTag>(),
                    BlamEngine.Halo2 => item.ReadMetadata<Halo2.BitmapTag>(),
                    BlamEngine.Halo3 => item.ReadMetadata<Halo3.BitmapTag>(),
                    BlamEngine.Halo3ODST => item.ReadMetadata<Halo3.BitmapTag>(),
                    BlamEngine.HaloReach => item.ReadMetadata<HaloReach.BitmapTag>(),
                    BlamEngine.Halo4 => item.ReadMetadata<Halo4.BitmapTag>(),
                    BlamEngine.Halo2X => item.ReadMetadata<Halo4.BitmapTag>(),
                    _ => null
                };

                return content != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetGeometryContent(IIndexItem item, out IContentProvider<Scene> content)
        {
            content = null;

            if (item == null)
                return false;

            try
            {
                var engineType = item.CacheFile.Metadata.Engine;
                var cacheType = item.CacheFile.CacheType;

                if (item.ClassCode is gbxmodel or render_model)
                {
                    content = engineType switch
                    {
                        BlamEngine.Halo1 => item.ReadMetadata<Halo1.GbxModelTag>(),
                        BlamEngine.Halo2 when cacheType <= CacheType.Halo2Beta => item.ReadMetadata<Halo2Beta.RenderModelTag>(),
                        BlamEngine.Halo2 => item.ReadMetadata<Halo2.RenderModelTag>(),
                        BlamEngine.Halo3 => item.ReadMetadata<Halo3.RenderModelTag>(),
                        BlamEngine.Halo3ODST => item.ReadMetadata<Halo3.RenderModelTag>(),
                        BlamEngine.HaloReach => item.ReadMetadata<HaloReach.RenderModelTag>(),
                        BlamEngine.Halo4 => item.ReadMetadata<Halo4.RenderModelTag>(),
                        BlamEngine.Halo2X => item.ReadMetadata<Halo4.RenderModelTag>(),
                        _ => null
                    };
                }
                else if (item.ClassCode == scenario_structure_bsp)
                {
                    content = engineType switch
                    {
                        BlamEngine.Halo1 when cacheType is CacheType.Halo1PC or CacheType.Halo1CE or CacheType.MccHalo1 => item.ReadMetadata<Halo1.ScenarioStructureBspTag>(),
                        BlamEngine.Halo2 when cacheType >= CacheType.Halo2Beta => item.ReadMetadata<Halo2.ScenarioStructureBspTag>(),
                        BlamEngine.Halo3 when cacheType >= CacheType.Halo3Delta => item.ReadMetadata<Halo3.ScenarioStructureBspTag>(),
                        BlamEngine.Halo3ODST => item.ReadMetadata<Halo3.ScenarioStructureBspTag>(),
                        BlamEngine.HaloReach => item.ReadMetadata<HaloReach.ScenarioStructureBspTag>(),
                        BlamEngine.Halo4 => item.ReadMetadata<Halo4.ScenarioStructureBspTag>(),
                        BlamEngine.Halo2X => item.ReadMetadata<Halo4.ScenarioStructureBspTag>(),
                        _ => null
                    };
                }
                else if (item.ClassCode == scenario)
                {
                    content = engineType switch
                    {
                        BlamEngine.Halo1 when cacheType is CacheType.Halo1PC or CacheType.Halo1CE or CacheType.MccHalo1 => item.ReadMetadata<Halo1.ScenarioTag>(),
                        BlamEngine.Halo2 when cacheType >= CacheType.Halo2Xbox => item.ReadMetadata<Halo2.ScenarioTag>(),
                        BlamEngine.Halo3 when cacheType >= CacheType.Halo3Delta => item.ReadMetadata<Halo3.ScenarioTag>(),
                        BlamEngine.Halo3ODST => item.ReadMetadata<Halo3.ScenarioTag>(),
                        BlamEngine.HaloReach => item.ReadMetadata<HaloReach.ScenarioTag>(),
                        BlamEngine.Halo4 => item.ReadMetadata<Halo4.ScenarioTag>(),
                        BlamEngine.Halo2X => item.ReadMetadata<Halo4.ScenarioTag>(),
                        _ => null
                    };
                }

                return content != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetSoundContent(IIndexItem item, out IContentProvider<GameSound> content)
        {
            content = null;

            if (item?.ClassCode != sound)
                return false;

            try
            {
                switch (item.CacheFile.CacheType)
                {
                    case CacheType.Halo2Xbox:
                        content = item.ReadMetadata<Halo2.SoundTag>();
                        break;
                    //case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        content = item.ReadMetadata<Halo3.SoundTag>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                        content = item.ReadMetadata<HaloReach.SoundTag>();
                        break;
                }

                return content != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Module Files

        public static bool TryGetPrimaryContent(IModuleItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    if (TryGetBitmapContent(item, out var bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case render_model:
                case object_customization:
                case runtime_geo:
                case model:
                case particle_model:
                case scenario:
                case scenario_structure_bsp:
                case structure_lightmap:
                    if (TryGetGeometryContent(item, out var geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IModuleItem item, out IContentProvider<IBitmap> content)
        {
            content = null;

            if (item?.ClassCode != bitmap)
                return false;

            try
            {
                content = item.Module.ModuleType switch
                {
                    ModuleType.Halo5Server or ModuleType.Halo5Forge => item.ReadMetadata<Halo5.BitmapTag>(),
                    ModuleType.HaloInfinite => item.ReadMetadata<HaloInfinite.BitmapTag>(),
                    _ => null
                };

                return content != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetGeometryContent(IModuleItem item, out IContentProvider<Scene> content)
        {
            content = null;

            if (item == null)
                return false;

            try
            {
                if (item.Module.ModuleType is ModuleType.Halo5Server or ModuleType.Halo5Forge)
                {
                    content = item.ClassCode switch
                    {
                        render_model => item.ReadMetadata<Halo5.RenderModelTag>(),
                        particle_model => item.ReadMetadata<Halo5.ParticleModelTag>(),
                        scenario => item.ReadMetadata<Halo5.ScenarioTag>(),
                        scenario_structure_bsp => item.ReadMetadata<Halo5.ScenarioStructureBspTag>(),
                        structure_lightmap => item.ReadMetadata<Halo5.StructureLightmapTag>(),
                        _ => null
                    };
                }
                else if (item.Module.ModuleType == ModuleType.HaloInfinite)
                {
                    content = item.ClassCode switch
                    {
                        render_model => item.ReadMetadata<HaloInfinite.RenderModelTag>(),
                        scenario_structure_bsp => item.ReadMetadata<HaloInfinite.ScenarioStructureBspTag>(),
                        runtime_geo => item.ReadMetadata<HaloInfinite.RuntimeGeoTag>(),
                        object_customization => item.ReadMetadata<HaloInfinite.CustomizationGlobalsDefinitionTag>(),
                        model => item.ReadMetadata<HaloInfinite.ModelTag>().ReadRenderModel(),
                        _ => null
                    };
                }

                return content != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        //the reader must currently be at the start of the encoded block. the first [segmentOffset] bytes of the block will be discarded.
        internal static byte[] GetResourceData(EndianReader reader, CacheResourceCodec codec, int maxLength, int segmentOffset, int compressedSize, int decompressedSize)
        {
            var segmentLength = Math.Min(maxLength, decompressedSize - segmentOffset);
            if (codec == CacheResourceCodec.Uncompressed || compressedSize == decompressedSize)
            {
                reader.Seek(segmentOffset, SeekOrigin.Current);
                return reader.ReadBytes(segmentLength);
            }
            else if (codec == CacheResourceCodec.Deflate)
            {
                const int maxSeekSize = 0x80000;

                using (var ds = new DeflateStream(reader.BaseStream, CompressionMode.Decompress))
                using (var reader2 = new BinaryReader(ds))
                {
                    var position = 0;
                    while (position < segmentOffset)
                    {
                        var seek = Math.Min(maxSeekSize, segmentOffset - position);
                        reader2.ReadBytes(seek);
                        position += seek;
                    }

                    return reader2.ReadBytes(segmentLength);
                }
            }
            else if (codec == CacheResourceCodec.LZX)
            {
                var data = reader.ReadBytes(compressedSize);
                data = XCompress.DecompressLZX(data, ref decompressedSize);

                if (data.Length == segmentLength)
                    return data;

                var result = new byte[segmentLength];
                Array.Copy(data, segmentOffset, result, 0, result.Length);
                return result;
            }
            else if (codec == CacheResourceCodec.UnknownDeflate) //experimental
            {
                using (var ms = new MemoryStream())
                using (var mw = new BinaryWriter(ms))
                using (var ds = new DeflateStream(reader.BaseStream, CompressionMode.Decompress))
                using (var reader2 = new BinaryReader(ds))
                {
                    for (var i = 0; i < segmentLength;)
                    {
                        var blockSize = ReadSpecialInt(reader2, out var flag);
                        if (flag)
                            reader2.ReadBytes(2);
                        mw.Write(reader2.ReadBytes(blockSize));
                        i += blockSize;
                    }

                    return ms.ToArray();
                }
            }
            else
                throw new NotSupportedException("Unknown Resource Codec");
        }

        private static int ReadSpecialInt(BinaryReader reader, out bool flag) //basically a variant of 7bit encoded int
        {
            flag = false;
            var isFirst = true;

            var result = 0;
            var shift = 0;

            byte b;
            do
            {
                var bits = isFirst ? 6 : 7;
                var mask = (1 << bits) - 1;

                b = reader.ReadByte();
                result |= (b & mask) << shift;
                shift += bits;

                if (isFirst)
                {
                    flag = (b & 0x40) != 0;
                    isFirst = false;
                }
            } while ((b & 0x80) != 0);

            return result;
        }
    }
}
