using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    public partial class DdsImage
    {
        private const uint DDS = 0x20534444;

        private readonly DdsHeader header;
        private readonly DdsHeaderDxt10 dx10Header;
        private readonly DdsHeaderXbox xboxHeader;
        private readonly byte[] data;

        private static ArgumentOutOfRangeException ParamMustBeGreaterThanZero(string paramName, object value)
        {
            return new ArgumentOutOfRangeException(paramName, value, string.Format(CultureInfo.CurrentCulture, "{0} must be greater than zero.", paramName));
        }

        #region Constructors

        private DdsImage(DdsHeader header, DdsHeaderDxt10 dx10Header, DdsHeaderXbox xboxHeader, byte[] data)
        {
            this.header = header;
            this.dx10Header = dx10Header;
            this.xboxHeader = xboxHeader;
            this.data = data;
        }

        private DdsImage(int height, int width, byte[] pixelData)
            : this(new DdsHeader(), new DdsHeaderDxt10(), new DdsHeaderXbox(), pixelData)
        {
            if (pixelData == null)
                throw new ArgumentNullException(nameof(pixelData));

            if (height <= 0)
                throw ParamMustBeGreaterThanZero(nameof(height), height);

            if (width <= 0)
                throw ParamMustBeGreaterThanZero(nameof(width), width);

            header.Flags = HeaderFlags.Default;
            header.Height = height;
            header.Width = width;
            header.Caps = DdsCaps.Texture;

            data = pixelData;
        }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImage"/> with the specified dimensions, pixel format and pixel data.
        /// </summary>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="fourCC">The FourCC code representing the format of the pixel data.</param>
        /// <param name="pixelData">The binary data containing the pixels of the image.</param>
        public DdsImage(int height, int width, int fourCC, byte[] pixelData)
            : this(height, width, pixelData)
        {
            header.PixelFormat.Flags |= FormatFlags.FourCC;
            header.PixelFormat.FourCC = fourCC;
        }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImage"/> with the specified dimensions, pixel format and pixel data.
        /// </summary>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="fourCC">The FourCC code representing the format of the pixel data.</param>
        /// <param name="pixelData">The binary data containing the pixels of the image.</param>
        public DdsImage(int height, int width, FourCC fourCC, byte[] pixelData)
            : this(height, width, (int)fourCC, pixelData)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImage"/> with the specified dimensions and pixel data, where the pixel data is uncompressed.
        /// </summary>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="bpp">The number of bits used to represent each pixel.</param>
        /// <param name="redMask">The mask used to isolate data for the red channel.</param>
        /// <param name="greenMask">The mask used to isolate data for the green channel.</param>
        /// <param name="blueMask">The mask used to isolate data for the blue channel.</param>
        /// <param name="alphaMask">The mask used to isolate data for the alpha channel.</param>
        /// <param name="pixelData">The binary data containing the pixels of the image.</param>
        public DdsImage(int height, int width, int bpp, int redMask, int greenMask, int blueMask, int alphaMask, byte[] pixelData)
            : this(height, width, pixelData)
        {
            if (bpp <= 0)
                throw ParamMustBeGreaterThanZero(nameof(bpp), bpp);

            header.PixelFormat.Flags |= FormatFlags.Rgb;
            header.PixelFormat.RgbBitCount = bpp;
            header.PixelFormat.RBitmask = redMask;
            header.PixelFormat.GBitmask = greenMask;
            header.PixelFormat.BBitmask = blueMask;
            header.PixelFormat.ABitmask = alphaMask;

            if (alphaMask > 0)
            {
                header.PixelFormat.Flags |= FormatFlags.AlphaPixels;
                if (redMask == 0 && greenMask == 0 && blueMask == 0)
                    header.PixelFormat.Flags |= FormatFlags.Alpha;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImage"/> with the specified dimensions and pixel data, using D3D10 header and format information.
        /// </summary>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="dxgiFormat">The DxgiFormat value that identifies the format of the pixel data.</param>
        /// <param name="textureType">The type of texture represented by the image.</param>
        /// <param name="pixelData">The binary data containing the pixels of the image.</param>
        public DdsImage(int height, int width, DxgiFormat dxgiFormat, byte[] pixelData)
            : this(height, width, FourCC.DX10, pixelData)
        {
            dx10Header.DxgiFormat = dxgiFormat;
            dx10Header.ResourceDimension = D3D10ResourceDimension.Texture2D;
            dx10Header.MiscFlags = D3D10ResourceMiscFlags.None;
            dx10Header.ArraySize = 1;
            dx10Header.MiscFlags2 = D3D10ResourceMiscFlag2.DdsAlphaModeStraight;
        }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImage"/> with the specified dimensions and pixel data, using D3D10 header and format information.
        /// </summary>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="xboxFormat">The XboxFormat value that identifies the format of the pixel data.</param>
        /// <param name="textureType">The type of texture represented by the image.</param>
        /// <param name="pixelData">The binary data containing the pixels of the image.</param>
        public DdsImage(int height, int width, XboxFormat xboxFormat, byte[] pixelData)
            : this(height, width, FourCC.XBOX, pixelData)
        {
            xboxHeader.XboxFormat = xboxFormat;
            xboxHeader.ResourceDimension = D3D10ResourceDimension.Texture2D;
            xboxHeader.MiscFlags = D3D10ResourceMiscFlags.None;
            xboxHeader.ArraySize = 1;
            xboxHeader.MiscFlags2 = D3D10ResourceMiscFlag2.DdsAlphaModeStraight;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the FourCC code in the DDS header.
        /// </summary>
        public int FormatCode => header.PixelFormat.FourCC;

        /// <summary>
        /// Gets or sets the height of the image, in pixels.
        /// </summary>
        public int Height
        {
            get { return header.Height; }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(Height), value);

                header.Height = value;
                header.Flags |= HeaderFlags.Height;
            }
        }

        /// <summary>
        /// Gets or sets the width of the image, in pixels.
        /// </summary>
        public int Width
        {
            get { return header.Width; }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(Width), value);

                header.Width = value;
                header.Flags |= HeaderFlags.Width;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the pitch of the image. This property is mutually exclusive with <see cref="LinearSize"/>.
        /// </summary>
        public int? Pitch
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.Pitch))
                    return null;

                return header.PitchOrLinearSize;
            }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(Pitch), value);

                header.SetFlag(HeaderFlags.Pitch, value.HasValue);
                if (value.HasValue)
                    LinearSize = null;

                header.PitchOrLinearSize = value ?? 0;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the linear size of the image. This property is mutually exclusive with <see cref="Pitch"/>.
        /// </summary>
        public int? LinearSize
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.LinearSize))
                    return null;

                return header.PitchOrLinearSize;
            }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(LinearSize), value);

                header.SetFlag(HeaderFlags.LinearSize, value.HasValue);
                if (value.HasValue)
                    Pitch = null;

                header.PitchOrLinearSize = value ?? 0;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the depth of the image.
        /// </summary>
        public int? Depth
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.Depth))
                    return null;

                return header.Depth;
            }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(Depth), value);

                header.SetFlag(HeaderFlags.Depth, value.HasValue);
                header.Depth = value ?? 0;

                dx10Header.ResourceDimension = xboxHeader.ResourceDimension = value > 1
                    ? D3D10ResourceDimension.Texture3D
                    : D3D10ResourceDimension.Texture2D;

                UpdateTextureFlags();
            }
        }

        /// <summary>
        /// Optional. Gets or sets the number of mipmaps contained within the image.
        /// </summary>
        public int? MipmapCount
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.MipmapCount))
                    return null;

                return header.MipmapCount;
            }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(MipmapCount), value);

                header.SetFlag(HeaderFlags.MipmapCount, value.HasValue);
                header.MipmapCount = value ?? 0;

                UpdateTextureFlags();
            }
        }

        /// <summary>
        /// Gets or sets the number of slices in an array texture.
        /// </summary>
        public int ArraySize
        {
            get { return dx10Header.ArraySize; }
            set
            {
                if (value <= 0)
                    throw ParamMustBeGreaterThanZero(nameof(ArraySize), value);

                dx10Header.ArraySize = xboxHeader.ArraySize = value;
            }
        }

        private TextureFlags TextureFlags => (TextureFlags)header.Caps;

        private void UpdateTextureFlags()
        {
            if (MipmapCount > 1)
                header.Caps = (DdsCaps)TextureFlags.DdsSurfaceFlagsMipmap;
            else if (CubemapFlags > 0)
                header.Caps = (DdsCaps)TextureFlags.DdsSurfaceFlagsCubemap;
            else
                header.Caps = (DdsCaps)TextureFlags.DdsSurfaceFlagsTexture;
        }

        /// <summary>
        /// Gets or sets flags indicating which faces of a cubemap are contained in the DDS image.
        /// </summary>
        public CubemapFlags CubemapFlags
        {
            get { return (CubemapFlags)header.Caps2; }
            set
            {
                header.Caps2 = (DdsCaps2)value;

                if (value > 0)
                    DX10ResourceFlags |= D3D10ResourceMiscFlags.TextureCube;
                else
                    DX10ResourceFlags &= ~D3D10ResourceMiscFlags.TextureCube;

                UpdateTextureFlags();
            }
        }

        /// <summary>
        /// Gets or sets miscellaneous flags for the image.
        /// <para>These flags are only used if the FourCC code is set to <see cref="FourCC.DX10"/></para>
        /// </summary>
        public D3D10ResourceMiscFlags DX10ResourceFlags
        {
            get { return dx10Header.MiscFlags; }
            set { dx10Header.MiscFlags = value; }
        }

        /// <summary>
        /// Gets or sets flags indicating the type of alpha used in the image.
        /// <para>These flags are only used if the FourCC code is set to <see cref="FourCC.DX10"/></para>
        /// </summary>
        public D3D10ResourceMiscFlag2 DX10AlphaFlags
        {
            get { return dx10Header.MiscFlags2; }
            set { dx10Header.MiscFlags2 = value; }
        }

        /// <summary>
        /// Gets or sets miscellaneous flags for the image.
        /// <para>These flags are only used if the FourCC code is set to <see cref="FourCC.XBOX"/></para>
        /// </summary>
        public D3D10ResourceMiscFlags XboxResourceFlags
        {
            get { return xboxHeader.MiscFlags; }
            set { xboxHeader.MiscFlags = value; }
        }

        /// <summary>
        /// Gets or sets flags indicating the type of alpha used in the image.
        /// <para>These flags are only used if the FourCC code is set to <see cref="FourCC.XBOX"/></para>
        /// </summary>
        public D3D10ResourceMiscFlag2 XboxAlphaFlags
        {
            get { return xboxHeader.MiscFlags2; }
            set { xboxHeader.MiscFlags2 = value; }
        }

        #endregion

        /// <summary>
        /// Returns a copy of the underlying raw pixel data for this image without perfoming any conversions.
        /// </summary>
        public byte[] GetPixelData()
        {
            var result = new byte[data.Length];
            Array.Copy(data, result, data.Length);
            return result;
        }

        /// <summary>
        /// Writes the DDS header and pixel data to a file on disk.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <exception cref="ArgumentNullException" />
        public void WriteToDisk(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var dir = Directory.GetParent(fileName).FullName;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                WriteToStream(fs);
        }

        /// <summary>
        /// Writes the DDS header and pixel data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <exception cref="ArgumentNullException" />
        public void WriteToStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(DDS);

                writer.Write(DdsHeader.Size);
                writer.Write((uint)header.Flags);
                writer.Write(header.Height);
                writer.Write(header.Width);
                writer.Write(header.PitchOrLinearSize);
                writer.Write(header.Depth);
                writer.Write(header.MipmapCount);
                foreach (var i in header.Reserved1)
                    writer.Write(i);

                writer.Write(DdsPixelFormat.Size);
                writer.Write((uint)header.PixelFormat.Flags);
                writer.Write(header.PixelFormat.FourCC);
                writer.Write(header.PixelFormat.RgbBitCount);
                writer.Write(header.PixelFormat.RBitmask);
                writer.Write(header.PixelFormat.GBitmask);
                writer.Write(header.PixelFormat.BBitmask);
                writer.Write(header.PixelFormat.ABitmask);

                writer.Write((uint)header.Caps);
                writer.Write((uint)header.Caps2);
                writer.Write(header.Caps3);
                writer.Write(header.Caps4);
                writer.Write(header.Reserved2);

                if (header.PixelFormat.FourCC == (uint)FourCC.DX10)
                {
                    writer.Write((uint)dx10Header.DxgiFormat);
                    writer.Write((uint)dx10Header.ResourceDimension);
                    writer.Write((uint)dx10Header.MiscFlags);
                    writer.Write(dx10Header.ArraySize);
                    writer.Write((uint)dx10Header.MiscFlags2);
                }
                else if (header.PixelFormat.FourCC == (uint)FourCC.XBOX)
                {
                    writer.Write((uint)xboxHeader.XboxFormat);
                    writer.Write((uint)xboxHeader.ResourceDimension);
                    writer.Write((uint)xboxHeader.MiscFlags);
                    writer.Write(xboxHeader.ArraySize);
                    writer.Write((uint)xboxHeader.MiscFlags2);
                }

                writer.Write(data);
            }
        }

        /// <summary>
        /// Reads a DDS header and pixel data from a file on disk.
        /// </summary>
        /// <param name="fileName">The full path of the file to read.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="InvalidDataException" />
        public static DdsImage ReadFromDisk(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("The specified file does not exist.", fileName);

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                return ReadFromStream(fs);
        }

        /// <summary>
        /// Reads a DDS header and pixel data from a stream object.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="InvalidDataException" />
        public static DdsImage ReadFromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (reader.ReadUInt32() != DDS)
                    throw new InvalidOperationException("Data is not a valid DDS file");

                if (reader.ReadUInt32() != DdsHeader.Size)
                    throw new InvalidDataException("Invalid DDS data");

                var header = new DdsHeader();
                header.Flags = (HeaderFlags)reader.ReadInt32();
                header.Height = reader.ReadInt32();
                header.Width = reader.ReadInt32();
                header.PitchOrLinearSize = reader.ReadInt32();
                header.Depth = reader.ReadInt32();
                header.MipmapCount = reader.ReadInt32();
                for (int i = 0; i < header.Reserved1.Length; i++)
                    header.Reserved1[i] = reader.ReadInt32();

                if (reader.ReadInt32() != DdsPixelFormat.Size)
                    throw new InvalidDataException("Invalid DDS data");

                header.PixelFormat.Flags = (FormatFlags)reader.ReadInt32();
                header.PixelFormat.FourCC = reader.ReadInt32();
                header.PixelFormat.RgbBitCount = reader.ReadInt32();
                header.PixelFormat.RBitmask = reader.ReadInt32();
                header.PixelFormat.GBitmask = reader.ReadInt32();
                header.PixelFormat.BBitmask = reader.ReadInt32();
                header.PixelFormat.ABitmask = reader.ReadInt32();

                var dx10Header = new DdsHeaderDxt10();
                if (header.PixelFormat.FourCC == (uint)FourCC.DX10)
                {
                    dx10Header.DxgiFormat = (DxgiFormat)reader.ReadInt32();
                    dx10Header.ResourceDimension = (D3D10ResourceDimension)reader.ReadInt32();
                    dx10Header.MiscFlags = (D3D10ResourceMiscFlags)reader.ReadInt32();
                    dx10Header.ArraySize = reader.ReadInt32();
                    dx10Header.MiscFlags2 = (D3D10ResourceMiscFlag2)reader.ReadInt32();
                }

                var xboxHeader = new DdsHeaderXbox();
                if (header.PixelFormat.FourCC == (uint)FourCC.XBOX)
                {
                    xboxHeader.XboxFormat = (XboxFormat)reader.ReadInt32();
                    xboxHeader.ResourceDimension = (D3D10ResourceDimension)reader.ReadInt32();
                    xboxHeader.MiscFlags = (D3D10ResourceMiscFlags)reader.ReadInt32();
                    xboxHeader.ArraySize = reader.ReadInt32();
                    xboxHeader.MiscFlags2 = (D3D10ResourceMiscFlag2)reader.ReadInt32();
                }

                var data = reader.ReadBytes((int)(stream.Length - stream.Position));
                return new DdsImage(header, dx10Header, xboxHeader, data);
            }
        }
    }

    /// <summary>
    /// Indicates which faces of a cubemap are contained in a DDS image.
    /// </summary>
    [Flags]
    public enum CubemapFlags
    {
        None = 0,

        DdsCubemapPositiveX = DdsCaps2.Cubemap | DdsCaps2.CubemapPositiveX,
        DdsCubemapNegativeX = DdsCaps2.Cubemap | DdsCaps2.CubemapNegativeX,
        DdsCubemapPositiveY = DdsCaps2.Cubemap | DdsCaps2.CubemapPositiveY,
        DdsCubemapNegativeY = DdsCaps2.Cubemap | DdsCaps2.CubemapNegativeY,
        DdsCubemapPositiveZ = DdsCaps2.Cubemap | DdsCaps2.CubemapPositiveZ,
        DdsCubemapNegativeZ = DdsCaps2.Cubemap | DdsCaps2.CubemapNegativeZ,

        DdsCubemapAllFaces = DdsCubemapPositiveX | DdsCubemapNegativeX | DdsCubemapPositiveY | DdsCubemapNegativeY | DdsCubemapPositiveZ | DdsCubemapNegativeZ,

        DdsFlagsVolume = DdsCaps2.Volume
    }

    /// <summary>
    /// A collection of known Four Character Codes (FourCC) that identify the format of the DDS image's pixel data.
    /// </summary>
    public enum FourCC
    {
        ATI1 = 0x31495441, //Also BC4, DXT5A
        ATI2 = 0x32495441, //Also BC5, DXN

        BC4U = 0x55344342, //DXGI_FORMAT_BC4_UNORM
        BC4S = 0x53344342, //DXGI_FORMAT_BC4_SNORM

        BC5U = 0x55354342, //DXGI_FORMAT_BC5_UNORM
        BC5S = 0x53354342, //DXGI_FORMAT_BC5_SNORM

        DXT1 = 0x31545844, //DXGI_FORMAT_BC1_UNORM
        DXT2 = 0x32545844, //D3DFMT_DXT2 (also BC2)
        DXT3 = 0x33545844, //DXGI_FORMAT_BC2_UNORM
        DXT4 = 0x34545844, //D3DFMT_DXT4 (also BC3)
        DXT5 = 0x35545844, //DXGI_FORMAT_BC3_UNORM
        DX10 = 0x30315844,
        XBOX = 0x584F4258,

        RGBG = 0x47424752, //DXGI_FORMAT_R8G8_B8G8_UNORM
        GRGB = 0x42475247, //DXGI_FORMAT_G8R8_G8B8_UNORM

        UYVY = 0x59565955, //D3DFMT_UYVY
        YUY2 = 0x32595559, //D3DFMT_YUY2
        V8U8 = 117, //D3DFMT_CxV8U8
    }
}
