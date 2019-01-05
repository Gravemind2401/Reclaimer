using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-header */
    public class DdsHeader
    {
        private const uint dwSize = 124;
        private readonly uint[] dwReserved1;
        private readonly DdsPixelFormat ddspf;

        public static uint Size => dwSize;
        public HeaderFlags Flags { get; set; }
        public uint Height { get; set; } //required
        public uint Width { get; set; } //required
        public uint PitchOrLinearSize { get; set; }
        public uint Depth { get; set; }
        public uint MipmapCount { get; set; }
        public uint[] Reserved1 => dwReserved1; //unused
        public DdsPixelFormat PixelFormat => ddspf;
        public DdsCaps Caps { get; set; }
        public DdsCaps2 Caps2 { get; set; }
        public uint Caps3 { get; set; } //unused
        public uint Caps4 { get; set; } //unused
        public uint Reserved2 { get; set; } //unused

        public DdsHeader()
        {
            Flags = HeaderFlags.Default;
            dwReserved1 = new uint[11];
            ddspf = new DdsPixelFormat();
        }
    }

    [Flags]
    public enum HeaderFlags
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
    public enum DdsCaps
    {
        Complex = 0x8,
        Texture = 0x1000,
        Mipmap = 0x400000,

        //aliases
        DdsSurfaceFlagsTexture = Texture,
        DdsSurfaceFlagsCubemap = Complex,
        DdsSurfaceFlagsMipmap =  Complex | Mipmap
    }

    [Flags]
    public enum DdsCaps2
    {
        Cubemap = 0x200,
        CubemapPositiveX = 0x400,
        CubemapNegativeX = 0x800,
        CubemapPositiveY = 0x1000,
        CubemapNegativeY = 0x2000,
        CubemapPositiveZ = 0x4000,
        CubemapNegativeZ = 0x8000,
        Volume = 0x200000,

        //aliases
        DdsCubemapPositiveX = Cubemap | CubemapPositiveX,
        DdsCubemapNegativeX = Cubemap | CubemapNegativeX,
        DdsCubemapPositiveY = Cubemap | CubemapPositiveY,
        DdsCubemapNegativeY = Cubemap | CubemapNegativeY,
        DdsCubemapPositiveZ = Cubemap | CubemapPositiveZ,
        DdsCubemapNegativeZ = Cubemap | CubemapNegativeZ,
        DdsCubemapAllFaces = DdsCubemapPositiveX | DdsCubemapNegativeX | DdsCubemapPositiveY | DdsCubemapNegativeY | DdsCubemapPositiveZ | DdsCubemapNegativeZ,
        DdsFlagsVolume = Volume
    }
}
