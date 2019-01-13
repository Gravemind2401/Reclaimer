using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-pixelformat */
    internal class DdsPixelFormat
    {
        public const int Size = 32;

        public FormatFlags Flags { get; set; }
        public int FourCC { get; set; }
        public int RgbBitCount { get; set; }
        public int RBitmask { get; set; }
        public int GBitmask { get; set; }
        public int BBitmask { get; set; }
        public int ABitmask { get; set; }
    }

    [Flags]
    internal enum FormatFlags
    {
        AlphaPixels = 0x1,
        Alpha = 0x2,
        FourCC = 0x4,
        Rgb = 0x40,
        Yuv = 0x200,
        Luminance = 0x20000
    }
}
