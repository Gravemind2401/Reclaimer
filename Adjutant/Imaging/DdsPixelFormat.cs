using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Imaging
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-pixelformat */
    [FixedSize(32)]
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct DdsPixelFormat
    {
        [Offset(0)]
        [DataLength]
        public uint dwSize { get; set; } //must be 32

        [Offset(4)]
        public FormatFlags dwFlags { get; set; }

        [Offset(8)]
        public uint dwFourCC { get; set; }

        [Offset(12)]
        public uint dwRGBBitCount { get; set; }

        [Offset(16)]
        public uint dwRBitMask { get; set; }

        [Offset(20)]
        public uint dwGBitMask { get; set; }

        [Offset(24)]
        public uint dwBBitMask { get; set; }

        [Offset(28)]
        public uint dwABitMask { get; set; }
    };

    [Flags]
    public enum FormatFlags
    {
        DDPF_ALPHAPIXELS = 0x1,
        DDPF_ALPHA = 0x2,
        DDPF_FOURCC = 0x4,
        DDPF_RGB = 0x40,
        DDPF_YUV = 0x200,
        DDPF_LUMINANCE = 0x20000
    }
}
