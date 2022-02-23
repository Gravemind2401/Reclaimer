using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-header */
    internal class DdsHeader
    {
        public const int Size = 124;
        public HeaderFlags Flags { get; set; }
        public int Height { get; set; } //required
        public int Width { get; set; }  //required
        public int PitchOrLinearSize { get; set; }
        public int Depth { get; set; }
        public int MipmapCount { get; set; }
        public int[] Reserved1 { get; private set; } //unused
        public DdsPixelFormat PixelFormat { get; private set; }
        public DdsCaps Caps { get; set; }
        public DdsCaps2 Caps2 { get; set; }
        public int Caps3 { get; set; } //unused
        public int Caps4 { get; set; } //unused
        public int Reserved2 { get; set; } //unused 

        public DdsHeader()
        {
            Reserved1 = new int[11];
            PixelFormat = new DdsPixelFormat();
        }

        public void SetFlag(HeaderFlags flag, bool set)
        {
            if (set)
                Flags |= flag;
            else
                Flags &= ~flag;
        }
    }

    [Flags]
    internal enum HeaderFlags
    {
        Default = Caps | Height | Width | PixelFormat,

        Caps = 0x1, //required
        Height = 0x2, //required
        Width = 0x4, //required
        Pitch = 0x8,
        PixelFormat = 0x1000, //required
        MipmapCount = 0x20000,
        LinearSize = 0x80000,
        Depth = 0x800000,
    }

    [Flags]
    internal enum DdsCaps
    {
        Complex = 0x8,
        Texture = 0x1000, //required
        Mipmap = 0x400000
    }

    [Flags]
    internal enum DdsCaps2
    {
        Cubemap = 0x200,
        CubemapPositiveX = 0x400,
        CubemapNegativeX = 0x800,
        CubemapPositiveY = 0x1000,
        CubemapNegativeY = 0x2000,
        CubemapPositiveZ = 0x4000,
        CubemapNegativeZ = 0x8000,
        Volume = 0x200000
    }
}
