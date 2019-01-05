using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    public static class FourCC
    {
        internal const uint DDS = 0x20534444;

        public const uint ATI1 = 0x31495441;
        public const uint ATI2 = 0x32495441;

        public const uint BC4U = 0x55344342;
        public const uint BC4S = 0x53344342;
        public const uint BC5U = 0x55354342;
        public const uint BC5S = 0x53354342;

        public const uint DXT1 = 0x31545844;
        public const uint DXT2 = 0x32545844;
        public const uint DXT3 = 0x33545844;
        public const uint DXT4 = 0x34545844;
        public const uint DXT5 = 0x35545844;
        public const uint DX10 = 0x30315844;

        public const uint RGBG = 0x47424752;
        public const uint GRGB = 0x42475247;
    }
}
