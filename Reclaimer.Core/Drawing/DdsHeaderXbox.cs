namespace Reclaimer.Drawing
{
    internal class DdsHeaderXbox
    {
        public XboxFormat XboxFormat { get; set; }
        public D3D10ResourceDimension ResourceDimension { get; set; }
        public D3D10ResourceMiscFlags MiscFlags { get; set; }
        public int ArraySize { get; set; }
        public D3D10ResourceMiscFlag2 MiscFlags2 { get; set; }
        public XgTileMode TileMode { get; set; }
        public int BaseAlignment { get; set; }
        public int DataSize { get; set; }
        public int XdkVersion { get; set; }
    }

    public enum XgTileMode
    {
        Invalid = 0,
        Linear = 1
    }

    // This enum does not represent actual Xbox DDS format values.
    // It just serves as a way to specify alternate
    // texture formats used by Xbox that are not part of the Dxgi spec.
    public enum XboxFormat
    {
        Unknown,
        AY8,
        G8B8,
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
        L16,
        V8U8,
        Y8,
        Y8A8
    }
}
