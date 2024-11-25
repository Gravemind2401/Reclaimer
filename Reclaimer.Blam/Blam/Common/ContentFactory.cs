﻿using Reclaimer.Audio;
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
                var gameType = item.CacheFile.Metadata.Game;
                var cacheType = item.CacheFile.CacheType;

                content = gameType switch
                {
                    HaloGame.Halo1 when cacheType != CacheType.Halo1AE => item.ReadMetadata<Halo1.bitmap>(),
                    HaloGame.Halo2 => item.ReadMetadata<Halo2.bitmap>(),
                    HaloGame.Halo3 => item.ReadMetadata<Halo3.bitmap>(),
                    HaloGame.Halo3ODST => item.ReadMetadata<Halo3.bitmap>(),
                    HaloGame.HaloReach => item.ReadMetadata<HaloReach.bitmap>(),
                    HaloGame.Halo4 => item.ReadMetadata<Halo4.bitmap>(),
                    HaloGame.Halo2X => item.ReadMetadata<Halo4.bitmap>(),
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
                var gameType = item.CacheFile.Metadata.Game;
                var cacheType = item.CacheFile.CacheType;

                if (item.ClassCode is gbxmodel or render_model)
                {
                    content = gameType switch
                    {
                        HaloGame.Halo1 => item.ReadMetadata<Halo1.gbxmodel>(),
                        HaloGame.Halo2 when cacheType == CacheType.Halo2Beta => item.ReadMetadata<Halo2Beta.render_model>(),
                        HaloGame.Halo2 when cacheType is CacheType.Halo2Xbox or CacheType.Halo2Vista => item.ReadMetadata<Halo2.render_model>(),
                        HaloGame.Halo3 => item.ReadMetadata<Halo3.render_model>(),
                        HaloGame.Halo3ODST => item.ReadMetadata<Halo3.render_model>(),
                        HaloGame.HaloReach => item.ReadMetadata<HaloReach.render_model>(),
                        HaloGame.Halo4 => item.ReadMetadata<Halo4.render_model>(),
                        HaloGame.Halo2X => item.ReadMetadata<Halo4.render_model>(),
                        _ => null
                    };
                }
                else if (item.ClassCode == scenario_structure_bsp)
                {
                    content = gameType switch
                    {
                        HaloGame.Halo1 when cacheType is CacheType.Halo1PC or CacheType.Halo1CE or CacheType.MccHalo1 => item.ReadMetadata<Halo1.scenario_structure_bsp>(),
                        HaloGame.Halo2 when cacheType is CacheType.Halo2Xbox or CacheType.Halo2Vista => item.ReadMetadata<Halo2.scenario_structure_bsp>(),
                        HaloGame.Halo3 when cacheType >= CacheType.Halo3Delta => item.ReadMetadata<Halo3.scenario_structure_bsp>(),
                        HaloGame.Halo3ODST => item.ReadMetadata<Halo3.scenario_structure_bsp>(),
                        HaloGame.HaloReach => item.ReadMetadata<HaloReach.scenario_structure_bsp>(),
                        HaloGame.Halo4 => item.ReadMetadata<Halo4.scenario_structure_bsp>(),
                        HaloGame.Halo2X => item.ReadMetadata<Halo4.scenario_structure_bsp>(),
                        _ => null
                    };
                }
                else if (item.ClassCode == scenario)
                {
                    content = gameType switch
                    {
                        HaloGame.Halo1 when cacheType is CacheType.Halo1PC or CacheType.Halo1CE or CacheType.MccHalo1 => item.ReadMetadata<Halo1.scenario>(),
                        HaloGame.Halo2 when cacheType == CacheType.Halo2Xbox => item.ReadMetadata<Halo2.scenario>(),
                        HaloGame.Halo3 when cacheType >= CacheType.Halo3Delta => item.ReadMetadata<Halo3.scenario>(),
                        HaloGame.Halo3ODST => item.ReadMetadata<Halo3.scenario>(),
                        HaloGame.HaloReach => item.ReadMetadata<HaloReach.scenario>(),
                        HaloGame.Halo4 => item.ReadMetadata<Halo4.scenario>(),
                        HaloGame.Halo2X => item.ReadMetadata<Halo4.scenario>(),
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
                        content = item.ReadMetadata<Halo2.sound>();
                        break;
                    //case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        content = item.ReadMetadata<Halo3.sound>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                        content = item.ReadMetadata<HaloReach.sound>();
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
                    ModuleType.Halo5Server or ModuleType.Halo5Forge => item.ReadMetadata<Halo5.bitmap>(),
                    ModuleType.HaloInfinite => item.ReadMetadata<HaloInfinite.bitmap>(),
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
                        render_model => item.ReadMetadata<Halo5.render_model>(),
                        particle_model => item.ReadMetadata<Halo5.particle_model>(),
                        scenario => item.ReadMetadata<Halo5.scenario>(),
                        scenario_structure_bsp => item.ReadMetadata<Halo5.scenario_structure_bsp>(),
                        structure_lightmap => item.ReadMetadata<Halo5.structure_lightmap>(),
                        _ => null
                    };
                }
                else if (item.Module.ModuleType == ModuleType.HaloInfinite)
                {
                    content = item.ClassCode switch
                    {
                        render_model => item.ReadMetadata<HaloInfinite.render_model>(),
                        scenario_structure_bsp => item.ReadMetadata<HaloInfinite.scenario_structure_bsp>(),
                        runtime_geo => item.ReadMetadata<HaloInfinite.runtime_geo>(),
                        object_customization => item.ReadMetadata<HaloInfinite.customization_globals_definition>(),
                        model => item.ReadMetadata<HaloInfinite.model>().ReadRenderModel(),
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
                var compressed = reader.ReadBytes(compressedSize);
                var decompressed = new byte[decompressedSize];

                var startSize = compressedSize;
                var endSize = decompressedSize;
                var decompressionContext = 0L;
                XCompress.XMemCreateDecompressionContext(XCompress.XMemCodecType.LZX, 0, 0, ref decompressionContext);
                XCompress.XMemResetDecompressionContext(decompressionContext);
                XCompress.XMemDecompressStream(decompressionContext, decompressed, ref endSize, compressed, ref startSize);
                XCompress.XMemDestroyDecompressionContext(decompressionContext);

                if (decompressed.Length == segmentLength)
                    return decompressed;

                var result = new byte[segmentLength];
                Array.Copy(decompressed, segmentOffset, result, 0, result.Length);
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
