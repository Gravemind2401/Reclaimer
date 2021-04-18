using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
     // This class does not represent an actual Xbox DDS header.
     // It is based on DdsHeaderDxt10 and serves as a way to specify alternate
     // texture formats used by Xbox that are not part of the Dxgi spec.

    internal class DdsHeaderXbox
    {
        public XboxFormat XboxFormat { get; set; }
        public D3D10ResourceDimension ResourceDimension { get; set; }
        public D3D10ResourceMiscFlags MiscFlags { get; set; }
        public int ArraySize { get; set; }
        public D3D10ResourceMiscFlag2 MiscFlags2 { get; set; }
    }

    public enum XboxFormat
    {
        Unknown,
        A8,
        AY8,
        CTX1,
        DXN,
        DXN_mono_alpha,
        DXN_SNorm,
        DXT3a_scalar,
        DXT3a_mono,
        DXT3a_alpha,
        DXT5a_scalar,
        DXT5a_mono,
        DXT5a_alpha,
        V8U8,
        Y8,
        Y8A8,
    }
}
