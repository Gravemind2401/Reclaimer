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
        public const uint Size = 32;

        public FormatFlags Flags { get; set; }
        public uint FourCC { get; set; }
        public uint RgbBitCount { get; set; }
        public uint RBitmask { get; set; }
        public uint GBitmask { get; set; }
        public uint BBitmask { get; set; }
        public uint ABitmask { get; set; }
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
