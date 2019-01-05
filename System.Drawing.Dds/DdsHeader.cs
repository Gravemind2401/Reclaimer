using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    /* https://docs.microsoft.com/en-us/windows/desktop/direct3ddds/dds-header */
    [StructLayout(LayoutKind.Sequential)]
    public struct DdsHeader
    {
        public uint dwSize { get; set; } //must be 124
        public HeaderFlags dwFlags { get; set; }
        public uint dwHeight { get; set; } //required
        public uint dwWidth { get; set; } //required
        public uint dwPitchOrLinearSize { get; set; }
        public uint dwDepth { get; set; }
        public uint dwMipMapCount { get; set; }
        public uint[] dwReserved1 { get; set; } //length 11, unused
        public DdsPixelFormat ddspf { get; set; }
        public DdsCaps dwCaps { get; set; }
        public DdsCaps2 dwCaps2 { get; set; }
        public uint dwCaps3 { get; set; } //unused
        public uint dwCaps4 { get; set; } //unused
        public uint dwReserved2 { get; set; } //unused
    }

    [Flags]
    public enum HeaderFlags
    {
        DDSD_CAPS = 0x1, //required
        DDSD_HEIGHT = 0x2, //required
        DDSD_WIDTH = 0x4, //required
        DDSD_PITCH = 0x8,
        DDSD_PIXELFORMAT = 0x1000, //required
        DDSD_MIPMAPCOUNT = 0x20000,
        DDSD_LINEARSIZE = 0x80000,
        DDSD_DEPTH = 0x800000,
    }

    [Flags]
    public enum DdsCaps
    {
        DDSCAPS_COMPLEX = 0x8,
        DDSCAPS_TEXTURE = 0x1000,
        DDSCAPS_MIPMAP = 0x400000,

        DDS_SURFACE_FLAGS_TEXTURE = DDSCAPS_TEXTURE,
        DDS_SURFACE_FLAGS_CUBEMAP = DDSCAPS_COMPLEX,
        DDS_SURFACE_FLAGS_MIPMAP = DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
    }

    [Flags]
    public enum DdsCaps2
    {
        DDSCAPS2_CUBEMAP = 0x200,
        DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
        DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
        DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
        DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
        DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
        DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
        DDSCAPS2_VOLUME = 0x200000,

        DDS_CUBEMAP_POSITIVEX = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX,
        DDS_CUBEMAP_NEGATIVEX = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX,
        DDS_CUBEMAP_POSITIVEY = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY,
        DDS_CUBEMAP_NEGATIVEY = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY,
        DDS_CUBEMAP_POSITIVEZ = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ,
        DDS_CUBEMAP_NEGATIVEZ = DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ,
        DDS_CUBEMAP_ALLFACES = DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX | DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY | DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ,
        DDS_FLAGS_VOLUME = DDSCAPS2_VOLUME
    }
}
