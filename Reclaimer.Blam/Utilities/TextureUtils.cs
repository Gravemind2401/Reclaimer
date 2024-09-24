using Reclaimer.Drawing;
using Reclaimer.IO;

namespace Reclaimer.Blam.Utilities
{
    public static class TextureUtils
    {
        #region Lookups

        private static readonly Dictionary<KnownTextureFormat, DxgiFormat> dxgiLookup = new Dictionary<KnownTextureFormat, DxgiFormat>
        {
            { KnownTextureFormat.DXT1, DxgiFormat.BC1_UNorm },
            { KnownTextureFormat.DXT3, DxgiFormat.BC2_UNorm },
            { KnownTextureFormat.DXT5, DxgiFormat.BC3_UNorm },
            { KnownTextureFormat.BC1_unorm, DxgiFormat.BC1_UNorm },
            { KnownTextureFormat.BC2_unorm, DxgiFormat.BC2_UNorm },
            { KnownTextureFormat.BC3_unorm, DxgiFormat.BC3_UNorm },
            { KnownTextureFormat.BC7_unorm, DxgiFormat.BC7_UNorm },
            { KnownTextureFormat.A8R8G8B8, DxgiFormat.B8G8R8A8_UNorm },
            { KnownTextureFormat.X8R8G8B8, DxgiFormat.B8G8R8X8_UNorm },
            { KnownTextureFormat.R5G6B5, DxgiFormat.B5G6R5_UNorm },
            { KnownTextureFormat.A1R5G5B5, DxgiFormat.B5G5R5A1_UNorm },
            { KnownTextureFormat.A4R4G4B4, DxgiFormat.B4G4R4A4_UNorm },
            { KnownTextureFormat.RGBFP32, DxgiFormat.R32G32B32_Float },
            { KnownTextureFormat.RGBAFP32, DxgiFormat.R32G32B32A32_Float },
            { KnownTextureFormat.L16, DxgiFormat.R16_UNorm },
            { KnownTextureFormat.A2R10G10B10, DxgiFormat.R10G10B10A2_UNorm },
            { KnownTextureFormat.Q8W8V8U8, DxgiFormat.R8G8B8A8_SNorm },
            { KnownTextureFormat.V8U8, DxgiFormat.R8G8_SNorm },
            { KnownTextureFormat.R8G8, DxgiFormat.R8G8_UNorm },
            { KnownTextureFormat.SignedR16G16B16A16, DxgiFormat.R16G16B16A16_SNorm },
            { KnownTextureFormat.RGBAFP16, DxgiFormat.R16G16B16A16_Float },
            { KnownTextureFormat.A16B16G16R16, DxgiFormat.R16G16B16A16_UNorm },
        };

        private static readonly Dictionary<KnownTextureFormat, XboxFormat> xboxLookup = new Dictionary<KnownTextureFormat, XboxFormat>
        {
            { KnownTextureFormat.A8, XboxFormat.A8 },
            { KnownTextureFormat.A8Y8, XboxFormat.Y8A8 },
            { KnownTextureFormat.AY8, XboxFormat.AY8 },
            { KnownTextureFormat.CTX1, XboxFormat.CTX1 },
            { KnownTextureFormat.DXT3a_mono, XboxFormat.DXT3a_mono },
            { KnownTextureFormat.DXT3a_alpha, XboxFormat.DXT3a_alpha },
            { KnownTextureFormat.BC4_unorm, XboxFormat.DXT5a_scalar },
            { KnownTextureFormat.DXT5a, XboxFormat.DXT5a_scalar },
            { KnownTextureFormat.DXT5a_mono, XboxFormat.DXT5a_mono },
            { KnownTextureFormat.DXT5a_alpha, XboxFormat.DXT5a_alpha },
            { KnownTextureFormat.DXN, XboxFormat.DXN },
            { KnownTextureFormat.DXN_SNorm, XboxFormat.DXN_SNorm },
            { KnownTextureFormat.DXN_mono_alpha, XboxFormat.DXN_mono_alpha },
            { KnownTextureFormat.P8, XboxFormat.Y8 },
            { KnownTextureFormat.P8_bump, XboxFormat.Y8 },
            { KnownTextureFormat.U8V8, XboxFormat.V8U8 },
            { KnownTextureFormat.Y8, XboxFormat.Y8 }
        };

