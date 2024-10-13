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

                //same layout as R8G8 but with auto-calculated blue channel
                XboxFormat.V8U8 => DxgiFormat.R8G8_SNorm,

                //same layout as R8 but R is also copied to B and G
                XboxFormat.Y8 => DxgiFormat.R8_UNorm,

                //same layout as R8G8, but G is moved to A, and R is copied to B and G
                XboxFormat.Y8A8 => DxgiFormat.R8G8_UNorm,

                _ => DxgiFormat.Unknown
            };
        }
    }
}
