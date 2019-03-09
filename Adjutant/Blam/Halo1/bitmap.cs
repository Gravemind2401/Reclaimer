using Adjutant.IO;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class bitmap : IBitmap
    {
        private readonly CacheFile cache;

        public bitmap(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(84)]
        public BlockCollection<Sequence> Sequences { get; set; }

        [Offset(96)]
        public BlockCollection<BitmapData> Bitmaps { get; set; }

        #region IBitmap

        int IBitmap.BitmapCount => Bitmaps.Count;

        public DdsImage ToDds(int index)
        {
            if (index < 0 || index >= Bitmaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var submap = Bitmaps[index];

            var dir = Directory.GetParent(cache.FileName).FullName;
            var bitmapsMap = Path.Combine(dir, "bitmaps.map");

            byte[] data;

            using (var fs = new FileStream(bitmapsMap, FileMode.Open, FileAccess.Read))
            using (var reader = new DependencyReader(fs, ByteOrder.LittleEndian))
            {
                reader.Seek(submap.PixelsOffset, SeekOrigin.Begin);
                data = reader.ReadBytes(submap.PixelsSize);
            }

            //FourCC fourCC;
            DxgiFormat dxgi;
            switch (submap.BitmapFormat)
            {
                case 14:
                    //fourCC = FourCC.DXT1;
                    dxgi = DxgiFormat.BC1_UNorm;
                    break;
                case 15:
                    //fourCC = FourCC.DXT3;
                    dxgi = DxgiFormat.BC2_UNorm;
                    break;
                case 16:
                    //fourCC = FourCC.DXT5;
                    dxgi = DxgiFormat.BC3_UNorm;
                    break;

                default: throw new NotSupportedException();
            }

            //return new DdsImage(submap.Height, submap.Width, fourCC, data);
            return new DdsImage(submap.Height, submap.Width, dxgi, DxgiTextureType.Texture2D, data);
        } 

        #endregion
    }

    [FixedSize(64)]
    public class Sequence
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public short FirstSubmapIndex { get; set; }

        [Offset(34)]
        public short BitmapCount { get; set; }

        [Offset(52)]
        public BlockCollection<Sprite> Sprites { get; set; }
    }

    [FixedSize(32)]
    public class Sprite
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
    public class BitmapData
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
        public short BitmapType { get; set; }

        [Offset(12)]
        public short BitmapFormat { get; set; }

        [Offset(14)]
        public short Flags { get; set; }

        [Offset(16)]
        public short RegX { get; set; }

        [Offset(18)]
        public short RegY { get; set; }

        [Offset(20)]
        public int MipmapCount { get; set; }

        [Offset(24)]
        public int PixelsOffset { get; set; }

        [Offset(28)]
        public int PixelsSize { get; set; }
    }
}
