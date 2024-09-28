namespace Reclaimer.Drawing
{
    public static class DrawingExtensions
    {
        public static DxgiFormat ToDxgiFormat(this FourCC fourCC)
        {
            return fourCC switch
            {
                FourCC.ATI1 => DxgiFormat.BC4_UNorm,
                FourCC.ATI2 => DxgiFormat.BC5_UNorm,

                FourCC.BC4U => DxgiFormat.BC4_UNorm,
                FourCC.BC4S => DxgiFormat.BC4_SNorm,

                FourCC.BC5U => DxgiFormat.BC5_UNorm,
                FourCC.BC5S => DxgiFormat.BC5_SNorm,

                FourCC.DXT1 => DxgiFormat.BC1_UNorm,
                FourCC.DXT2 or FourCC.DXT3 => DxgiFormat.BC2_UNorm,
                FourCC.DXT4 or FourCC.DXT5 => DxgiFormat.BC3_UNorm,

                FourCC.RGBG => DxgiFormat.R8G8_B8G8_UNorm,
                FourCC.GRGB => DxgiFormat.G8R8_G8B8_UNorm,

                FourCC.UYVY => DxgiFormat.Unknown,
                FourCC.YUY2 => DxgiFormat.YUY2,
                FourCC.V8U8 => DxgiFormat.R8G8_SNorm,

                _ => DxgiFormat.Unknown
            };
        }

        public static DxgiFormat ToDxgiFormat(this XboxFormat xboxFormat)
        {
            return xboxFormat switch
            {
                XboxFormat.A8 => DxgiFormat.A8_UNorm,

                //same layout as A8 but A is also copied across RGB channels
                XboxFormat.AY8 => DxgiFormat.A8_UNorm,

                //dual channel version of BC1 (no DXGI equivalent)
                XboxFormat.CTX1 => DxgiFormat.Unknown,

                //same layout as BC5 but with auto-calculated blue channel
                XboxFormat.DXN => DxgiFormat.BC5_UNorm,
                XboxFormat.DXN_SNorm => DxgiFormat.BC5_SNorm,

                //same layout as BC5, but G is moved to A, and R is copied to B and G
                XboxFormat.DXN_mono_alpha => DxgiFormat.BC5_UNorm,

                //alpha-only version of BC2 (no DXGI equivalent)
                XboxFormat.DXT3a_scalar or XboxFormat.DXT3a_mono or XboxFormat.DXT3a_alpha => DxgiFormat.Unknown,

                //alpha-only version of BC3 (which happens to be the same layout as BC4)
                XboxFormat.DXT5a_scalar or XboxFormat.DXT5a_mono or XboxFormat.DXT5a_alpha => DxgiFormat.BC4_UNorm,

                XboxFormat.V8U8 => DxgiFormat.R8G8_SNorm,

                //same layout as R8 but R is also copied to B and G
                XboxFormat.Y8 => DxgiFormat.R8_UNorm,

                //same layout as R8G8, but G is moved to A, and R is copied to B and G
                XboxFormat.Y8A8 => DxgiFormat.R8G8_UNorm,

                _ => DxgiFormat.Unknown
            };
        }

        public static PixelFormatDescriptor GetFormatDescriptor(this XboxFormat format)
        {
            var dxgi = ToDxgiFormat(format);
            if (dxgi != DxgiFormat.Unknown)
                return GetFormatDescriptor(dxgi);

            if (format is XboxFormat.CTX1 or XboxFormat.DXT3a_scalar or XboxFormat.DXT3a_mono or XboxFormat.DXT3a_alpha)
            {
                return new PixelFormatDescriptor
                {
                    BitsPerPixel = 4,
                    ReadUnitSize = 2,
                    BlockWidth = 4,
                    BytesPerBlock = 8,
                    PaddingAlignment = 128
                };
            }

            return null;
        }

        public static PixelFormatDescriptor GetFormatDescriptor(this DxgiFormat format)
        {
            var formatString = format.ToString();

            var bpp = format switch
            {
                _ when formatString.StartsWith("R32G32B32A32") => 128,
                _ when formatString.StartsWith("R32G32B32_") => 96,
                _ when formatString.StartsWith("R16G16B16A16") => 64,
                _ when formatString.StartsWith("R32G32_") => 64,
                _ when formatString.StartsWith("R16G16_") => 32,

                _ when formatString.StartsWith("R10G10B10A2") => 32,
                _ when formatString.StartsWith("R8G8B8A8") => 32,
                _ when formatString.StartsWith("B8G8R8A8") => 32,
                _ when formatString.StartsWith("B8G8R8X8") => 32,
                _ when formatString.StartsWith("R8_") => 8,

                _ when formatString.StartsWith("BC1") => 4,
                _ when formatString.StartsWith("BC2") => 8,
                _ when formatString.StartsWith("BC3") => 8,
                _ when formatString.StartsWith("BC4") => 4,
                _ when formatString.StartsWith("BC5") => 8,
                _ when formatString.StartsWith("BC6") => 8,
                _ when formatString.StartsWith("BC7") => 8,

                DxgiFormat.A8_UNorm or DxgiFormat.P8 => 8,

                _ => 16
            };

            var unitSize = format switch
            {
                _ when formatString.StartsWith("R32") => 4,
                _ when formatString.StartsWith("R16") => 2,
                _ when formatString.StartsWith("R8G8B8A8") => 4,
                _ when formatString.StartsWith("B8G8R8A8") => 4,
                _ when formatString.StartsWith("B8G8R8X8") => 4,
                _ when formatString.StartsWith("R8_") => 1,
                DxgiFormat.A8_UNorm or DxgiFormat.P8 => 1,
                _ => 2
            };

            var blockWidth = format switch
            {
                DxgiFormat.BC1_Typeless or DxgiFormat.BC1_UNorm or DxgiFormat.BC1_UNorm_SRGB => 4,
                DxgiFormat.BC2_Typeless or DxgiFormat.BC2_UNorm or DxgiFormat.BC2_UNorm_SRGB => 4,
                DxgiFormat.BC3_Typeless or DxgiFormat.BC3_UNorm or DxgiFormat.BC3_UNorm_SRGB => 4,
                DxgiFormat.BC4_Typeless or DxgiFormat.BC4_UNorm or DxgiFormat.BC4_SNorm => 4,
                DxgiFormat.BC5_Typeless or DxgiFormat.BC5_UNorm or DxgiFormat.BC5_SNorm => 4,
                DxgiFormat.BC6H_Typeless or DxgiFormat.BC6H_UF16 or DxgiFormat.BC6H_SF16 => 4,
                DxgiFormat.BC7_Typeless or DxgiFormat.BC7_UNorm or DxgiFormat.BC7_UNorm_SRGB => 4,
                _ => 1
            };

            var blockBytes = format switch
            {
                _ when formatString.StartsWith("BC1") => 8,
                _ when formatString.StartsWith("BC2") => 16,
                _ when formatString.StartsWith("BC3") => 16,
                _ when formatString.StartsWith("BC4") => 8,
                _ when formatString.StartsWith("BC5") => 16,
                _ when formatString.StartsWith("BC6") => 16,
                _ when formatString.StartsWith("BC7") => 16,
                _ => unitSize
            };

            //this isnt entirely accurate but works for most of the x360 bitmaps
            var alignment = format switch
            {
                DxgiFormat.A8_UNorm => 32, //A8, AY8
                DxgiFormat.B8G8R8A8_UNorm or DxgiFormat.B8G8R8X8_UNorm => 32,
                DxgiFormat.B4G4R4A4_UNorm => 32,
                DxgiFormat.B5G6R5_UNorm => 32,
                DxgiFormat.R8G8_SNorm => 32, //U8V8

                DxgiFormat.R8G8_UNorm => 128, //A8Y8
                DxgiFormat.R8_UNorm => 128, //Y8
                _ when formatString.StartsWith("BC") => 128,

                _ => 1
            };

            return new PixelFormatDescriptor
            {
                BitsPerPixel = bpp,
                ReadUnitSize = unitSize,
                BlockWidth = blockWidth,
                BytesPerBlock = blockBytes,
                PaddingAlignment = alignment
            };
        }
    }
}
