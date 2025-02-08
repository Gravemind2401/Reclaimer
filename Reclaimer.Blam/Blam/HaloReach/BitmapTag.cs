using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class BitmapTag : ContentTagDefinition<IBitmap>, IBitmap
    {
        public BitmapTag(IIndexItem item)
            : base(item)
        { }

        [Offset(96, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(112, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<SequenceBlock> Sequences { get; set; }

        [Offset(108, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(124, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        [Offset(152, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(168, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<BitmapResourceBlock> Resources { get; set; }

        [Offset(164, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(180, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<BitmapResourceBlock> InterleavedResources { get; set; }

        #region IContentProvider

        public override IBitmap GetContent() => this;

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => Cache.Metadata.IsMcc ? CacheFactory.MccGen3CubeLayout : CacheFactory.Gen3CubeLayout;

        float IBitmap.GetSubmapGamma(int index)
        {
            Exceptions.ThrowIfIndexOutOfRange(index, Bitmaps.Count);

            return Bitmaps[index].Curve switch
            {
                ColorSpace.Linear => 1f,
                ColorSpace.Gamma2 => 2f,
                ColorSpace.sRGB => 2.2f,
                _ => 1.95f //xRGB, Unknown
            };
        }

        public DdsImage ToDds(int index)
        {
            Exceptions.ThrowIfIndexOutOfRange(index, Bitmaps.Count);

            var submap = Bitmaps[index];

            var resource = InterleavedResources.Any()
                ? InterleavedResources[submap.InterleavedIndex].ResourcePointer
                : Resources[index].ResourcePointer;

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, Cache.Metadata.Platform == CachePlatform.PC);
            var props = new BitmapProperties(submap.Width, submap.Height, format, submap.BitmapType)
            {
                ByteOrder = Cache.ByteOrder,
                UsesPadding = !Cache.Metadata.IsMcc,
                Swizzled = submap.Flags.HasFlag(BitmapFlags.Swizzled),
                Depth = submap.BitmapType == TextureType.Texture3D ? submap.Depth : 1,
                FrameCount = submap.BitmapType == TextureType.CubeMap ? 6 : submap.Depth,
                MipmapCount = submap.MipmapCount,
                ArrayMipLayout = Cache.Metadata.IsMcc ? MipmapLayout.Fragmented : MipmapLayout.None
            };

            var useMips = Cache.Metadata.IsMcc && submap.BitmapType == TextureType.Array;
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

    [FixedSize(32, MaxVersion = (int)CacheType.HaloReachRetail)]
    [FixedSize(48, MinVersion = (int)CacheType.HaloReachRetail)]
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

    [FixedSize(48, MaxVersion = (int)CacheType.HaloReachRetail)]
    [FixedSize(44, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
    [FixedSize(56, MinVersion = (int)CacheType.MccHaloReach)]
    public class BitmapDataBlock
    {
        [Offset(0)]
        [FixedLength(4)]
        [VersionSpecific((int)CacheType.HaloReachBeta)]
        public string Class { get; set; }

        [Offset(4, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(0, MinVersion = (int)CacheType.HaloReachRetail)]
        public short Width { get; set; }

        [Offset(6, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(2, MinVersion = (int)CacheType.HaloReachRetail)]
        public short Height { get; set; }

        [Offset(8, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(4, MinVersion = (int)CacheType.HaloReachRetail)]
        public byte Depth { get; set; }

        [Offset(9, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(5, MinVersion = (int)CacheType.HaloReachRetail)]
        public BitmapFlags Flags { get; set; }

        [Offset(10, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(6, MinVersion = (int)CacheType.HaloReachRetail)]
        [StoreType(typeof(byte))]
        public TextureType BitmapType { get; set; }

        [Offset(12, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(8, MinVersion = (int)CacheType.HaloReachRetail)]
        public TextureFormat BitmapFormat { get; set; }

        [Offset(14, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(10, MinVersion = (int)CacheType.HaloReachRetail)]
        public short MoreFlags { get; set; }

        [Offset(16, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(12, MinVersion = (int)CacheType.HaloReachRetail)]
        public short RegX { get; set; }

        [Offset(18, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(14, MinVersion = (int)CacheType.HaloReachRetail)]
        public short RegY { get; set; }

        [Offset(20, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(16, MinVersion = (int)CacheType.HaloReachRetail)]
        public byte MipmapCount { get; set; }

        [Offset(21, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(17, MinVersion = (int)CacheType.HaloReachRetail)]
        public ColorSpace Curve { get; set; }

        [Offset(22, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(18, MinVersion = (int)CacheType.HaloReachRetail)]
        public byte InterleavedIndex { get; set; }

        [Offset(23, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(19, MinVersion = (int)CacheType.HaloReachRetail)]
        public byte Index2 { get; set; }

        [Offset(24, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(20, MinVersion = (int)CacheType.HaloReachRetail)]
        public byte PixelsOffset { get; set; }

        [Offset(28, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(24, MinVersion = (int)CacheType.HaloReachRetail)]
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

    public enum ColorSpace : byte
    {
        Unknown = 0, //same as xRGB
        xRGB = 1, //1.95
        Gamma2 = 2, //2.0
        Linear = 3, //1.0
        OffsetLog = 4, //???
        sRGB = 5 //2.2
    }
}
