using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Drawing;
using System.IO;

namespace Reclaimer.Blam.Halo1
{
    public class bitmap : ContentTagDefinition, IBitmap
    {
        public bitmap(IIndexItem item)
            : base(item)
        { }

        [Offset(84)]
        public BlockCollection<SequenceBlock> Sequences { get; set; }

        [Offset(96)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        #region IBitmap

        private static readonly CubemapLayout Halo1CubeLayout = new CubemapLayout
        {
            Face1 = CubemapFace.Right,
            Face2 = CubemapFace.Back,
            Face3 = CubemapFace.Left,
            Face4 = CubemapFace.Front,
            Face5 = CubemapFace.Top,
            Face6 = CubemapFace.Bottom,
            Orientation1 = RotateFlipType.Rotate270FlipNone,
            Orientation2 = RotateFlipType.Rotate180FlipNone,
            Orientation3 = RotateFlipType.Rotate90FlipNone,
            Orientation6 = RotateFlipType.Rotate180FlipNone
        };

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => Halo1CubeLayout;

        public DdsImage ToDds(int index)
        {
            if (index < 0 || index >= Bitmaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var submap = Bitmaps[index];

            var dir = Directory.GetParent(Cache.FileName).FullName;
            var bitmapSource = Path.Combine(dir, CacheFile.BitmapsMap);

            //Xbox maps and player-made CE maps use internal bitmap resources
            if (Cache.CacheType == CacheType.Halo1Xbox
                || (Cache.CacheType == CacheType.Halo1CE && Item.MetaPointer.Address > 0 && !submap.Flags.HasFlag(BitmapFlags.External))
                || (Cache.CacheType == CacheType.MccHalo1 && submap.Flags == BitmapFlags.MccInternal))
                bitmapSource = Cache.FileName;

            byte[] data;
            using (var fs = new FileStream(bitmapSource, FileMode.Open, FileAccess.Read))
            using (var reader = new DependencyReader(fs, ByteOrder.LittleEndian))
            {
                reader.Seek(submap.PixelsOffset, SeekOrigin.Begin);
                data = reader.ReadBytes(submap.PixelsSize);
            }

            var type = submap.BitmapType == TextureType.CubeMap ? submap.BitmapType : TextureType.Texture2D;
            var props = new BitmapProperties(submap.Width, submap.Height, submap.BitmapFormat, type)
            {
                ByteOrder = Cache.ByteOrder,
                Depth = submap.BitmapType == TextureType.Texture3D ? submap.Depth : 1,
                FrameCount = submap.BitmapType == TextureType.CubeMap ? 6 : submap.Depth,
                MipmapCount = submap.MipmapCount,
                CubeMipLayout = MipmapLayout.Contiguous
            };

            return TextureUtils.GetDds(props, data, submap.BitmapType != TextureType.CubeMap);
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
        public RealVector2 RegPoint { get; set; }
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
        public short Depth { get; set; }

        [Offset(10)]
        public TextureType BitmapType { get; set; }

        [Offset(12)]
        public TextureFormat BitmapFormat { get; set; }

        [Offset(14)]
        public BitmapFlags Flags { get; set; }

        [Offset(16)]
        public short RegX { get; set; }

        [Offset(18)]
        public short RegY { get; set; }

        [Offset(20)]
        public short MipmapCount { get; set; }

        [Offset(24)]
        public int PixelsOffset { get; set; }

        [Offset(28)]
        public int PixelsSize { get; set; }
    }

    [Flags]
    public enum BitmapFlags : short
    {
        //haven't seen any Halo1 bitmaps with the swizzle flag, likely only used on xbox
        Swizzled = 8,
        External = 256,

        //hack, but this combination doesnt seem to be used on any external bitmaps
        MccInternal = 641
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
        U8V8 = 22
    }
}
