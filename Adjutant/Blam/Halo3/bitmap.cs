using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Dds;
using System.IO;
using Adjutant.IO;

namespace Adjutant.Blam.Halo3
{
    public class bitmap : IBitmap
    {
        private readonly CacheFile cache;

        public bitmap(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(84)]
        public BlockCollection<SequenceBlock> Sequences { get; set; }

        [Offset(96)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        [Offset(140)]
        public BlockCollection<BitmapResourceBlock> Resources { get; set; }

        [Offset(152)]
        public BlockCollection<BitmapResourceBlock> InterleavedResources { get; set; }

        #region IBitmap

        int IBitmap.BitmapCount => Bitmaps.Count;

        public DdsImage ToDds(int index)
        {
            if (index < 0 || index >= Bitmaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var submap = Bitmaps[index];

            var resource = InterleavedResources.Any()
                ? InterleavedResources[submap.InterleavedIndex].ResourcePointer
                : Resources[index].ResourcePointer;

            var data = resource.ReadData();

            var bpp = submap.BitmapFormat.Bpp();
            for (int i = 0; i < data.Length - 1; i += bpp)
                Array.Reverse(data, i, bpp);

            if (submap.Flags.HasFlag(BitmapFlags.Swizzled))
            {
                var blockSize = submap.BitmapFormat.LinearBlockSize();
                var texelPitch = submap.BitmapFormat.LinearTexelPitch();

                data = TextureUtils.XTextureScramble(data, submap.Width, submap.Height, blockSize, texelPitch, false);
            }

            DxgiFormat dxgi;
            switch (submap.BitmapFormat)
            {
                case TextureFormat.DXT1:
                    dxgi = DxgiFormat.BC1_UNorm;
                    break;

                case TextureFormat.DXT3:
                    dxgi = DxgiFormat.BC2_UNorm;
                    break;

                case TextureFormat.DXT5:
                    dxgi = DxgiFormat.BC3_UNorm;
                    break;

                case TextureFormat.DXT5a:
                    dxgi = DxgiFormat.BC4_UNorm;
                    break;

                case TextureFormat.DXN:
                    dxgi = DxgiFormat.BC5_UNorm;
                    break;

                case TextureFormat.A8R8G8B8:
                    dxgi = DxgiFormat.B8G8R8A8_UNorm;
                    break;

                case TextureFormat.X8R8G8B8:
                    dxgi = DxgiFormat.B8G8R8X8_UNorm;
                    break;

                case TextureFormat.R5G6B5:
                    dxgi = DxgiFormat.B5G6R5_UNorm;
                    break;

                case TextureFormat.A1R5G5B5:
                    dxgi = DxgiFormat.B5G5R5A1_UNorm;
                    break;

                case TextureFormat.A4R4G4B4:
                    dxgi = DxgiFormat.B4G4R4A4_UNorm;
                    break;

                case TextureFormat.P8_bump:
                case TextureFormat.P8:
                    dxgi = DxgiFormat.P8;
                    break;

                default: throw new NotSupportedException();
            }

            return new DdsImage(submap.Height, submap.Width, dxgi, DxgiTextureType.Texture2D, data);
        }

        #endregion
    }

    [FixedSize(64)]
    public class SequenceBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public short FirstSubmapIndex { get; set; }

        [Offset(34)]
        public short BitmapCount { get; set; }

        [Offset(52)]
        public BlockCollection<SpriteBlock> Sprites { get; set; }
    }

    [FixedSize(32)]
    public class SpriteBlock
    {
        [Offset(0)]
        public short SubmapIndex { get; set; }

        [Offset(8)]
        public float Left { get; set; }

        [Offset(12)]
        public float Right { get; set; }

        [Offset(16)]
        public float Top { get; set; }

        [Offset(20)]
        public float Bottom { get; set; }

        [Offset(24)]
        public RealVector2D RegPoint { get; set; }
    }

    [FixedSize(48)]
    public class BitmapDataBlock
    {
        [Offset(0)]
        [FixedLength(4)]
        public string Class { get; set; }

        [Offset(4)]
        public short Width { get; set; }

        [Offset(6)]
        public short Height { get; set; }

        [Offset(8)]
        public byte Depth { get; set; }

        [Offset(9)]
        public BitmapFlags Flags { get; set; }

        [Offset(10)]
        public TextureType BitmapType { get; set; }

        [Offset(12)]
        public TextureFormat BitmapFormat { get; set; }

        [Offset(14)]
        public short MoreFlags { get; set; }

        [Offset(16)]
        public short RegX { get; set; }

        [Offset(18)]
        public short RegY { get; set; }

        [Offset(20)]
        public short MipmapCount { get; set; }

        [Offset(22)]
        public byte InterleavedIndex { get; set; }

        [Offset(23)]
        public byte Index2 { get; set; }

        [Offset(24)]
        public byte PixelsOffset { get; set; }

        [Offset(28)]
        public byte PixelsSize { get; set; }
    }

    [FixedSize(8)]
    public class BitmapResourceBlock
    {
        [Offset(0)]
        public ResourceIdentifier ResourcePointer { get; set; }

        [Offset(4)]
        public int Unknown { get; set; }
    }

    [Flags]
    public enum BitmapFlags : byte
    {
        Swizzled = 8
    }

    public enum TextureType : short
    {
        Texture2D = 0,
        Texture3D = 1,
        CubeMap = 2,
        Sprite = 3,
        UIBitmap = 4
    }

    public enum TextureFormat : short
    {
        A8 = 0,
        Y8 = 1,
        AY8 = 2,
        A8Y8 = 3,
        Unused4 = 4,
        Unused5 = 5,
        R5G6B5 = 6,
        Unused7 = 7,
        A1R5G5B5 = 8,
        A4R4G4B4 = 9,
        X8R8G8B8 = 10,
        A8R8G8B8 = 11,
        Unused12 = 12,
        Unused13 = 13,
        DXT1 = 14,
        DXT3 = 15,
        DXT5 = 16,
        P8_bump = 17,
        P8 = 18,
        ARGBFP32 = 19,
        RGBFP32 = 20,
        RGBFP16 = 21,
        U8V8 = 22,
        Unknown23 = 23,
        Unknown24 = 24,
        Unknown25 = 25,
        Unknown26 = 26,
        Unknown27 = 27,
        Unknown28 = 28,
        Unknown29 = 29,
        Unknown30 = 30,
        DXT5a = 31,
        Unknown32 = 32,
        DXN = 33,
        CTX1 = 34,
        DXT3a_alpha = 35,
        DXT3a_mono = 36,
        DXT5a_alpha = 37,
        DXT5a_mono = 38,
        DXN_mono_alpha = 39,
        Unknown40 = 40,
        Unknown41 = 41,
        Unknown42 = 42,
        Unknown43 = 43,
        Unknown44 = 44
    }
}
