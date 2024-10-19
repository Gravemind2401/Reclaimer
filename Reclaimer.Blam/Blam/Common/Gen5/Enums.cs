namespace Reclaimer.Blam.Common.Gen5
{
    public enum ModuleType : int
    {
        Halo5Server = 23,
        Halo5Forge = 27,
        HaloInfinite = 53
    }

    public enum StructureType : short
    {
        Main = 0,
        TagBlock = 1,
        Resource = 2,
        Custom = 3,
        Literal = 4
    }

    [Flags]
    public enum FileEntryFlags : byte
    {
        Compressed = 1 << 0,
        HasBlocks = 1 << 1,
        RawFile = 1 << 2
    }

    public enum TextureFormat : short
    {
        A8, //a8_unorm (000A){a8}
        Y8, //r8_unorm_rrr1 (RRR1){y8}
        AY8, //r8_unorm_rrrr (RRRR){ay8}
        R8G8, //r8g8_unorm_rrrg (RRRG){r8g8_unorm_gggr (GGGR)}
        Unused1,
        Unused2,
        R5G6B5, //b5g6r5_unorm{r5g6b5}
        Unused3,
        A1R5G5B5, //b5g6r5a1_unorm{a1r5g5b5}
        A4R4G4B4, //b4g4r4a4_unorm{a4r4g4b4}
        X8R8G8B8, //b8g8r8x8_unorm{x8r8g8b8}
        A8R8G8B8, //b8g8r8a8_unorm{a8r8g8b8}
        Unused4,
        DXT5_bias_alpha, //DEPRECATED_dxt5_bias_alpha{dxt5_bias_alpha}
        DXT1, //bc1_unorm (dxt1){dxt1}
        DXT3, //bc2_unorm (dxt3){dxt3}
        DXT5, //bc3_unorm (dxt5){dxt5}
        A4R4G4B4_font, //DEPRECATED_a4r4g4b4_font{a4r4g4b4 font}
        Unused7,
        Unused8,
        Software_RGBFP32, //DEPRECATED_SOFTWARE_rgbfp32{software rgbfp32}
        Unused9,
        V8U8, //r8g8_snorm (v8u8){v8u8}
        G8B8, //DEPRECATED_g8b8{g8b8}
        RGBAFP32, //r32g32b32a32_float (abgrfp32){abgrfp32}
        RGBAFP16, //r16g16b16a16_float (abgrfp16){abgrfp16}
        Mono_16F, //r16_float_rrr1 (16f_mono){16f_mono}
        Red_16F, //r16_float_r000 (16f_red){16f_red}
        Q8W8V8U8, //r8g8b8a8_snorm (q8w8v8u8){q8w8v8u8}
        A2R10G10B10, //r10g10b10a2_unorm (a2r10g10b10){a2r10g10b10}
        A16B16G16R16, //r16g16b16a16_unorm (a16b16g16r16){a16b16g16r16}
        V16U16, //r16g16_snorm (v16u16){v16u16}
        L16, //r16_unorm_rrr0 (L16){l16}
        R16G16, //r16g16_unorm (r16g16){r16g16}
        SignedR16G16B16A16, //r16g16b16a16_snorm (signedr16g16b16a16){signedr16g16b16a16}
        DXT3a, //DEPRECATED_dxt3a{dxt3a}
        BC4_unorm, //bc4_unorm_rrrr (dxt5a){bc4_unorm (dxt5a)}
        BC4_snorm, //bc4_snorm_rrrr
        DXT3a_1111, //DEPRECATED_dxt3a_1111{dxt3a_1111}
        DXN, //bc5_snorm (dxn){dxn}
        CTX1, //DEPRECATED_ctx1{ctx1}
        DXT3a_alpha, //DEPRECATED_dxt3a_alpha_only{dxt3a_alpha}
        DXT3a_mono, //DEPRECATED_dxt3a_monochrome_only{dxt3a_mono}
        DXT5a_alpha, //bc4_unorm_000r (dxt5a_alpha){dxt5a_alpha}
        DXT5a_mono, //bc4_unorm_rrr1 (dxt5a_mono){dxt5a_mono}
        DXN_mono_alpha, //bc5_unorm_rrrg (dxn_mono_alpha){dxn_mono_alpha}
        BC5_snorm, //bc5_snorm_rrrg (dxn_mono_alpha signed)
        BC6H_UF16, //bc6h_uf16 {dxt5_red}
        BC6H_SF16, //bc6h_sf16 {dxt5_green}
        BC7_unorm, //bc7_unorm {dxt5_blue}
        Depth_24, //d24_unorm_s8_uint (depth 24){depth 24}
        R11G11B10_float, //r11g11b10_float
    }
}
