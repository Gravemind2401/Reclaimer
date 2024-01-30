using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.Halo5
{
    public class bitmap : ContentTagDefinition<IBitmap>, IBitmap
    {
        public bitmap(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(240)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        #region IContentProvider

        public override IBitmap GetContent() => this;

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;

        public DdsImage ToDds(int index)
        {
            var isChunk = false;

            if (index >= Item.ResourceCount)
                System.Diagnostics.Debugger.Break();

            var resourceIndex = Module.Resources[Item.ResourceIndex] + index;
            var resource = Module.Items[resourceIndex]; //this will be the [bitmap resource handle*] tag
            if (resource.ResourceCount > 0)
            {
                //get the last [bitmap resource handle*.chunk#] tag (mips from smallest to largest?)
                resource = Module.Items.Last(i => i.ParentIndex == resourceIndex);
                isChunk = true;
            }

            var submap = Bitmaps[index];

            byte[] data;
            using (var reader = resource.CreateReader())
            {
                if (!isChunk)
                {
                    if (resource.Flags.HasFlag(FileEntryFlags.HasBlocks))
                        reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagDataSize, SeekOrigin.Begin);
                    else
                        reader.Seek(Header.Header.HeaderSize, SeekOrigin.Begin);
                }

                data = reader.ReadBytes((int)resource.TotalUncompressedSize);
            }

            //todo: cubemap check
            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(submap.Width, submap.Height, format, "Texture2D");
            return TextureUtils.GetDds(props, data, false);
        }

        #endregion
    }

    [FixedSize(40)]
    public class BitmapDataBlock
    {
        [Offset(0)]
        public short Width { get; set; }

        [Offset(2)]
        public short Height { get; set; }

        [Offset(8)]
        public TextureFormat BitmapFormat { get; set; }
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
        ABGRFP32, //r32g32b32a32_float (abgrfp32){abgrfp32}
        ABGRFP16, //r16g16b16a16_float (abgrfp16){abgrfp16}
        Mono_16F, //r16_float_rrr1 (16f_mono){16f_mono}
        Red_16F, //r16_float_r000 (16f_red){16f_red}
        Q8W8V8U8, //r8g8b8a8_snorm (q8w8v8u8){q8w8v8u8}
        A2R10G10B10, //r10g10b10a2_unorm (a2r10g10b10){a2r10g10b10}
        A16B16G16R16, //r16g16b16a16_unorm (a16b16g16r16){a16b16g16r16}
        V16U16, //r16g16_snorm (v16u16){v16u16}
        L16, //r16_unorm_rrr0 (L16){l16}
        R16G16, //r16g16_unorm (r16g16){r16g16}
        Signedr16G16B16A16, //r16g16b16a16_snorm (signedr16g16b16a16){signedr16g16b16a16}
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
