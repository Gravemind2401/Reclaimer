using Reclaimer.Drawing;
using Reclaimer.IO;
using System.Buffers;

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
            { KnownTextureFormat.A8, DxgiFormat.A8_UNorm },
            { KnownTextureFormat.R8G8, DxgiFormat.R8G8_UNorm },
            { KnownTextureFormat.A8R8G8B8, DxgiFormat.B8G8R8A8_UNorm },
            { KnownTextureFormat.X8R8G8B8, DxgiFormat.B8G8R8X8_UNorm },
            { KnownTextureFormat.Q8W8V8U8, DxgiFormat.R8G8B8A8_SNorm },
            { KnownTextureFormat.R16G16B16A16, DxgiFormat.R16G16B16A16_UNorm },
            { KnownTextureFormat.R16G16B16A16_snorm, DxgiFormat.R16G16B16A16_SNorm },
            { KnownTextureFormat.R5G6B5, DxgiFormat.B5G6R5_UNorm },
            { KnownTextureFormat.A1R5G5B5, DxgiFormat.B5G5R5A1_UNorm },
            { KnownTextureFormat.A4R4G4B4, DxgiFormat.B4G4R4A4_UNorm },
            { KnownTextureFormat.R10G10B10A2, DxgiFormat.R10G10B10A2_UNorm },
            { KnownTextureFormat.RGBAFP16, DxgiFormat.R16G16B16A16_Float },
            { KnownTextureFormat.RGBFP32, DxgiFormat.R32G32B32_Float },
            { KnownTextureFormat.RGBAFP32, DxgiFormat.R32G32B32A32_Float }
        };

        private static readonly Dictionary<KnownTextureFormat, XboxFormat> xboxLookup = new Dictionary<KnownTextureFormat, XboxFormat>
        {
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
            { KnownTextureFormat.L16, XboxFormat.L16 },
            { KnownTextureFormat.P8, XboxFormat.Y8 },
            { KnownTextureFormat.P8_bump, XboxFormat.Y8 },
            { KnownTextureFormat.U8V8, XboxFormat.V8U8 },
            { KnownTextureFormat.V8U8, XboxFormat.V8U8 },
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
            R8G8,
            R5G6B5,
            R10G10B10A2,
            A1R5G5B5,
            A4R4G4B4,
            X8R8G8B8,
            A8R8G8B8,
            R16G16B16A16,
            R16G16B16A16_snorm,
            DXT1,
            DXT3,
            DXT5,
            P8_bump,
            P8,
            ARGBFP32, //TODO: should this actually be RGBA instead of ARGB? the games this is defined in have no examples
            RGBAFP16,
            RGBAFP32,
            RGBFP32,
            L16,
            U8V8,
            V8U8,
            Q8W8V8U8,
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
            BC7_unorm
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
            if (input is T enumValue)
                return enumValue;

            if (input != null)
            {
                if (Enum.TryParse(input.ToString(), out enumValue))
                    return enumValue;
            }

            return defaultValue;
        }

        public static byte[] ApplyCrop(byte[] data, DdsImageDescriptor descriptor)
        {
            ArgumentNullException.ThrowIfNull(data);

            var (inWidth, inHeight) = (descriptor.PaddedWidth, descriptor.PaddedHeight);
            var (outWidth, outHeight) = (descriptor.Width, descriptor.Height);

            if (outWidth >= inWidth && outHeight >= inHeight)
                return data;

            var blockLen = (double)descriptor.BlockWidth;
            var blockSize = descriptor.BytesPerBlock;
            var faces = descriptor.FrameCount;

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
            return CreateFormatDescriptor(props, includeMips).PaddedDataLength;
        }

        public static DdsImageDescriptor CreateFormatDescriptor(this BitmapProperties props) => CreateFormatDescriptor(props, false);
        public static DdsImageDescriptor CreateFormatDescriptor(this BitmapProperties props, bool includeMips)
        {
            var bitmapFormat = props.BitmapFormat.ParseToEnum<KnownTextureFormat>();

            DdsImageDescriptor desc;
            if (dxgiLookup.TryGetValue(bitmapFormat, out var dxgiFormat) || Enum.TryParse(props.BitmapFormat?.ToString(), true, out dxgiFormat))
                desc = new DdsImageDescriptor(dxgiFormat, props.Width, props.Height, props.FrameCount, includeMips ? props.MipmapCount : 0);
            else if (xboxLookup.TryGetValue(bitmapFormat, out var xboxFormat) || Enum.TryParse(props.BitmapFormat?.ToString(), true, out xboxFormat))
                desc = new DdsImageDescriptor(xboxFormat, props.Width, props.Height, props.FrameCount, includeMips ? props.MipmapCount : 0);
            else
                throw Exceptions.BitmapFormatNotSupported(props.BitmapFormat.ToString());

            if (!props.UsesPadding)
                (desc.PaddedWidth, desc.PaddedHeight) = (desc.Width, desc.Height);

            //use explicit dimensions if provided
            if (props.VirtualWidth > 0)
                desc.PaddedWidth = props.VirtualWidth;
            if (props.VirtualHeight > 0)
                desc.PaddedHeight = props.VirtualHeight;

            return desc;
        }

        public static DdsImage GetDds(BitmapProperties props, byte[] data, bool includeMips)
        {
            var formatDesc = CreateFormatDescriptor(props, includeMips);

            if (props.ByteOrder == ByteOrder.BigEndian && formatDesc.ReadUnitSize > 1)
            {
                for (var i = 0; i < data.Length - 1; i += formatDesc.ReadUnitSize)
                    Array.Reverse(data, i, formatDesc.ReadUnitSize);
            }

            if (formatDesc.PaddedDataLength > data.Length)
                Array.Resize(ref data, formatDesc.PaddedDataLength);

            if (props.Swizzled)
                XTextureUnscramble(data, formatDesc);

            if (formatDesc.PaddedWidth > props.Width || formatDesc.PaddedHeight > props.Height)
                data = ApplyCrop(data, formatDesc);

            var dds = formatDesc.XboxFormat == default
                ? new DdsImage(props.Height, props.Width, formatDesc.DxgiFormat, data)
                : new DdsImage(props.Height, props.Width, formatDesc.XboxFormat, data);

            var textureType = props.BitmapType.ParseToEnum<KnownTextureType>();
            if (textureType == KnownTextureType.CubeMap)
                dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
            else if (textureType is KnownTextureType.Array or KnownTextureType.Texture3D)
                dds.ArraySize = props.FrameCount;

            if (props.MipmapCount > 0)
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

        public static byte[] Unswizzle(byte[] data, int width, int height, int depth, int bpp) => Swizzle(data, width, height, depth, bpp, true);

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
        public static void XTextureUnscramble(byte[] data, DdsImageDescriptor descriptor) => XTextureScramble(data, descriptor, false);

        public static void XTextureScramble(byte[] data, DdsImageDescriptor descriptor, bool toLinear)
        {
            ArgumentNullException.ThrowIfNull(data);

            var (xBlocks, yBlocks) = (descriptor.BlockCountX, descriptor.BlockCountY);
            var output = ArrayPool<byte>.Shared.Rent(data.Length);

            for (var i = 0; i < yBlocks; i++)
            {
                for (var j = 0; j < xBlocks; j++)
                {
                    var blockOffset = i * xBlocks + j;

                    var x = XGAddress2DTiledX(blockOffset, xBlocks, descriptor.BytesPerBlock);
                    var y = XGAddress2DTiledY(blockOffset, xBlocks, descriptor.BytesPerBlock);

                    var sourceIndex = i * xBlocks * descriptor.BytesPerBlock + j * descriptor.BytesPerBlock;
                    var destIndex = y * xBlocks * descriptor.BytesPerBlock + x * descriptor.BytesPerBlock;

                    if (toLinear)
                        Array.Copy(data, destIndex, output, sourceIndex, descriptor.BytesPerBlock);
                    else
                        Array.Copy(data, sourceIndex, output, destIndex, descriptor.BytesPerBlock);
                }
            }

            output.AsSpan(..data.Length).CopyTo(data);
            ArrayPool<byte>.Shared.Return(output);

            return;

            static int XGAddress2DTiledX(int offset, int width, int texelPitch)
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

            static int XGAddress2DTiledY(int offset, int width, int texelPitch)
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
        }

        #endregion
    }
}
