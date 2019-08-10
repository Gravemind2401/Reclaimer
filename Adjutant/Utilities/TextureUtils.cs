using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Adjutant.Utilities
{
    public static class TextureUtils
    {
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
            DXN_mono_alpha
        }

        private static KnownTextureFormat AsKnown(this Enum format)
        {
            var name = format.ToString();
            KnownTextureFormat common;
            if (Enum.TryParse(name, out common))
                return common;
            else return KnownTextureFormat.Unknown;
        }

        //bytes, not bits
        private static int GetBpp(KnownTextureFormat format)
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

                case KnownTextureFormat.AY8:
                case KnownTextureFormat.Y8:
                    return 1;

                case KnownTextureFormat.A8R8G8B8:
                case KnownTextureFormat.X8R8G8B8:
                    return 4;

                default: return 2;
            }
        }

        public static int Bpp(this Blam.Halo1.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.Halo2.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.Halo3.TextureFormat format) => GetBpp(format.AsKnown());
        public static int Bpp(this Blam.HaloReach.TextureFormat format) => GetBpp(format.AsKnown());

        public static int LinearBlockSize(this Blam.Halo3.TextureFormat format) => GetLinearBlockSize(format.AsKnown());
        public static int LinearBlockSize(this Blam.HaloReach.TextureFormat format) => GetLinearBlockSize(format.AsKnown());

        public static int LinearTexelPitch(this Blam.Halo3.TextureFormat format) => GetLinearTexelPitch(format.AsKnown());
        public static int LinearTexelPitch(this Blam.HaloReach.TextureFormat format) => GetLinearTexelPitch(format.AsKnown());

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
        public static byte[] XTextureScramble(byte[] data, int width, int height, int blockSize, int texelPitch)
        {
            return XTextureScramble(data, width, height, blockSize, texelPitch, false);
        }

        public static byte[] XTextureScramble(byte[] data, int width, int height, int blockSize, int texelPitch, bool toLinear)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

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
