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

        //bytes, not bits
        public static int Bpp(this Blam.Halo1.TextureFormat format)
        {
            switch (format)
            {
                case Blam.Halo1.TextureFormat.A8R8G8B8:
                case Blam.Halo1.TextureFormat.X8R8G8B8:
                    return 4;

                case Blam.Halo1.TextureFormat.A8:
                case Blam.Halo1.TextureFormat.Y8:
                case Blam.Halo1.TextureFormat.AY8:
                case Blam.Halo1.TextureFormat.P8_bump:
                    return 1;

                default: return 2;
            }
        }

        public static int Bpp(this Blam.Halo2.TextureFormat format)
        {
            switch (format)
            {
                case Blam.Halo2.TextureFormat.A8R8G8B8:
                case Blam.Halo2.TextureFormat.X8R8G8B8:
                    return 4;

                case Blam.Halo2.TextureFormat.A8:
                case Blam.Halo2.TextureFormat.Y8:
                case Blam.Halo2.TextureFormat.AY8:
                case Blam.Halo2.TextureFormat.P8_bump:
                    return 1;

                default: return 2;
            }
        }

        public static int Bpp(this Blam.Halo3.TextureFormat format)
        {
            switch (format)
            {
                case Blam.Halo3.TextureFormat.A8R8G8B8:
                case Blam.Halo3.TextureFormat.X8R8G8B8:
                    return 4;

                case Blam.Halo3.TextureFormat.A8:
                case Blam.Halo3.TextureFormat.Y8:
                case Blam.Halo3.TextureFormat.AY8:
                case Blam.Halo3.TextureFormat.P8_bump:
                    return 1;

                default: return 2;
            }
        }

        public static int LinearBlockSize(this Blam.Halo3.TextureFormat format)
        {
            switch (format)
            {
                case Blam.Halo3.TextureFormat.DXT5a_mono:
                case Blam.Halo3.TextureFormat.DXT5a_alpha:
                case Blam.Halo3.TextureFormat.DXT1:
                case Blam.Halo3.TextureFormat.CTX1:
                case Blam.Halo3.TextureFormat.DXT5a:
                case Blam.Halo3.TextureFormat.DXT3a_alpha:
                case Blam.Halo3.TextureFormat.DXT3a_mono:
                case Blam.Halo3.TextureFormat.DXT3:
                case Blam.Halo3.TextureFormat.DXT5:
                case Blam.Halo3.TextureFormat.DXN:
                case Blam.Halo3.TextureFormat.DXN_mono_alpha:
                    return 4;

                default: return 1;
            }
        }

        public static int LinearTexelPitch(this Blam.Halo3.TextureFormat format)
        {
            switch (format)
            {
                case Blam.Halo3.TextureFormat.DXT5a_mono:
                case Blam.Halo3.TextureFormat.DXT5a_alpha:
                case Blam.Halo3.TextureFormat.DXT1:
                case Blam.Halo3.TextureFormat.CTX1:
                case Blam.Halo3.TextureFormat.DXT5a:
                case Blam.Halo3.TextureFormat.DXT3a_alpha:
                case Blam.Halo3.TextureFormat.DXT3a_mono:
                    return 8;

                case Blam.Halo3.TextureFormat.DXT3:
                case Blam.Halo3.TextureFormat.DXT5:
                case Blam.Halo3.TextureFormat.DXN:
                case Blam.Halo3.TextureFormat.DXN_mono_alpha:
                    return 16;

                case Blam.Halo3.TextureFormat.AY8:
                case Blam.Halo3.TextureFormat.Y8:
                    return 1;

                case Blam.Halo3.TextureFormat.A8R8G8B8:
                case Blam.Halo3.TextureFormat.X8R8G8B8:
                    return 4;

                default: return 2;
            }
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

        public static byte[] Swizzle(byte[] data, int width, int height, int depth, int bpp, bool deswizzle = true)
        {
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

        public static byte[] XTextureScramble(byte[] data, int width, int height, int blockSize, int texelPitch, bool toLinear = false)
        {
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

            var identical = Enumerable.Range(0, data.Length).All(i => data[i] == output[i]);

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
