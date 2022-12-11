using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class bitmap : IBitmap
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public bitmap(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(112)]
        public BlockCollection<SequenceBlock> Sequences { get; set; }

        [Offset(124)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        [Offset(168)]
        public BlockCollection<BitmapResourceBlock> Resources { get; set; }

        [Offset(180)]
        public BlockCollection<BitmapResourceBlock> InterleavedResources { get; set; }

        #region IBitmap

        string IBitmap.SourceFile => item.CacheFile.FileName;

        int IBitmap.Id => item.Id;

        string IBitmap.Name => item.FullPath;

        string IBitmap.Class => item.ClassName;

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => cache.Metadata.IsMcc ? CacheFactory.MccGen3CubeLayout : CacheFactory.Gen3CubeLayout;

        public DdsImage ToDds(int index)
        {
            if (index < 0 || index >= Bitmaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var submap = Bitmaps[index];

            var resource = InterleavedResources.Any()
                ? InterleavedResources[submap.InterleavedIndex].ResourcePointer
                : Resources[index].ResourcePointer;

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, cache.Metadata.Platform == CachePlatform.PC);
            var props = new BitmapProperties(submap.Width, submap.Height, format, submap.BitmapType)
            {
                ByteOrder = cache.ByteOrder,
                UsesPadding = !cache.Metadata.IsMcc,
                Swizzled = submap.Flags.HasFlag(BitmapFlags.Swizzled),
                Depth = submap.BitmapType == TextureType.Texture3D ? submap.Depth : 1,
                FrameCount = submap.BitmapType == TextureType.CubeMap ? 6 : submap.Depth,
                MipmapCount = submap.MipmapCount,
                ArrayMipLayout = cache.Metadata.IsMcc ? MipmapLayout.Fragmented : MipmapLayout.None
            };

            var useMips = cache.Metadata.IsMcc && submap.BitmapType == TextureType.Array;
            var data = resource.ReadData(PageType.Auto, TextureUtils.GetBitmapDataLength(props, useMips));
            return TextureUtils.GetDds(props, data, useMips);
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

    [FixedSize(48)]
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
        public RealVector2 RegPoint { get; set; }
    }

    [FixedSize(44, MaxVersion = (int)CacheType.MccHalo4)]
    [FixedSize(48, MinVersion = (int)CacheType.MccHalo4)]
    public class BitmapDataBlock
    {
        [Offset(0)]
        public short Width { get; set; }

        [Offset(2)]
        public short Height { get; set; }

        [Offset(4)]
        public byte Depth { get; set; }

        [Offset(5)]
        public BitmapFlags Flags { get; set; }

        [Offset(6)]
        public TextureType BitmapType { get; set; }

        [Offset(8)]
        public TextureFormat BitmapFormat { get; set; }

        [Offset(10)]
        public short MoreFlags { get; set; }

        [Offset(12)]
        public short RegX { get; set; }

        [Offset(14)]
        public short RegY { get; set; }

        [Offset(16)]
        public byte MipmapCount { get; set; }

        [Offset(17)]
        public byte Curve { get; set; }

        [Offset(18)]
        public byte InterleavedIndex { get; set; }

        [Offset(19)]
        public byte Index2 { get; set; }

        [Offset(20)]
        public byte PixelsOffset { get; set; }

        [Offset(24)]
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

    public enum TextureType : byte
    {
        Texture2D = 0,
        Texture3D = 1,
        CubeMap = 2,
        Array = 3
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
        Unknown33 = 33,
        Unknown34 = 34,
        Unknown35 = 35,
        Unknown36 = 36,
        Unknown37 = 37,
        DXN = 38,
        CTX1 = 39,
        DXT3a_alpha = 40,
        DXT3a_mono = 41,
        DXT5a_alpha = 42,
        DXT5a_mono = 43,
        DXN_mono_alpha = 44
    }
}
