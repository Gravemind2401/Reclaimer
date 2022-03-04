using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public static class ContentFactory
    {
        private const string bitmap = "bitm";
        private const string gbxmodel = "mod2";
        private const string render_model = "mode";
        private const string scenario_structure_bsp = "sbsp";
        private const string sound = "snd!";

        #region Standard Halo Maps

        public static bool TryGetPrimaryContent(IIndexItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    IBitmap bitmapContent;
                    if (TryGetBitmapContent(item, out bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case gbxmodel:
                case render_model:
                case scenario_structure_bsp:
                    IRenderGeometry geometryContent;
                    if (TryGetGeometryContent(item, out geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
                case sound:
                    ISoundContainer soundContent;
                    if (TryGetSoundContent(item, out soundContent))
                    {
                        content = soundContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IIndexItem item, out IBitmap content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode != bitmap)
                return false;

            switch (item.CacheFile.CacheType)
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1CE:
                case CacheType.Halo1PC:
                case CacheType.MccHalo1:
                    content = item.ReadMetadata<Halo1.bitmap>();
                    break;
                case CacheType.Halo2Beta:
                case CacheType.Halo2Xbox:
                    content = item.ReadMetadata<Halo2.bitmap>();
                    break;
                case CacheType.Halo3Alpha:
                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.MccHalo3:
                case CacheType.MccHalo3U4:
                case CacheType.MccHalo3F6:
                case CacheType.MccHalo3U6:
                case CacheType.Halo3ODST:
                case CacheType.MccHalo3ODST:
                case CacheType.MccHalo3ODSTF3:
                case CacheType.MccHalo3ODSTU3:
                    content = item.ReadMetadata<Halo3.bitmap>();
                    break;
                case CacheType.HaloReachBeta:
                case CacheType.HaloReachRetail:
                case CacheType.MccHaloReach:
                case CacheType.MccHaloReachU3:
                case CacheType.MccHaloReachU8:
                    content = item.ReadMetadata<HaloReach.bitmap>();
                    break;
                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
                case CacheType.MccHalo4:
                case CacheType.MccHalo2X:
                    content = item.ReadMetadata<Halo4.bitmap>();
                    break;
            }

            return content != null;
        }

        public static bool TryGetGeometryContent(IIndexItem item, out IRenderGeometry content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode == gbxmodel || item.ClassCode == render_model)
            {
                switch (item.CacheFile.CacheType)
                {
                    case CacheType.Halo1Xbox:
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                    case CacheType.Halo1AE:
                    case CacheType.MccHalo1:
                        content = item.ReadMetadata<Halo1.gbxmodel>();
                        break;
                    case CacheType.Halo2Beta:
                        content = item.ReadMetadata<Halo2Beta.render_model>();
                        break;
                    case CacheType.Halo2Xbox:
                        content = item.ReadMetadata<Halo2.render_model>();
                        break;
                    case CacheType.Halo3Alpha:
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.MccHalo3:
                    case CacheType.MccHalo3U4:
                    case CacheType.MccHalo3F6:
                    case CacheType.MccHalo3U6:
                    case CacheType.Halo3ODST:
                    case CacheType.MccHalo3ODST:
                    case CacheType.MccHalo3ODSTF3:
                    case CacheType.MccHalo3ODSTU3:
                        content = item.ReadMetadata<Halo3.render_model>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                    case CacheType.MccHaloReach:
                    case CacheType.MccHaloReachU3:
                    case CacheType.MccHaloReachU8:
                        content = item.ReadMetadata<HaloReach.render_model>();
                        break;
                    case CacheType.Halo4Beta:
                    case CacheType.Halo4Retail:
                    case CacheType.MccHalo4:
                    case CacheType.MccHalo2X:
                        content = item.ReadMetadata<Halo4.render_model>();
                        break;
                }
            }
            else if (item.ClassCode == scenario_structure_bsp)
            {
                switch (item.CacheFile.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        content = item.ReadMetadata<Halo1.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo2Xbox:
                        content = item.ReadMetadata<Halo2.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo3Alpha:
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.MccHalo3:
                    case CacheType.MccHalo3U4:
                    case CacheType.MccHalo3F6:
                    case CacheType.MccHalo3U6:
                    case CacheType.Halo3ODST:
                    case CacheType.MccHalo3ODST:
                    case CacheType.MccHalo3ODSTF3:
                    case CacheType.MccHalo3ODSTU3:
                        content = item.ReadMetadata<Halo3.scenario_structure_bsp>();
                        break;
                    case CacheType.HaloReachBeta:
                    case CacheType.HaloReachRetail:
                    case CacheType.MccHaloReach:
                    case CacheType.MccHaloReachU3:
                    case CacheType.MccHaloReachU8:
                        content = item.ReadMetadata<HaloReach.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo4Beta:
                    case CacheType.Halo4Retail:
                    case CacheType.MccHalo4:
                    case CacheType.MccHalo2X:
                        content = item.ReadMetadata<Halo4.scenario_structure_bsp>();
                        break;
                }
            }

            return content != null;
        }

        public static bool TryGetSoundContent(IIndexItem item, out ISoundContainer content)
        {
            content = null;

            if (item == null)
                return false;

            if (item.ClassCode != sound)
                return false;

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

        #endregion

        #region Halo 5

        public static bool TryGetPrimaryContent(Halo5.ModuleItem item, out object content)
        {
            switch (item.ClassCode)
            {
                case bitmap:
                    IBitmap bitmapContent;
                    if (TryGetBitmapContent(item, out bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case render_model:
                    IRenderGeometry geometryContent;
                    if (TryGetGeometryContent(item, out geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(Halo5.ModuleItem item, out IBitmap content)
        {
            content = null;

            if (item.ClassCode != bitmap)
                return false;

            content = item.ReadMetadata<Halo5.bitmap>();

            return content != null;
        }

        public static bool TryGetGeometryContent(Halo5.ModuleItem item, out IRenderGeometry content)
        {
            content = null;

            if (item.ClassCode != render_model)
                return false;

            content = item.ReadMetadata<Halo5.render_model>();

            return content != null;
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
                    int position = 0;
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

                int startSize = compressedSize;
                int endSize = decompressedSize;
                int decompressionContext = 0;
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
                    for (int i = 0; i < segmentLength;)
                    {
                        bool flag;
                        var blockSize = ReadSpecialInt(reader2, out flag);
                        if (flag) reader2.ReadBytes(2);
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
