using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    public class DdsImage
    {
        private const uint DDS = 0x20534444;

        private readonly DdsHeader header;
        private readonly DdsHeaderDxt10 dx10Header;
        private readonly byte[] data;

        #region Constructors

        private DdsImage()
        {
            header = new DdsHeader();
            dx10Header = new DdsHeaderDxt10();
        }

        private DdsImage(uint height, uint width, byte[] pixelData) : this()
        {
            if (pixelData == null)
                throw new ArgumentNullException(nameof(pixelData));

            header.Flags = HeaderFlags.Default;
            header.Height = height;
            header.Width = width;
            header.Caps = DdsCaps.Texture;

            data = pixelData;
        }

        public DdsImage(uint height, uint width, uint fourCC, byte[] pixelData)
            : this(height, width, pixelData)
        {
            header.PixelFormat.Flags |= FormatFlags.FourCC;
            header.PixelFormat.FourCC = fourCC;
        }

        public DdsImage(uint height, uint width, FourCC fourCC, byte[] pixelData)
            : this(height, width, (uint)fourCC, pixelData)
        {

        }

        public DdsImage(uint height, uint width, uint bpp, uint RMask, uint GMask, uint BMask, uint AMask, byte[] pixelData)
            : this(height, width, pixelData)
        {
            header.PixelFormat.Flags |= FormatFlags.Rgb;
            header.PixelFormat.RgbBitCount = bpp;
            header.PixelFormat.RBitmask = RMask;
            header.PixelFormat.GBitmask = GMask;
            header.PixelFormat.BBitmask = BMask;
            header.PixelFormat.ABitmask = AMask;

            if (AMask > 0)
            {
                header.PixelFormat.Flags |= FormatFlags.AlphaPixels;
                if (RMask == 0 && GMask == 0 && BMask == 0)
                    header.PixelFormat.Flags |= FormatFlags.Alpha;
            }
        }

        public DdsImage(uint height, uint width, DxgiFormat dxgiFormat, DxgiTextureType textureType, byte[] pixelData)
            : this(height, width, FourCC.DX10, pixelData)
        {
            dx10Header.DxgiFormat = dxgiFormat;
            dx10Header.ResourceDimension = (D3D10ResourceDimension)textureType;
            dx10Header.MiscFlags = D3D10ResourceMiscFlags.None;
            dx10Header.ArraySize = 1;
            dx10Header.MiscFlags2 = D3D10ResourceMiscFlag2.DdsAlphaModeStraight;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the height of the image, in pixels.
        /// </summary>
        public uint Height
        {
            get { return header.Height; }
            set
            {
                header.Height = value;
                header.Flags |= HeaderFlags.Height;
            }
        }

        /// <summary>
        /// Gets or sets the width of the image, in pixels.
        /// </summary>
        public uint Width
        {
            get { return header.Width; }
            set
            {
                header.Width = value;
                header.Flags |= HeaderFlags.Width;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the pitch of the image. This property is mutually exclusive with <see cref="LinearSize"/>.
        /// </summary>
        public uint? Pitch
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.Pitch))
                    return null;

                return header.PitchOrLinearSize;
            }
            set
            {
                if (value.HasValue)
                {
                    header.Flags |= HeaderFlags.Pitch;
                    LinearSize = null;
                }
                else
                    header.Flags &= ~HeaderFlags.Pitch;

                header.PitchOrLinearSize = value ?? 0;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the linear size of the image. This property is mutually exclusive with <see cref="Pitch"/>.
        /// </summary>
        public uint? LinearSize
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.LinearSize))
                    return null;

                return header.PitchOrLinearSize;
            }
            set
            {
                if (value.HasValue)
                {
                    header.Flags |= HeaderFlags.LinearSize;
                    Pitch = null;
                }
                else
                    header.Flags &= ~HeaderFlags.LinearSize;

                header.PitchOrLinearSize = value ?? 0;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the depth of the image.
        /// </summary>
        public uint? Depth
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.Depth))
                    return null;

                return header.Depth;
            }
            set
            {
                var flag = HeaderFlags.Depth;
                header.Flags = value.HasValue ? header.Flags | flag : header.Flags & ~flag;
                header.Depth = value ?? 0;
            }
        }

        /// <summary>
        /// Optional. Gets or sets the number of mipmaps contained within the image.
        /// </summary>
        public uint? MipmapCount
        {
            get
            {
                if (!header.Flags.HasFlag(HeaderFlags.MipmapCount))
                    return null;

                return header.MipmapCount;
            }
            set
            {
                var flag = HeaderFlags.MipmapCount;
                header.Flags = value.HasValue ? header.Flags | flag : header.Flags & ~flag;
                header.Depth = value ?? 0;
            }
        }

        public TextureFlags TextureFlags
        {
            get { return (TextureFlags)header.Caps; }
            set { header.Caps = (DdsCaps)value; }
        }

        /// <summary>
        /// Gets or sets flags indicating which faces of a cubemap are contained in the DDS image.
        /// </summary>
        public CubemapFlags CubemapFlags
        {
            get { return (CubemapFlags)header.Caps2; }
            set { header.Caps2 = (DdsCaps2)value; }
        }

        public D3D10ResourceMiscFlags DX10ResourceFlags
        {
            get { return dx10Header.MiscFlags; }
            set { dx10Header.MiscFlags = value; }
        }

        public D3D10ResourceMiscFlag2 DX10AlphaFlags
        {
            get { return dx10Header.MiscFlags2; }
            set { dx10Header.MiscFlags2 = value; }
        }

        #endregion
    }

    /// <summary>
    /// Indicates the type of texture contained in the DDS image.
    /// </summary>
    [Flags]
    public enum TextureFlags
    {
        DdsSurfaceFlagsTexture = DdsCaps.Texture,
        DdsSurfaceFlagsCubemap = DdsCaps.Texture | DdsCaps.Complex,
        DdsSurfaceFlagsMipmap = DdsCaps.Texture | DdsCaps.Complex | DdsCaps.Mipmap
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
        ATI1 = 0x31495441,
        ATI2 = 0x32495441,

        BC4U = 0x55344342,
        BC4S = 0x53344342,
        BC5U = 0x55354342,
        BC5S = 0x53354342,

        DXT1 = 0x31545844,
        DXT2 = 0x32545844,
        DXT3 = 0x33545844,
        DXT4 = 0x34545844,
        DXT5 = 0x35545844,
        DX10 = 0x30315844,

        RGBG = 0x47424752,
        GRGB = 0x42475247,
    }

    public enum DxgiTextureType
    {
        Buffer = D3D10ResourceDimension.Buffer,
        Texture1D = D3D10ResourceDimension.Texture1D,
        Texture2D = D3D10ResourceDimension.Texture2D,
        Texture3D = D3D10ResourceDimension.Texture3D
    }
}
