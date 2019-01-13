using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class bitmap
    {
        [Offset(60)]
        public BlockCollection<Sequence> Sequences { get; set; }

        [Offset(68)]
        public BlockCollection<BitmapData> Bitmaps { get; set; }
    }

    [FixedSize(60)]
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

    [FixedSize(116)]
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

        [Offset(30)]
        public int Lod0Pointer { get; set; }

        [Offset(34)]
        public int Lod1Pointer { get; set; }

        [Offset(38)]
        public int Lod2Pointer { get; set; }

        [Offset(54)]
        public int Lod0Size { get; set; }

        [Offset(58)]
        public int Lod1Size { get; set; }

        [Offset(62)]
        public int Lod2Size { get; set; }
    }
}