        #endregion

        #region Extensions

        private enum KnownTextureFormat
        {
            Unknown,
            A8,
            Y8,
            AY8,
            A8Y8,
            R5G6B5,
            A1R5G5B5,
            A4R4G4B4,
            X8R8G8B8,
            A8R8G8B8,
            DXT1,
            DXT3,
            DXT5,
            P8_bump,
            P8,
            ARGBFP32, //TODO: should this actually be RGBA instead of ARGB? the games this is defined in have no examples
            RGBAFP32,
            RGBFP32,
            RGBFP16,
            RGBAFP16,
            A16B16G16R16,
            SignedR16G16B16A16,
            U8V8,
            DXT5a,
            DXN,
            DXN_SNorm,
            CTX1,
            DXT3a_alpha,
            DXT3a_mono,
            DXT5a_alpha,
            DXT5a_mono,
            DXN_mono_alpha,
            BC1_unorm,
            BC2_unorm,
            BC3_unorm,
            BC4_unorm, //same as DXT5a
            BC6H_UF16,
            BC6H_SF16,
            BC7_unorm,
            L16,
            A2R10G10B10,
            Q8W8V8U8,
            V8U8,
            R8G8
        }

        private enum KnownTextureType : short
        {
            Texture2D,
            Texture3D,
            CubeMap,
            Array
        }

        private static T ParseToEnum<T>(this object input, T defaultValue = default) where T : struct
        {
            if (input != null)
            {
                if (Enum.TryParse(input.ToString(), out T enumValue))
                    return enumValue;
            }

            return defaultValue;
        }

        //number of bits used to store each pixel
        private static int GetBpp(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.ARGBFP32:
                case KnownTextureFormat.RGBAFP32:
                    return 128;

                case KnownTextureFormat.RGBFP32:
                    return 96;

                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.A2R10G10B10:
                case KnownTextureFormat.Q8W8V8U8:
                case KnownTextureFormat.X8R8G8B8:
                    return 32;

                case KnownTextureFormat.R8G8:
                case KnownTextureFormat.V8U8:
                case KnownTextureFormat.L16:
                    return 16;

                case KnownTextureFormat.A8:
                case KnownTextureFormat.Y8:
                case KnownTextureFormat.AY8:
                case KnownTextureFormat.P8_bump:
                    return 8;

                case KnownTextureFormat.CTX1:
                case KnownTextureFormat.DXT1:
                case KnownTextureFormat.DXT3a_alpha:
                case KnownTextureFormat.DXT3a_mono:
                case KnownTextureFormat.DXT5a:
                case KnownTextureFormat.DXT5a_alpha:
                case KnownTextureFormat.DXT5a_mono:
                case KnownTextureFormat.BC4_unorm:
                    return 4;

                case KnownTextureFormat.DXT3:
                case KnownTextureFormat.DXT5:
                case KnownTextureFormat.DXN:
                case KnownTextureFormat.DXN_SNorm:
                case KnownTextureFormat.DXN_mono_alpha:
                case KnownTextureFormat.BC6H_UF16:
                case KnownTextureFormat.BC6H_SF16:
                case KnownTextureFormat.BC7_unorm:
                    return 8;

                default:
                    return 16;
            }
        }

