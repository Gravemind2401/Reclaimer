namespace Reclaimer.Drawing
{
    public class DdsImageDescriptor
    {
        /// <summary>
        /// The <see cref="Drawing.DxgiFormat"/> used to obtain this descriptor, or the equivalent format if
        /// if this descriptor was obtained using <see cref="Drawing.XboxFormat"/>.
        /// </summary>
        public DxgiFormat DxgiFormat { get; }

        /// <summary>
        /// The <see cref="Drawing.XboxFormat"/> used to obtain this descriptor, or <see cref="XboxFormat.Unknown"/>
        /// if this descriptor was not obtained using <see cref="Drawing.XboxFormat"/>.
        /// </summary>
        public XboxFormat XboxFormat { get; }

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The number of frames in the array, the number of faces in the cubemap, or the depth of the image for 3D images.
        /// </summary>
        public int FrameCount { get; }

        /// <summary>
        /// The number of mipmaps in the image data.
        /// <br/> This does not include the initial full-size image.
        /// </summary>
        public int MipmapCount { get; }

        /// <summary>
        /// The number of bits required to store each pixel in the image.
        /// </summary>
        public int BitsPerPixel { get; }

        /// <summary>
        /// The number of bytes used to store each read/write unit.
        /// </summary>
        /// <remarks>
        /// This is used for endian swaps to convert the data between little endian and big endian.
        /// </remarks>
        public int ReadUnitSize { get; }

        /// <summary>
        /// The width and height of each compressed block in pixels.
        /// </summary>
        /// <remarks>
        /// For formats that don't use blocks, this will just be 1.
        /// </remarks>
        public int BlockWidth { get; }

        /// <summary>
        /// The number of bytes used to store each compressed block.
        /// </summary>
        /// <remarks>
        /// For formats that don't use blocks, this will just be the read unit size.
        /// </remarks>
        public int BytesPerBlock { get; }

        /// <summary>
        /// The width of the image, rounded up to the nearest valid padded dimensions, accounting for the block width and padding alignment.
        /// </summary>
        public int PaddedWidth { get; set; }

        /// <summary>
        /// The height of the image, rounded up to the nearest valid padded dimensions, accounting for the block width and padding alignment.
        /// </summary>
        public int PaddedHeight { get; set; }

        /// <summary>
        /// The number of bytes required to store the pixel data of the image, including mipmaps but excluding any padding alignment.
        /// </summary>
        public int DataLength => GetPixelDataLength(Width, Height);

        /// <summary>
        /// The number of bytes required to store the pixel data of the image, including mipmaps and including the padding alignment.
        /// </summary>
        public int PaddedDataLength => GetPixelDataLength(Math.Max(Width, PaddedWidth), Math.Max(Height, PaddedHeight));

        /// <summary>
        /// The total number of pixel rows in the image data, including all frames, mipmaps and padding.
        /// </summary>
        public int TotalArrayHeight
        {
            get
            {
                var height = Math.Max(Height, PaddedHeight);
                var result = height * Math.Max(1, FrameCount);

                if (MipmapCount > 0)
                {
                    var mipsHeight = 0d;
                    for (var i = 1; i <= MipmapCount; i++)
                        mipsHeight += height * Math.Pow(0.25, i);

                    mipsHeight += (BlockWidth - (mipsHeight % BlockWidth)) % BlockWidth;
                    result += (int)mipsHeight * Math.Max(1, FrameCount);
                }

                return result;
            }
        }

        /// <summary>
        /// The number of compressed blocks spanning the width of the image.
        /// </summary>
        public int BlockCountX => (Math.Max(Width, PaddedWidth) + BlockWidth - 1) / BlockWidth;

        /// <summary>
        /// The number of compressed blocks spanning the height of the image, including all frames and mips.
        /// </summary>
        public int BlockCountY => (TotalArrayHeight + BlockWidth - 1) / BlockWidth;

        /// <param name="format">The FourCC image format.</param>
        /// <inheritdoc cref="DdsImageDescriptor(DxgiFormat, XboxFormat, int, int, int, int)" />
        public DdsImageDescriptor(FourCC format, int width, int height, int frameCount, int mipCount)
            : this(format.ToDxgiFormat(), XboxFormat.Unknown, width, height, frameCount, mipCount)
        { }

        /// <param name="format">The DXGI image format.</param>
        /// <inheritdoc cref="DdsImageDescriptor(DxgiFormat, XboxFormat, int, int, int, int)" />
        public DdsImageDescriptor(DxgiFormat format, int width, int height, int frameCount, int mipCount)
            : this(format, XboxFormat.Unknown, width, height, frameCount, mipCount)
        { }

        /// <param name="format">The Xbox image format.</param>
        /// <inheritdoc cref="DdsImageDescriptor(DxgiFormat, XboxFormat, int, int, int, int)" />
        public DdsImageDescriptor(XboxFormat format, int width, int height, int frameCount, int mipCount)
            : this(format.ToDxgiFormat(), format, width, height, frameCount, mipCount)
        { }

        /// <summary>
        /// Creates a new instance of <see cref="DdsImageDescriptor"/> based on the specified format and dimensions.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="frameCount">The number of frames in the array, the number of faces in the cubemap, or the depth of the image for 3D images.</param>
        /// <param name="mipCount">The number of mipmaps in the image data, not including the initial full-size image.</param>
        private DdsImageDescriptor(DxgiFormat dxgiFormat, XboxFormat xboxFormat, int width, int height, int frameCount, int mipCount)
        {
            DxgiFormat = dxgiFormat;
            XboxFormat = xboxFormat;

            Width = width;
            Height = height;
            FrameCount = Math.Max(1, frameCount);
            MipmapCount = mipCount;

            int paddingAlignment;

            if (xboxFormat is XboxFormat.CTX1 or XboxFormat.DXT3a_scalar or XboxFormat.DXT3a_mono or XboxFormat.DXT3a_alpha)
            {
                //these formats have no direct equivalent in DXGI.
                //they also happen to have the same properties for this.

                BitsPerPixel = 4;
                ReadUnitSize = 2;
                BlockWidth = 4;
                BytesPerBlock = 8;
                paddingAlignment = 128;
            }
            else
            {
                var formatString = dxgiFormat.ToString();

                BitsPerPixel = dxgiFormat switch
                {
                    _ when formatString.StartsWith("R32G32B32A32") => 128,
                    _ when formatString.StartsWith("R32G32B32_") => 96,
                    _ when formatString.StartsWith("R16G16B16A16") => 64,
                    _ when formatString.StartsWith("R32G32_") => 64,
                    _ when formatString.StartsWith("R16G16_") => 32,

                    _ when formatString.StartsWith("R10G10B10A2") => 32,
                    _ when formatString.StartsWith("R8G8B8A8") => 32,
                    _ when formatString.StartsWith("B8G8R8A8") => 32,
                    _ when formatString.StartsWith("B8G8R8X8") => 32,
                    _ when formatString.StartsWith("R8_") => 8,

                    _ when formatString.StartsWith("BC1") => 4,
                    _ when formatString.StartsWith("BC2") => 8,
                    _ when formatString.StartsWith("BC3") => 8,
                    _ when formatString.StartsWith("BC4") => 4,
                    _ when formatString.StartsWith("BC5") => 8,
                    _ when formatString.StartsWith("BC6") => 8,
                    _ when formatString.StartsWith("BC7") => 8,

                    DxgiFormat.A8_UNorm or DxgiFormat.P8 => 8,

                    _ => 16
                };

                ReadUnitSize = dxgiFormat switch
                {
                    _ when formatString.StartsWith("R32") => 4,
                    _ when formatString.StartsWith("R16") => 2,
                    _ when formatString.StartsWith("R8G8B8A8") => 4,
                    _ when formatString.StartsWith("B8G8R8A8") => 4,
                    _ when formatString.StartsWith("B8G8R8X8") => 4,
                    _ when formatString.StartsWith("R8_") => 1,
                    DxgiFormat.A8_UNorm or DxgiFormat.P8 => 1,
                    _ => 2
                };

                BlockWidth = dxgiFormat switch
                {
                    DxgiFormat.BC1_Typeless or DxgiFormat.BC1_UNorm or DxgiFormat.BC1_UNorm_SRGB => 4,
                    DxgiFormat.BC2_Typeless or DxgiFormat.BC2_UNorm or DxgiFormat.BC2_UNorm_SRGB => 4,
                    DxgiFormat.BC3_Typeless or DxgiFormat.BC3_UNorm or DxgiFormat.BC3_UNorm_SRGB => 4,
                    DxgiFormat.BC4_Typeless or DxgiFormat.BC4_UNorm or DxgiFormat.BC4_SNorm => 4,
                    DxgiFormat.BC5_Typeless or DxgiFormat.BC5_UNorm or DxgiFormat.BC5_SNorm => 4,
                    DxgiFormat.BC6H_Typeless or DxgiFormat.BC6H_UF16 or DxgiFormat.BC6H_SF16 => 4,
                    DxgiFormat.BC7_Typeless or DxgiFormat.BC7_UNorm or DxgiFormat.BC7_UNorm_SRGB => 4,
                    _ => 1
                };

                BytesPerBlock = dxgiFormat switch
                {
                    _ when formatString.StartsWith("BC1") => 8,
                    _ when formatString.StartsWith("BC2") => 16,
                    _ when formatString.StartsWith("BC3") => 16,
                    _ when formatString.StartsWith("BC4") => 8,
                    _ when formatString.StartsWith("BC5") => 16,
                    _ when formatString.StartsWith("BC6") => 16,
                    _ when formatString.StartsWith("BC7") => 16,
                    _ => ReadUnitSize
                };

                //this isnt entirely accurate but works for most of the x360 bitmaps.
                //the padded dimensions are settable so they can be overridden if need be.
                paddingAlignment = dxgiFormat switch
                {
                    DxgiFormat.A8_UNorm => 32, //A8, AY8
                    DxgiFormat.B8G8R8A8_UNorm or DxgiFormat.B8G8R8X8_UNorm => 32,
                    DxgiFormat.B4G4R4A4_UNorm => 32,
                    DxgiFormat.B5G6R5_UNorm => 32,
                    DxgiFormat.R8G8_SNorm => 32, //U8V8

                    DxgiFormat.R8G8_UNorm => 128, //A8Y8
                    DxgiFormat.R8_UNorm => 128, //Y8
                    _ when formatString.StartsWith("BC") => 128,

                    _ => 1
                };
            }

            PaddedWidth = GetPaddedDimension(Width, paddingAlignment);
            PaddedHeight = GetPaddedDimension(Height, paddingAlignment);
        }

        private int GetPaddedDimension(int pixels, int alignment)
        {
            if (alignment <= 0)
                return pixels;

            var result = (int)(Math.Ceiling(pixels / (double)alignment) * alignment);
            result = (int)(Math.Ceiling(result / (double)BlockWidth) * BlockWidth);

            return result;
        }

        private int GetPixelDataLength(int width, int height)
        {
            var frameDataLength = width * height * BitsPerPixel / 8;

            if (MipmapCount > 0)
            {
                var mipsDataLength = 0;
                var minUnit = (int)Math.Pow(BlockWidth, 2) * BitsPerPixel / 8;
                for (var i = 1; i <= MipmapCount; i++)
                    mipsDataLength += Math.Max(minUnit, (int)(frameDataLength * Math.Pow(0.25, i)));
                frameDataLength += mipsDataLength;
            }

            return frameDataLength * Math.Max(1, FrameCount);
        }
    }
}
