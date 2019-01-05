using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-pixelformat */
    [StructLayout(LayoutKind.Sequential)]
    public struct DdsPixelFormat
    {
        public uint dwSize { get; set; } //must be 32
        public FormatFlags dwFlags { get; set; }
        public uint dwFourCC { get; set; }
        public uint dwRGBBitCount { get; set; }
        public uint dwRBitMask { get; set; }
        public uint dwGBitMask { get; set; }
        public uint dwBBitMask { get; set; }
        public uint dwABitMask { get; set; }
    }

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
