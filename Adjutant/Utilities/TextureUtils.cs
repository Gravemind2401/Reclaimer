using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Adjutant.Utilities
{
    public static class TextureUtils
    {
        #region Lookups

        private static readonly Dictionary<KnownTextureFormat, DxgiFormat> dxgiLookup = new Dictionary<KnownTextureFormat, DxgiFormat>
        {
            { KnownTextureFormat.DXT1, DxgiFormat.BC1_UNorm },
            { KnownTextureFormat.DXT3, DxgiFormat.BC2_UNorm },
            { KnownTextureFormat.DXT5, DxgiFormat.BC3_UNorm },
            { KnownTextureFormat.BC7_unorm, DxgiFormat.BC7_UNorm },
            { KnownTextureFormat.A8R8G8B8, DxgiFormat.B8G8R8A8_UNorm },
            { KnownTextureFormat.X8R8G8B8, DxgiFormat.B8G8R8X8_UNorm },
            { KnownTextureFormat.R5G6B5, DxgiFormat.B5G6R5_UNorm },
            { KnownTextureFormat.A1R5G5B5, DxgiFormat.B5G5R5A1_UNorm },
            { KnownTextureFormat.A4R4G4B4, DxgiFormat.B4G4R4A4_UNorm }
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
            ARGBFP32,
            RGBFP32,
            RGBFP16,
            U8V8,
            DXT5a,
            DXN,
            CTX1,
            DXT3a_alpha,
            DXT3a_mono,
            DXT5a_alpha,
            DXT5a_mono,
            DXN_mono_alpha,
            BC4_unorm, //same as DXT5a
            BC7_unorm
        }

        private static KnownTextureFormat AsKnown(this object format)
        {
            var name = format.ToString();
            KnownTextureFormat common;
            if (Enum.TryParse(name, out common))
                return common;
            else return KnownTextureFormat.Unknown;
        }

        //number of bits used to store each pixel
        private static int GetBpp(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                case KnownTextureFormat.ARGBFP32:
                case KnownTextureFormat.RGBFP32:
                    return 32;

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
                case KnownTextureFormat.DXN_mono_alpha:
                case KnownTextureFormat.BC7_unorm:
                    return 8;

                default: return 16;
            }
        }

        //the size in bytes of each read/write unit
        //ie 32bit uses ints, DXT uses shorts etc. Used for endian swaps.
        private static int GetLinearUnitSize(KnownTextureFormat format)
        {
            switch (format)
            {
                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                    return 4;

                case KnownTextureFormat.A8:
                case KnownTextureFormat.Y8:
                case KnownTextureFormat.AY8:
                case KnownTextureFormat.P8_bump:
                    return 1;

                default: return 2;
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
                case KnownTextureFormat.DXN_mono_alpha:
                    return 4;

                default: return 1;
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
                    return 8;

                case KnownTextureFormat.DXT3:
                case KnownTextureFormat.DXT5:
                case KnownTextureFormat.DXN:
                case KnownTextureFormat.DXN_mono_alpha:
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

                default: return 2;
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

                default: return 1;
            }
        }

        public static int Bpp(this Blam.Halo1.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.Halo2.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.Halo3.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.HaloReach.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.Halo4.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Saber3D.Halo1X.TextureFormat format) => GetBpp(format.AsKnown());

        public static int LinearUnitSize(this Blam.Halo1.TextureFormat format) => GetLinearUnitSize(format.AsKnown());
        public static int LinearUnitSize(this Blam.Halo2.TextureFormat format) => GetLinearUnitSize(format.AsKnown());
        public static int LinearUnitSize(this Blam.Halo3.TextureFormat format) => GetLinearUnitSize(format.AsKnown());
        public static int LinearUnitSize(this Blam.HaloReach.TextureFormat format) => GetLinearUnitSize(format.AsKnown());
        public static int LinearUnitSize(this Blam.Halo4.TextureFormat format) => GetLinearUnitSize(format.AsKnown());
        public static int LinearUnitSize(this Saber3D.Halo1X.TextureFormat format) => GetLinearUnitSize(format.AsKnown());

        public static int LinearBlockSize(this Blam.Halo3.TextureFormat format) => GetLinearBlockSize(format.AsKnown());
        public static int LinearBlockSize(this Blam.HaloReach.TextureFormat format) => GetLinearBlockSize(format.AsKnown());
        public static int LinearBlockSize(this Blam.Halo4.TextureFormat format) => GetLinearBlockSize(format.AsKnown());

        public static int LinearTexelPitch(this Blam.Halo3.TextureFormat format) => GetLinearTexelPitch(format.AsKnown());
        public static int LinearTexelPitch(this Blam.HaloReach.TextureFormat format) => GetLinearTexelPitch(format.AsKnown());
        public static int LinearTexelPitch(this Blam.Halo4.TextureFormat format) => GetLinearTexelPitch(format.AsKnown());

        //round up to the nearst valid size, accounting for block sizes and tile sizes
        public static void GetVirtualSize(object format, int width, int height, int faces, out int virtualWidth, out int virtualHeight)
        {
            var knownFormat = format.AsKnown();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            double blockSize = GetLinearBlockSize(knownFormat);
            double tileSize = GetTileSize(knownFormat);

            virtualWidth = (int)(Math.Ceiling(width / tileSize) * tileSize);
            virtualHeight = (int)(Math.Ceiling(height / tileSize) * tileSize) * faces;

            virtualWidth = (int)(Math.Ceiling(virtualWidth / blockSize) * blockSize);
            virtualHeight = (int)(Math.Ceiling(virtualHeight / blockSize) * blockSize);
        }

        public static byte[] ApplyCrop(byte[] data, object format, int faces, int inWidth, int inHeight, int outWidth, int outHeight)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (outWidth >= inWidth && outHeight >= inHeight)
                return data;

            var knownFormat = format.AsKnown();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            double blockLen = GetLinearBlockSize(knownFormat);
            var blockSize = GetLinearTexelPitch(knownFormat);

            var inRows = (int)Math.Ceiling(inHeight / blockLen) / faces;
            var outRows = (int)Math.Ceiling(outHeight / blockLen) / faces;
            var inStride = (int)Math.Ceiling(inWidth / blockLen) * blockSize;
            var outStride = (int)Math.Ceiling(outWidth / blockLen) * blockSize;

            var output = new byte[outRows * outStride * faces];
            for (int f = 0; f < faces; f++)
            {
                var srcTileStart = inRows * inStride * f;
                var destTileStart = outRows * outStride * f;

                for (int s = 0; s < outRows; s++)
                    Array.Copy(data, srcTileStart + inStride * s, output, destTileStart + outStride * s, outStride);
            }

            return output;
        }

        public static DdsImage GetDds(int height, int width, object format, bool isCubemap, byte[] data, bool isPC = false)
        {
            var knownFormat = format.AsKnown();
            if (knownFormat == KnownTextureFormat.Unknown)
                throw new ArgumentException("Could not translate to a known texture format.", nameof(format));

            DdsImage dds;
            if (isPC && knownFormat == KnownTextureFormat.DXN)
                dds = new DdsImage(height, width, XboxFormat.DXN_SNorm, DxgiTextureType.Texture2D, data);
            else if (dxgiLookup.ContainsKey(knownFormat))
                dds = new DdsImage(height, width, dxgiLookup[knownFormat], DxgiTextureType.Texture2D, data);
            else if (xboxLookup.ContainsKey(knownFormat))
                dds = new DdsImage(height, width, xboxLookup[knownFormat], DxgiTextureType.Texture2D, data);
            else throw Exceptions.BitmapFormatNotSupported(format.ToString());

            if (isCubemap)
            {
                dds.TextureFlags = TextureFlags.DdsSurfaceFlagsCubemap;
                dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
                dds.DX10ResourceFlags = D3D10ResourceMiscFlags.TextureCube;
            }

            return dds;
        }

        #endregion

        #region Original Xbox

        /* http://www.h2maps.net/Tools/Xbox/Mutation/Mutation/DDS/Swizzle.cs */

        private class MaskSet
        {
            public readonly int x;
            public readonly int y;
            public readonly int z;

            public MaskSet(int w, int h, int d)
            {
                int bit = 1;
                int index = 1;

                while (bit < w || bit < h || bit < d)
                {
                    if (bit < w)
                    {
                        x |= index;
                        index <<= 1;
                    }

                    if (bit < h)
                    {
                        y |= index;
                        index <<= 1;
                    }

                    if (bit < d)
                    {
                        z |= index;
                        index <<= 1;
                    }

                    bit <<= 1;
                }
            }
        }

        public static byte[] Swizzle(byte[] data, int width, int height, int depth, int bpp)
        {
            return Swizzle(data, width, height, depth, bpp, true);
        }

        public static byte[] Swizzle(byte[] data, int width, int height, int depth, int bpp, bool deswizzle)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int a = 0, b = 0;
            var output = new byte[data.Length];

            var masks = new MaskSet(width, height, depth);
            for (int y = 0; y < height * depth; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (deswizzle)
                    {
                        a = ((y * width) + x) * bpp;
                        b = (Swizzle(x, y, depth, masks)) * bpp;
                    }
                    else
                    {
                        b = ((y * width) + x) * bpp;
                        a = (Swizzle(x, y, depth, masks)) * bpp;
                    }

                    if (a < output.Length && b < data.Length)
                    {
                        for (int i = 0; i < bpp; i++)
                            output[a + i] = data[b + i];
                    }
                    else return null;
                }
            }

            return output;
        }

        private static int Swizzle(int x, int y, int z, MaskSet masks)
        {
            return SwizzleAxis(x, masks.x) | SwizzleAxis(y, masks.y) | (z == -1 ? 0 : SwizzleAxis(z, masks.z));
        }

        private static int SwizzleAxis(int val, int mask)
        {
            int bit = 1;
            int result = 0;

            while (bit <= mask)
            {
                int tmp = mask & bit;

                if (tmp != 0) result |= (val & bit);
                else val <<= 1;

                bit <<= 1;
            }

            return result;
        }

        #endregion

        #region Xbox 360

        /* https://github.com/gdkchan/MESTool/blob/master/MESTool/Program.cs */
        public static byte[] XTextureScramble(byte[] data, int width, int height, object format)
        {
            return XTextureScramble(data, width, height, format, false);
        }

        public static byte[] XTextureScramble(byte[] data, int width, int height, object format, bool toLinear)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var knownFormat = format.AsKnown();
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

            int xBlocks = width / blockSize;
            int yBlocks = height / blockSize;

            for (int i = 0; i < yBlocks; i++)
            {
                for (int j = 0; j < xBlocks; j++)
                {
                    int blockOffset = i * xBlocks + j;

                    int x = XGAddress2DTiledX(blockOffset, xBlocks, texelPitch);
                    int y = XGAddress2DTiledY(blockOffset, xBlocks, texelPitch);

                    int sourceIndex = i * xBlocks * texelPitch + j * texelPitch;
                    int destIndex = y * xBlocks * texelPitch + x * texelPitch;

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
            int alignedWidth = (width + 31) & ~31;

            int logBPP = (texelPitch >> 2) + ((texelPitch >> 1) >> (texelPitch >> 2));
            int offsetB = offset << logBPP;
            int offsetT = ((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2) + (offsetB & 63);
            int offsetM = offsetT >> (7 + logBPP);

            int macroX = (offsetM % (alignedWidth >> 5)) << 2;
            int tile = (((offsetT >> (5 + logBPP)) & 2) + (offsetB >> 6)) & 3;
            int macro = (macroX + tile) << 3;
            int micro = ((((offsetT >> 1) & ~15) + (offsetT & 15)) & ((texelPitch << 3) - 1)) >> logBPP;

            return macro + micro;
        }

        private static int XGAddress2DTiledY(int offset, int width, int texelPitch)
        {
            int alignedWidth = (width + 31) & ~31;

            int logBPP = (texelPitch >> 2) + ((texelPitch >> 1) >> (texelPitch >> 2));
            int offsetB = offset << logBPP;
            int offsetT = ((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2) + (offsetB & 63);
            int offsetM = offsetT >> (7 + logBPP);

            int macroY = (offsetM / (alignedWidth >> 5)) << 2;
            int tile = ((offsetT >> (6 + logBPP)) & 1) + ((offsetB & 2048) >> 10);
            int macro = (macroY + tile) << 3;
            int micro = (((offsetT & (((texelPitch << 6) - 1) & ~31)) + ((offsetT & 15) << 1)) >> (3 + logBPP)) & ~1;

            return macro + micro + ((offsetT & 16) >> 4);
        }

        #endregion
    }
}
