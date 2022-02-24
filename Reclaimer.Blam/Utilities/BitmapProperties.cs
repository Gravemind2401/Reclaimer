using Reclaimer.Blam.Common;
using System;
using System.Collections.Generic;
using Reclaimer.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public class BitmapProperties
    {
        public int Width { get; }
        public int Height { get; }
        public object BitmapFormat { get; }
        public object BitmapType { get; }

        public int Depth { get; set; }
        public int FrameCount { get; set; }
        public int MipmapCount { get; set; }

        public ByteOrder ByteOrder { get; set; }
        public MipmapLayout CubeMipLayout { get; set; }
        public MipmapLayout ArrayMipLayout { get; set; }

        public bool UsesPadding { get; set; }
        public bool Swizzled { get; set; }

        //non-zero to override
        public int VirtualWidth { get; set; }
        public int VirtualHeight { get; set; }

        public BitmapProperties(int width, int height, object format, object type)
        {
            Width = width;
            Height = height;
            BitmapFormat = format;
            BitmapType = type;
            Depth = FrameCount = 1;
            MipmapCount = 0;
            ByteOrder = ByteOrder.LittleEndian;
            CubeMipLayout = ArrayMipLayout = MipmapLayout.None;
            UsesPadding = false;
            Swizzled = false;
            VirtualWidth = 0;
            VirtualHeight = 0;
        }
    }
}