        //the size in bytes of each read/write unit
        //ie 32bit uses ints, DXT uses shorts etc. Used for endian swaps.
        private static int GetLinearUnitSize(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.ARGBFP32:
                case KnownTextureFormat.RGBAFP32:
                case KnownTextureFormat.RGBFP32:
                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                    return 4;

                case KnownTextureFormat.A8:
                case KnownTextureFormat.Y8:
                case KnownTextureFormat.AY8:
                case KnownTextureFormat.P8_bump:
                    return 1;

                default:
                    return 2;
            }
        }

        //the width and height in pixels of each compressed block
        private static int GetLinearBlockSize(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.DXT5a_mono:
                case KnownTextureFormat.DXT5a_alpha:
                case KnownTextureFormat.DXT1:
                case KnownTextureFormat.CTX1:
                case KnownTextureFormat.DXT5a:
                case KnownTextureFormat.DXT3a_alpha:
                case KnownTextureFormat.DXT3a_mono:
                case KnownTextureFormat.DXT3:
                case KnownTextureFormat.DXT5:
                case KnownTextureFormat.DXN:
                case KnownTextureFormat.DXN_SNorm:
                case KnownTextureFormat.DXN_mono_alpha:
                case KnownTextureFormat.BC4_unorm:
                case KnownTextureFormat.BC6H_UF16:
                case KnownTextureFormat.BC6H_SF16:
                case KnownTextureFormat.BC7_unorm:
                    return 4;

                default:
                    return 1;
            }
        }

        //the size in bytes of each compressed block
        private static int GetLinearTexelPitch(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.DXT5a_mono:
                case KnownTextureFormat.DXT5a_alpha:
                case KnownTextureFormat.DXT1:
                case KnownTextureFormat.CTX1:
                case KnownTextureFormat.DXT5a:
                case KnownTextureFormat.DXT3a_alpha:
                case KnownTextureFormat.DXT3a_mono:
                case KnownTextureFormat.BC4_unorm:
                    return 8;

                case KnownTextureFormat.DXT3:
                case KnownTextureFormat.DXT5:
                case KnownTextureFormat.DXN:
                case KnownTextureFormat.DXN_SNorm:
                case KnownTextureFormat.DXN_mono_alpha:
                case KnownTextureFormat.BC6H_UF16:
                case KnownTextureFormat.BC6H_SF16:
                case KnownTextureFormat.BC7_unorm:
                    return 16;

                case KnownTextureFormat.A8:
                case KnownTextureFormat.AY8:
                case KnownTextureFormat.P8:
                case KnownTextureFormat.P8_bump:
                case KnownTextureFormat.Y8:
                    return 1;

                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                    return 4;

                default:
                    return 2;
            }
        }

        //on xbox 360 these texture formats must have dimensions that are multiples of these values.
        //if the bitmap dimensions are not multiples they are rounded up and cropped when displayed.
        private static int GetTileSize(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.A8:
                case KnownTextureFormat.AY8:
                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                case KnownTextureFormat.A4R4G4B4:
                case KnownTextureFormat.R5G6B5:
                case KnownTextureFormat.U8V8:
                    return 32;

                case KnownTextureFormat.A8Y8:
                case KnownTextureFormat.Y8:
                case KnownTextureFormat.DXT5a_mono:
                case KnownTextureFormat.DXT5a_alpha:
                case KnownTextureFormat.DXT1:
                case KnownTextureFormat.CTX1:
                case KnownTextureFormat.DXT5a:
                case KnownTextureFormat.DXT3a_alpha:
                case KnownTextureFormat.DXT3a_mono:
                case KnownTextureFormat.DXT3:
                case KnownTextureFormat.DXT5:
                case KnownTextureFormat.DXN:
                case KnownTextureFormat.DXN_mono_alpha:
                    return 128;

                default:
                    return 1;
            }
        }

        public static int Bpp(this Blam.Halo2.TextureFormat format) => GetBpp(format.ParseToEnum<KnownTextureFormat>());
        public static int LinearUnitSize(this Blam.Halo2.TextureFormat format) => GetLinearUnitSize(format.ParseToEnum<KnownTextureFormat>());

        //round up to the nearst valid size, accounting for block sizes and tile sizes
        public static void GetVirtualSize(object format, int width, int height, out int virtualWidth, out int virtualHeight)
        {
            var knownFormat = format.ParseToEnum<KnownTextureFormat>();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            double blockSize = GetLinearBlockSize(knownFormat);
            double tileSize = GetTileSize(knownFormat);

            virtualWidth = (int)(Math.Ceiling(width / tileSize) * tileSize);
            virtualHeight = (int)(Math.Ceiling(height / tileSize) * tileSize);

            virtualWidth = (int)(Math.Ceiling(virtualWidth / blockSize) * blockSize);
            virtualHeight = (int)(Math.Ceiling(virtualHeight / blockSize) * blockSize);
        }

        public static byte[] ApplyCrop(byte[] data, object format, int faces, int inWidth, int inHeight, int outWidth, int outHeight)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (outWidth >= inWidth && outHeight >= inHeight)
                return data;

            var knownFormat = format.ParseToEnum<KnownTextureFormat>();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            double blockLen = GetLinearBlockSize(knownFormat);
            var blockSize = GetLinearTexelPitch(knownFormat);

            var inRows = (int)Math.Ceiling(inHeight / blockLen) / faces;
            var outRows = (int)Math.Ceiling(outHeight / blockLen) / faces;
            var inStride = (int)Math.Ceiling(inWidth / blockLen) * blockSize;
            var outStride = (int)Math.Ceiling(outWidth / blockLen) * blockSize;

            var output = new byte[outRows * outStride * faces];
            for (var f = 0; f < faces; f++)
            {
                var srcTileStart = inRows * inStride * f;
                var destTileStart = outRows * outStride * f;

                for (var s = 0; s < outRows; s++)
                    Array.Copy(data, srcTileStart + inStride * s, output, destTileStart + outStride * s, outStride);
            }

            return output;
        }

        public static object DXNSwap(object format, bool shouldSwap)
        {
            var knownFormat = format.ParseToEnum<KnownTextureFormat>();
            return shouldSwap && knownFormat == KnownTextureFormat.DXN
                ? KnownTextureFormat.DXN_SNorm
                : format;
        }

        public static int GetBitmapDataLength(BitmapProperties props, bool includeMips)
        {
            if (props.MipmapCount == 0)
                includeMips = false;

            int virtualWidth, virtualHeight;
            if (!props.UsesPadding)
            {
                virtualWidth = props.Width;
                virtualHeight = props.Height;
            }
            else
                GetVirtualSize(props.BitmapFormat, props.Width, props.Height, out virtualWidth, out virtualHeight);

            var bitmapFormat = props.BitmapFormat.ParseToEnum<KnownTextureFormat>();
            var frameSize = virtualWidth * virtualHeight * GetBpp(bitmapFormat) / 8;

            if (includeMips)
            {
                var mipsSize = 0;
                var minUnit = (int)Math.Pow(GetLinearBlockSize(bitmapFormat), 2) * GetBpp(bitmapFormat) / 8;
                for (var i = 1; i <= props.MipmapCount; i++)
                    mipsSize += Math.Max(minUnit, (int)(frameSize * Math.Pow(0.25, i)));
                frameSize += mipsSize;
            }

            return frameSize * Math.Max(1, props.FrameCount);
        }

        public static DdsImage GetDds(BitmapProperties props, byte[] data, bool includeMips)
        {
            if (props.MipmapCount == 0)
                includeMips = false;

            int virtualWidth, virtualHeight;
            if (!props.UsesPadding)
            {
                virtualWidth = props.Width;
                virtualHeight = props.Height;
            }
            else
                GetVirtualSize(props.BitmapFormat, props.Width, props.Height, out virtualWidth, out virtualHeight);

            if (props.VirtualWidth > 0)
                virtualWidth = props.VirtualWidth;
            if (props.VirtualHeight > 0)
                virtualHeight = props.VirtualHeight;

            var bitmapFormat = props.BitmapFormat.ParseToEnum<KnownTextureFormat>();
            var textureType = props.BitmapType.ParseToEnum<KnownTextureType>();

            if (props.ByteOrder == ByteOrder.BigEndian)
            {
                var unitSize = GetLinearUnitSize(bitmapFormat);
                if (unitSize > 1)
                {
                    for (var i = 0; i < data.Length - 1; i += unitSize)
                        Array.Reverse(data, i, unitSize);
                }
            }

            var arrayHeight = virtualHeight * Math.Max(1, props.FrameCount);

            if (includeMips)
            {
                var mipsHeight = 0d;
                for (var i = 1; i <= props.MipmapCount; i++)
                    mipsHeight += arrayHeight * Math.Pow(0.25, i);

                var minUnit = GetLinearBlockSize(bitmapFormat);
                mipsHeight += (minUnit - (mipsHeight % minUnit)) % minUnit;

                arrayHeight += (int)mipsHeight * Math.Max(1, props.FrameCount);
            }

            if (props.Swizzled)
                data = XTextureScramble(data, virtualWidth, arrayHeight, props.BitmapFormat, false);

            if (virtualWidth > props.Width || virtualHeight > props.Height)
                data = ApplyCrop(data, props.BitmapFormat, props.FrameCount, virtualWidth, virtualHeight, props.Width, props.Height);

            var dds = dxgiLookup.TryGetValue(bitmapFormat, out var dxgiFormat)
                ? new DdsImage(props.Height, props.Width, dxgiFormat, data)
                : xboxLookup.TryGetValue(bitmapFormat, out var xboxFormat)
                ? new DdsImage(props.Height, props.Width, xboxFormat, data)
                : throw Exceptions.BitmapFormatNotSupported(props.BitmapFormat.ToString());

            if (textureType == KnownTextureType.CubeMap)
                dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
            if (textureType == KnownTextureType.Array || textureType == KnownTextureType.Texture3D)
                dds.ArraySize = props.FrameCount;
            if (includeMips)
                dds.MipmapCount = props.MipmapCount + 1;

            return dds;
        }

        #endregion

        #region Original Xbox

        /* http://www.h2maps.net/Tools/Xbox/Mutation/Mutation/DDS/Swizzle.cs */

        private class MaskSet
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;

            public MaskSet(int w, int h, int d)
            {
                var bit = 1;
                var index = 1;

                while (bit < w || bit < h || bit < d)
                {
                    if (bit < w)
                    {
                        X |= index;
                        index <<= 1;
                    }

                    if (bit < h)
                    {
                        Y |= index;
                        index <<= 1;
                    }

                    if (bit < d)
                    {
                        Z |= index;
                        index <<= 1;
                    }

                    bit <<= 1;
                }
            }
        }

        public static byte[] Swizzle(byte[] data, int width, int height, int depth, int bpp) => Swizzle(data, width, height, depth, bpp, true);

        public static byte[] Swizzle(byte[] data, int width, int height, int depth, int bpp, bool deswizzle)
        {
            ArgumentNullException.ThrowIfNull(data);

            int a, b;
            var output = new byte[data.Length];

            var masks = new MaskSet(width, height, depth);
            for (var y = 0; y < height * depth; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (deswizzle)
                    {
                        a = ((y * width) + x) * bpp;
                        b = Swizzle(x, y, depth, masks) * bpp;
                    }
                    else
                    {
                        b = ((y * width) + x) * bpp;
                        a = Swizzle(x, y, depth, masks) * bpp;
                    }

                    if (a < output.Length && b < data.Length)
                    {
                        for (var i = 0; i < bpp; i++)
                            output[a + i] = data[b + i];
                    }
                    else
                        return null;
                }
            }

            return output;
        }

        private static int Swizzle(int x, int y, int z, MaskSet masks) => SwizzleAxis(x, masks.X) | SwizzleAxis(y, masks.Y) | (z == -1 ? 0 : SwizzleAxis(z, masks.Z));

        private static int SwizzleAxis(int val, int mask)
        {
            var bit = 1;
            var result = 0;

            while (bit <= mask)
            {
                var tmp = mask & bit;

                if (tmp != 0)
                    result |= val & bit;
                else
                    val <<= 1;

                bit <<= 1;
            }

            return result;
        }

        #endregion

        #region Xbox 360

        /* https://github.com/gdkchan/MESTool/blob/master/MESTool/Program.cs */
        public static byte[] XTextureScramble(byte[] data, int width, int height, object format) => XTextureScramble(data, width, height, format, false);

        public static byte[] XTextureScramble(byte[] data, int width, int height, object format, bool toLinear)
        {
            ArgumentNullException.ThrowIfNull(data);

            var knownFormat = format.ParseToEnum<KnownTextureFormat>();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            var blockSize = GetLinearBlockSize(knownFormat);
            var texelPitch = GetLinearTexelPitch(knownFormat);
            var bpp = GetBpp(knownFormat);
            var tileSize = GetTileSize(knownFormat);

            width = (int)Math.Ceiling((float)width / tileSize) * tileSize;
            height = (int)Math.Ceiling((float)height / tileSize) * tileSize;

            var expectedSize = width * height * bpp / 8;
            if (expectedSize > data.Length)
                Array.Resize(ref data, expectedSize);

            var output = new byte[data.Length];

            var (xBlocks, yBlocks) = (width / blockSize, height / blockSize);

            for (var i = 0; i < yBlocks; i++)
            {
                for (var j = 0; j < xBlocks; j++)
                {
                    var blockOffset = i * xBlocks + j;

                    var x = XGAddress2DTiledX(blockOffset, xBlocks, texelPitch);
                    var y = XGAddress2DTiledY(blockOffset, xBlocks, texelPitch);

                    var sourceIndex = i * xBlocks * texelPitch + j * texelPitch;
                    var destIndex = y * xBlocks * texelPitch + x * texelPitch;

                    if (toLinear)
                        Array.Copy(data, destIndex, output, sourceIndex, texelPitch);
                    else
                        Array.Copy(data, sourceIndex, output, destIndex, texelPitch);
                }
            }

            return output;
        }

        private static int XGAddress2DTiledX(int offset, int width, int texelPitch)
        {
            var alignedWidth = (width + 31) & ~31;

            var logBPP = (texelPitch >> 2) + (texelPitch >> 1 >> (texelPitch >> 2));
            var offsetB = offset << logBPP;
            var offsetT = ((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2) + (offsetB & 63);
            var offsetM = offsetT >> (7 + logBPP);

            var macroX = (offsetM % (alignedWidth >> 5)) << 2;
            var tile = (((offsetT >> (5 + logBPP)) & 2) + (offsetB >> 6)) & 3;
            var macro = (macroX + tile) << 3;
            var micro = ((((offsetT >> 1) & ~15) + (offsetT & 15)) & ((texelPitch << 3) - 1)) >> logBPP;

            return macro + micro;
        }

        private static int XGAddress2DTiledY(int offset, int width, int texelPitch)
        {
            var alignedWidth = (width + 31) & ~31;

            var logBPP = (texelPitch >> 2) + (texelPitch >> 1 >> (texelPitch >> 2));
            var offsetB = offset << logBPP;
            var offsetT = ((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2) + (offsetB & 63);
            var offsetM = offsetT >> (7 + logBPP);

            var macroY = (offsetM / (alignedWidth >> 5)) << 2;
            var tile = ((offsetT >> (6 + logBPP)) & 1) + ((offsetB & 2048) >> 10);
            var macro = (macroY + tile) << 3;
            var micro = (((offsetT & ((texelPitch << 6) - 1) & ~31) + ((offsetT & 15) << 1)) >> (3 + logBPP)) & ~1;

            return macro + micro + ((offsetT & 16) >> 4);
        }

        #endregion
    }
}
