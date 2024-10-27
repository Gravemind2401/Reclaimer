using Reclaimer.Drawing.Bc7;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

/* https://docs.microsoft.com/en-us/windows/desktop/direct3d10/d3d10-graphics-programming-guide-resources-block-compression */
namespace Reclaimer.Drawing
{
    using static DxgiFormat;
    using static XboxFormat;

    public partial class DdsImage
    {
        private const int bcBlockWidth = 4;
        private const int bcBlockHeight = 4;

        private delegate byte[] Decompress(byte[] data, int height, int width, bool bgr24);

        private static readonly Dictionary<DxgiFormat, Decompress> decompressMethodsDxgi = CreateLookup<DxgiDecompressorAttribute, DxgiFormat>();
        private static readonly Dictionary<XboxFormat, Decompress> decompressMethodsXbox = CreateLookup<XboxDecompressorAttribute, XboxFormat>();
        private static readonly Dictionary<FourCC, Decompress> decompressMethodsFourCC = CreateLookup<FourCCDecompressorAttribute, FourCC>();

        private static Dictionary<TFormat, Decompress> CreateLookup<TAttribute, TFormat>() where TAttribute : Attribute, IFormatAttribute<TFormat>
        {
            return typeof(DdsImage).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .SelectMany(m => m.GetCustomAttributes<TAttribute>().Select(a => new { a.Format, Delegate = (Decompress)m.CreateDelegate(typeof(Decompress)) }))
                .ToDictionary(m => m.Format, m => m.Delegate);
        }

        #region WriteToDisk
        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <param name="format">The image format to write with.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToDisk(string fileName, ImageFormat format) => WriteToDisk(fileName, format, new DdsOutputArgs());

        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <param name="format">The image format to write with.</param>
        /// <param name="args">Parameters to use when writing the image.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToDisk(string fileName, ImageFormat format, DdsOutputArgs args)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(format);
            ArgumentNullException.ThrowIfNull(args);

            var dir = Directory.GetParent(fileName).FullName;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                WriteToStream(fs, format, args);
        }
        #endregion

        #region WriteToStream
        /// <summary>
        /// Decompresses any compressed pixel data and writes the image to a stream using a standard image format
        /// using the default decompression options and a non-cubemap layout.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="format">The image format to write with.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToStream(Stream stream, ImageFormat format) => WriteToStream(stream, format, new DdsOutputArgs());

        /// <summary>
        /// Decompresses any compressed pixel data and writes the image to a stream using a standard image format.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="format">The image format to write with.</param>
        /// <param name="args">Parameters to use when writing the image.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToStream(Stream stream, ImageFormat format, DdsOutputArgs args)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(format);
            ArgumentNullException.ThrowIfNull(args);

            BitmapEncoder encoder;
            if (format.Equals(ImageFormat.Bmp))
                encoder = new BmpBitmapEncoder();
            else if (format.Equals(ImageFormat.Gif))
                encoder = new GifBitmapEncoder();
            else if (format.Equals(ImageFormat.Jpeg))
                encoder = new JpegBitmapEncoder();
            else if (format.Equals(ImageFormat.Png))
                encoder = new PngBitmapEncoder();
            else if (format.Equals(ImageFormat.Tiff))
                encoder = new TiffBitmapEncoder();
            else
                throw new NotSupportedException("The ImageFormat is not supported.");

            WriteToStream(stream, encoder, args);
        }

        /// <summary>
        /// Decompresses any compressed pixel data and writes the image to a stream using a standard image format.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoder">The bitmap encoder to write with.</param>
        /// <param name="args">Parameters to use when writing the image.</param>
        /// <exception cref="ArgumentNullException" />
        public void WriteToStream(Stream stream, BitmapEncoder encoder, DdsOutputArgs args)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(encoder);
            ArgumentNullException.ThrowIfNull(args);

            var source = ToBitmapSource(args);
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
        }
        #endregion

        #region ToBitmapSource
        /// <summary>
        /// Decompresses any compressed pixel data and returns the image data as a <see cref="BitmapSource"/>.
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public BitmapSource ToBitmapSource() => ToBitmapSource(new DdsOutputArgs());

        /// <summary>
        /// Decompresses any compressed pixel data and returns the image data as a <see cref="BitmapSource"/>
        /// </summary>
        /// <param name="args">Parameters to use when writing the image.</param>
        /// <exception cref="NotSupportedException" />
        public BitmapSource ToBitmapSource(DdsOutputArgs args)
        {
            ArgumentNullException.ThrowIfNull(args);

            const double dpi = 96;
            var virtualHeight = Height;

            var isCubeMap = TextureFlags.HasFlag(TextureFlags.DdsSurfaceFlagsCubemap) && CubemapFlags.HasFlag(CubemapFlags.DdsCubemapAllFaces);
            if (isCubeMap)
                virtualHeight *= 6;

            var pixels = DecompressPixelData(args.Bgr24);

            if (args.UseChannelMask)
            {
                //MaskChannels() will alter the array that gets passed in, so make sure it doesnt modify the original dds pixels array
                if (ReferenceEquals(pixels, data))
                    pixels = (byte[])data.Clone();

                MaskChannels(pixels, args.Options);
            }

            var source = BitmapSource.Create(Width, virtualHeight, dpi, dpi, args.Format, null, pixels, Width * args.Bpp);

            if (isCubeMap && args.ValidCubeLayout)
                source = UnwrapCubemapSource(source, dpi, args.Format, args.Layout);

            return source;
        }
        #endregion

        /// <summary>
        /// Returns a copy of the image in 32bit BGRA format.
        /// </summary>
        public DdsImage AsUncompressed()
        {
            if (header.PixelFormat.FourCC == (int)FourCC.DX10)
            {
                switch (dx10Header.DxgiFormat)
                {
                    case DxgiFormat.B8G8R8X8_UNorm:
                    case DxgiFormat.B8G8R8A8_UNorm:
                        return this;
                }
            }

            var pixels = DecompressPixelData(false);
            var result = new DdsImage(Height, Width, pixels);

            result.header.PixelFormat.FourCC = (int)FourCC.DX10;
            result.header.PixelFormat.Flags = header.PixelFormat.Flags;
            result.header.PixelFormat.BBitmask = header.PixelFormat.BBitmask;
            result.header.PixelFormat.GBitmask = header.PixelFormat.GBitmask;
            result.header.PixelFormat.RBitmask = header.PixelFormat.RBitmask;
            result.header.PixelFormat.ABitmask = header.PixelFormat.ABitmask;
            result.header.PixelFormat.RgbBitCount = header.PixelFormat.RgbBitCount;

            result.header.Caps = header.Caps;
            result.header.Caps2 = header.Caps2;
            result.header.Caps3 = header.Caps3;
            result.header.Caps4 = header.Caps4;
            result.header.Depth = header.Depth;
            result.header.Flags = header.Flags;
            result.header.MipmapCount = header.MipmapCount;
            result.header.PitchOrLinearSize = header.PitchOrLinearSize;

            result.dx10Header.DxgiFormat = DxgiFormat.B8G8R8A8_UNorm;

            if (header.PixelFormat.FourCC == (int)FourCC.DX10)
            {
                result.dx10Header.ResourceDimension = dx10Header.ResourceDimension;
                result.dx10Header.MiscFlags = dx10Header.MiscFlags;
                result.dx10Header.ArraySize = dx10Header.ArraySize;
                result.dx10Header.MiscFlags2 = dx10Header.MiscFlags2;
            }
            else if (header.PixelFormat.FourCC == (int)FourCC.XBOX)
            {
                result.dx10Header.ResourceDimension = xboxHeader.ResourceDimension;
                result.dx10Header.MiscFlags = xboxHeader.MiscFlags;
                result.dx10Header.ArraySize = xboxHeader.ArraySize;
                result.dx10Header.MiscFlags2 = xboxHeader.MiscFlags2;
            }
            else
            {
                result.dx10Header.ResourceDimension = D3D10ResourceDimension.Texture2D;
                result.dx10Header.MiscFlags = D3D10ResourceMiscFlags.None;
                result.dx10Header.ArraySize = 1;
                result.dx10Header.MiscFlags2 = D3D10ResourceMiscFlag2.DdsAlphaModeStraight;
            }

            return result;
        }

        private byte[] DecompressPixelData(bool bgr24)
        {
            var virtualHeight = Height;
            var isCubeMap = TextureFlags.HasFlag(TextureFlags.DdsSurfaceFlagsCubemap) && CubemapFlags.HasFlag(CubemapFlags.DdsCubemapAllFaces);
            if (isCubeMap)
                virtualHeight *= 6;

            if (header.PixelFormat.FourCC == (uint)FourCC.DX10)
            {
                if (decompressMethodsDxgi.TryGetValue(dx10Header.DxgiFormat, out var dxgiDecode))
                    return dxgiDecode(data, virtualHeight, Width, bgr24);
                else
                {
                    switch (dx10Header.DxgiFormat)
                    {
                        case DxgiFormat.B8G8R8X8_UNorm:
                        case DxgiFormat.B8G8R8A8_UNorm:
                            return bgr24 ? ToArray(SkipNth(data, 4), true, virtualHeight, Width) : data;

                        default:
                            throw new NotSupportedException($"The {nameof(DxgiFormat)} '{dx10Header.DxgiFormat}' is not supported.");
                    }
                }
            }
            else if (header.PixelFormat.FourCC == (uint)FourCC.XBOX)
            {
                return decompressMethodsXbox.TryGetValue(xboxHeader.XboxFormat, out var xboxDecode)
                    ? xboxDecode(data, virtualHeight, Width, bgr24)
                    : throw new NotSupportedException($"The {nameof(XboxFormat)} '{xboxHeader.XboxFormat}' is not supported.");
            }
            else
            {
                var fourcc = (FourCC)header.PixelFormat.FourCC;
                return decompressMethodsFourCC.TryGetValue(fourcc, out var fourccDecode)
                    ? fourccDecode(data, virtualHeight, Width, bgr24)
                    : throw new NotSupportedException($"The {nameof(FourCC)} '{fourcc}' is not supported.");
            }
        }

        private static void MaskChannels(byte[] source, DecompressOptions channels)
        {
            var bpp = channels.HasFlag(DecompressOptions.Bgr24) ? 3 : 4;
            var mask = 0;

            if (!channels.HasFlag(DecompressOptions.RemoveBlueChannel))
                mask |= 1;
            if (!channels.HasFlag(DecompressOptions.RemoveGreenChannel))
                mask |= 2;
            if (!channels.HasFlag(DecompressOptions.RemoveRedChannel))
                mask |= 4;
            if (!channels.HasFlag(DecompressOptions.RemoveAlphaChannel))
                mask |= 8;

            var channelIndex = mask switch
            {
                1 => 0,
                2 => 1,
                4 => 2,
                8 => 3,
                _ => -1
            };

            for (var i = 0; i < source.Length; i += bpp)
            {
                for (var j = 0; j < bpp; j++)
                {
                    if (channelIndex >= 0)
                    {
                        if (j == 3)
                            source[i + j] = byte.MaxValue; //full opacity
                        else
                            source[i + j] = channelIndex < bpp ? source[i + channelIndex] : byte.MinValue;
                    }
                    else
                    {
                        var bit = (int)Math.Pow(2, j);
                        if ((mask & bit) == 0)
                            source[i + j] = j < 3 ? byte.MinValue : byte.MaxValue;
                    }
                }
            }
        }

        private BitmapSource UnwrapCubemapSource(BitmapSource source, double dpi, System.Windows.Media.PixelFormat format, CubemapLayout layout)
        {
            var bpp = format.BitsPerPixel / 8;
            var stride = bpp * Width;
            var dest = new WriteableBitmap(Width * 4, Height * 3, dpi, dpi, format, null);

            var faceArray = new[] { layout.Face1, layout.Face2, layout.Face3, layout.Face4, layout.Face5, layout.Face6 };
            var rotateArray = new[] { layout.Orientation1, layout.Orientation2, layout.Orientation3, layout.Orientation4, layout.Orientation5, layout.Orientation6 };

            var xTiles = new[] { 1, 0, 1, 2, 3, 1 };
            var yTiles = new[] { 0, 1, 1, 1, 1, 2 };

            for (var i = 0; i < 6; i++)
            {
                var tileIndex = (int)faceArray[i] - 1;

                var sourceRect = new Int32Rect(0, Height * i, Width, Height);
                var destRect = new Int32Rect(xTiles[tileIndex] * Width, yTiles[tileIndex] * Height, Width, Height);

                var buffer = new byte[Width * Height * bpp];
                source.CopyPixels(sourceRect, buffer, stride, 0);
                Rotate(buffer, Width, Height, bpp, rotateArray[i]);
                dest.WritePixels(destRect, buffer, stride, 0);
            }

            return dest;
        }

        #region Standard Decompression Methods

        #region Packed Formats

        [DxgiDecompressor(B5G6R5_UNorm)]
        internal static byte[] DecompressB5G6R5(byte[] data, int height, int width, bool bgr24) => DecompressPacked(data, height, width, bgr24, BgraColour.From565);

        [DxgiDecompressor(B5G5R5A1_UNorm)]
        internal static byte[] DecompressB5G5R5A1(byte[] data, int height, int width, bool bgr24) => DecompressPacked(data, height, width, bgr24, BgraColour.From5551);

        [DxgiDecompressor(B4G4R4A4_UNorm)]
        internal static byte[] DecompressB4G4R4A4(byte[] data, int height, int width, bool bgr24) => DecompressPacked(data, height, width, bgr24, BgraColour.From4444);

        private static byte[] DecompressPacked(byte[] data, int height, int width, bool bgr24, BgraColour.ColorUnpackMethod colorFunc)
        {
            var bpp = bgr24 ? 3 : 4;
            var srcPixelCount = Math.Min(width * height, data.Length / 2);
            var srcPixelData = MemoryMarshal.Cast<byte, ushort>(data);

            var output = new byte[width * height * bpp];
            for (int srcPixelIndex = 0, outputIndex = 0; srcPixelIndex < srcPixelCount; srcPixelIndex++, outputIndex += bpp)
            {
                var color = colorFunc(srcPixelData[srcPixelIndex]);
                color.CopyTo(output, outputIndex, bgr24);
            }

            return output;
        }

        #endregion

        #region Integer Formats

        [DxgiDecompressor(A8_UNorm)]
        internal static byte[] DecompressA8(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];

            if (!bgr24)
            {
                for (int inputIndex = 0, outputIndex = 0; inputIndex < data.Length && outputIndex < output.Length; inputIndex++, outputIndex += bpp)
                    output[outputIndex + 3] = data[inputIndex];
            }

            return output;
        }

        [DxgiDecompressor(R8G8_UNorm), DxgiDecompressor(R8G8_UInt)]
        internal static byte[] DecompressR8G8(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];

            for (int inputIndex = 0, outputIndex = 0; inputIndex < data.Length && outputIndex < output.Length; inputIndex += 2, outputIndex += bpp)
            {
                output[outputIndex + 1] = data[inputIndex + 1];
                output[outputIndex + 2] = data[inputIndex + 0];
                if (!bgr24)
                    output[outputIndex + 3] = byte.MaxValue;
            }

            return output;
        }

        [DxgiDecompressor(R8G8_SNorm), DxgiDecompressor(R8G8_SInt)]
        internal static byte[] DecompressR8G8Signed(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, sbyte>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex += 2, outputIndex += bpp)
            {
                output[outputIndex + 1] = (byte)(unchecked(input[inputIndex + 1]) - sbyte.MinValue);
                output[outputIndex + 2] = (byte)(unchecked(input[inputIndex + 0]) - sbyte.MinValue);
                if (!bgr24)
                    output[outputIndex + 3] = byte.MaxValue;
            }

            return output;
        }

        [DxgiDecompressor(R8G8B8A8_SNorm), DxgiDecompressor(R8G8B8A8_SInt)]
        internal static byte[] DecompressR8G8B8A8Signed(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, sbyte>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex += 4, outputIndex += bpp)
            {
                //note: input is rgba, output is bgra
                output[outputIndex + 0] = (byte)(unchecked(input[inputIndex + 2]) - sbyte.MinValue);
                output[outputIndex + 1] = (byte)(unchecked(input[inputIndex + 1]) - sbyte.MinValue);
                output[outputIndex + 2] = (byte)(unchecked(input[inputIndex + 0]) - sbyte.MinValue);
                if (!bgr24)
                    output[outputIndex + 3] = (byte)(unchecked(input[inputIndex + 3]) - sbyte.MinValue);
            }

            return output;
        }

        [DxgiDecompressor(R16_UNorm), DxgiDecompressor(R16_UInt)]
        internal static byte[] DecompressR16(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, ushort>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex++, outputIndex += bpp)
            {
                output[outputIndex + 2] = (byte)MathF.Round(input[inputIndex] / (float)ushort.MaxValue * byte.MaxValue);
                if (!bgr24)
                    output[outputIndex + 3] = byte.MaxValue;
            }

            return output;
        }

        #endregion

        #region BC Formats

        [FourCCDecompressor(FourCC.DXT1)]
        [DxgiDecompressor(BC1_Typeless), DxgiDecompressor(BC1_UNorm)]
        internal static byte[] DecompressBC1(byte[] data, int height, int width, bool bgr24)
        {
            const float oneHalf = 1 / 2f;
            const float oneThird = 1 / 3f;
            const float twoThirds = 2 / 3f;

            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 8;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            var paletteData = MemoryMarshal.Cast<byte, ushort>(data);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    var paletteIndex = srcIndex / 2;

                    if (paletteIndex >= paletteData.Length)
                        return output;

                    var c0 = paletteData[paletteIndex];
                    var c1 = paletteData[paletteIndex + 1];

                    palette[0] = BgraColour.From565(c0);
                    palette[1] = BgraColour.From565(c1);

                    if (c0 <= c1)
                    {
                        palette[2] = Lerp(palette[0], palette[1], oneHalf);
                        palette[3] = new BgraColour(); //zero on all channels
                    }
                    else
                    {
                        palette[2] = Lerp(palette[0], palette[1], oneThird);
                        palette[3] = Lerp(palette[0], palette[1], twoThirds);
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        var indexBits = data[srcIndex + 4 + i];
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var destIndex = (destY * width + destX) * bpp;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);
                            palette[pIndex].CopyTo(output, destIndex, bgr24);
                        }
                    }
                }
            }

            return output;
        }

        [FourCCDecompressor(FourCC.DXT2), FourCCDecompressor(FourCC.DXT3)]
        [DxgiDecompressor(BC2_Typeless), DxgiDecompressor(BC2_UNorm)]
        internal static byte[] DecompressBC2(byte[] data, int height, int width, bool bgr24)
        {
            const float oneThird = 1 / 3f;
            const float twoThirds = 2 / 3f;

            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 16;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            var paletteData = MemoryMarshal.Cast<byte, ushort>(data);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    var paletteIndex = srcIndex / 2;

                    if (paletteIndex >= paletteData.Length)
                        return output;

                    palette[0] = BgraColour.From565(paletteData[paletteIndex + 4]);
                    palette[1] = BgraColour.From565(paletteData[paletteIndex + 5]);

                    palette[2] = Lerp(palette[0], palette[1], oneThird);
                    palette[3] = Lerp(palette[0], palette[1], twoThirds);

                    for (var i = 0; i < 4; i++)
                    {
                        var alphaBits = paletteData[paletteIndex + i];
                        var indexBits = data[srcIndex + 12 + i];
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var destIndex = (destY * width + destX) * bpp;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);

                            var result = palette[pIndex];
                            result.A = (byte)(((alphaBits >> j * 4) & 0xF) * (0xFF / 0xF));
                            result.CopyTo(output, destIndex, bgr24);
                        }
                    }
                }
            }

            return output;
        }

        [FourCCDecompressor(FourCC.DXT4), FourCCDecompressor(FourCC.DXT5)]
        [DxgiDecompressor(BC3_Typeless), DxgiDecompressor(BC3_UNorm)]
        internal static byte[] DecompressBC3(byte[] data, int height, int width, bool bgr24)
        {
            const float oneThird = 1 / 3f;
            const float twoThirds = 2 / 3f;

            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var rgbPalette = new BgraColour[4];
            var alphaPalette = new byte[8];

            const int bytesPerBlock = 16;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            var paletteData = MemoryMarshal.Cast<byte, ushort>(data);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    var paletteIndex = srcIndex / 2;

                    if (paletteIndex >= paletteData.Length)
                        return output;

                    rgbPalette[0] = BgraColour.From565(paletteData[paletteIndex + 4]);
                    rgbPalette[1] = BgraColour.From565(paletteData[paletteIndex + 5]);

                    rgbPalette[2] = Lerp(rgbPalette[0], rgbPalette[1], oneThird);
                    rgbPalette[3] = Lerp(rgbPalette[0], rgbPalette[1], twoThirds);

                    alphaPalette[0] = data[srcIndex];
                    alphaPalette[1] = data[srcIndex + 1];

                    var gradients = alphaPalette[0] > alphaPalette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        alphaPalette[i + 1] = Lerp(alphaPalette[0], alphaPalette[1], i / gradients);

                    if (alphaPalette[0] <= alphaPalette[1])
                    {
                        alphaPalette[6] = byte.MinValue;
                        alphaPalette[7] = byte.MaxValue;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        var rgbIndexBits = data[srcIndex + 12 + i];
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var pixelIndex = i * 4 + j;
                            var alphaStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var alphaIndexBits = (data[alphaStart + 2] << 16) | (data[alphaStart + 1] << 8) | data[alphaStart];

                            var destIndex = (destY * width + destX) * bpp;
                            var pIndex = (byte)((rgbIndexBits >> j * 2) & 0x3);

                            var result = rgbPalette[pIndex];
                            result.A = alphaPalette[(alphaIndexBits >> (pixelIndex % 8) * 3) & 0x7];
                            result.CopyTo(output, destIndex, bgr24);
                        }
                    }
                }
            }

            return output;
        }

        [FourCCDecompressor(FourCC.ATI1), FourCCDecompressor(FourCC.BC4U), FourCCDecompressor(FourCC.BC4S)]
        [DxgiDecompressor(BC4_Typeless), DxgiDecompressor(BC4_UNorm), DxgiDecompressor(BC4_SNorm)]
        internal static byte[] DecompressBC4(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var palette = new byte[8];

            const int bytesPerBlock = 8;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    palette[0] = data[srcIndex];
                    palette[1] = data[srcIndex + 1];

                    var gradients = palette[0] > palette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        palette[i + 1] = Lerp(palette[0], palette[1], i / gradients);

                    if (palette[0] <= palette[1])
                    {
                        palette[6] = byte.MinValue;
                        palette[7] = byte.MaxValue;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var pixelIndex = i * 4 + j;
                            var pStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var pIndexBits = (data[pStart + 2] << 16) | (data[pStart + 1] << 8) | data[pStart];

                            var destIndex = (destY * width + destX) * bpp;
                            var pIndex = (byte)((pIndexBits >> (pixelIndex % 8) * 3) & 0x7);

                            output[destIndex] = output[destIndex + 1] = output[destIndex + 2] = palette[pIndex];
                            if (!bgr24)
                                output[destIndex + 3] = byte.MaxValue;
                        }
                    }
                }
            }

            return output;
        }

        [FourCCDecompressor(FourCC.ATI2), FourCCDecompressor(FourCC.BC5U)]
        [DxgiDecompressor(BC5_Typeless), DxgiDecompressor(BC5_UNorm)]
        internal static byte[] DecompressBC5(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var rPalette = new byte[8];
            var gPalette = new byte[8];

            const int bytesPerBlock = 16;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;

                    rPalette[0] = data[srcIndex];
                    rPalette[1] = data[srcIndex + 1];

                    var gradients = rPalette[0] > rPalette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        rPalette[i + 1] = Lerp(rPalette[0], rPalette[1], i / gradients);

                    if (rPalette[0] <= rPalette[1])
                    {
                        rPalette[6] = byte.MinValue;
                        rPalette[7] = byte.MaxValue;
                    }

                    gPalette[0] = data[srcIndex + 8];
                    gPalette[1] = data[srcIndex + 9];

                    gradients = gPalette[0] > gPalette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        gPalette[i + 1] = Lerp(gPalette[0], gPalette[1], i / gradients);

                    if (gPalette[0] <= gPalette[1])
                    {
                        gPalette[6] = byte.MinValue;
                        gPalette[7] = byte.MaxValue;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var pixelIndex = i * 4 + j;

                            var rStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var rIndexBits = (data[rStart + 2] << 16) | (data[rStart + 1] << 8) | data[rStart];

                            var gStart = srcIndex + (pixelIndex < 8 ? 10 : 13);
                            var gIndexBits = (data[gStart + 2] << 16) | (data[gStart + 1] << 8) | data[gStart];

                            var destIndex = (destY * width + destX) * bpp;
                            var shift = (pixelIndex % 8) * 3;

                            var rIndex = (byte)((rIndexBits >> shift) & 0x7);
                            var gIndex = (byte)((gIndexBits >> shift) & 0x7);

                            //output[destIndex] = byte.MinValue;
                            output[destIndex + 1] = gPalette[gIndex];
                            output[destIndex + 2] = rPalette[rIndex];
                            if (!bgr24)
                                output[destIndex + 3] = byte.MaxValue;
                        }
                    }
                }
            }

            return output;
        }

        [FourCCDecompressor(FourCC.BC5S)]
        [DxgiDecompressor(BC5_SNorm)]
        internal static byte[] DecompressBC5Signed(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var rPalette = new sbyte[8];
            var gPalette = new sbyte[8];

            const int bytesPerBlock = 16;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;

                    rPalette[0] = unchecked((sbyte)data[srcIndex]);
                    rPalette[1] = unchecked((sbyte)data[srcIndex + 1]);

                    var gradients = rPalette[0] > rPalette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        rPalette[i + 1] = Lerp(rPalette[0], rPalette[1], i / gradients);

                    if (rPalette[0] <= rPalette[1])
                    {
                        rPalette[6] = sbyte.MinValue;
                        rPalette[7] = sbyte.MaxValue;
                    }

                    gPalette[0] = unchecked((sbyte)data[srcIndex + 8]);
                    gPalette[1] = unchecked((sbyte)data[srcIndex + 9]);

                    gradients = gPalette[0] > gPalette[1] ? 7f : 5f;
                    for (var i = 1; i < gradients; i++)
                        gPalette[i + 1] = Lerp(gPalette[0], gPalette[1], i / gradients);

                    if (gPalette[0] <= gPalette[1])
                    {
                        gPalette[6] = sbyte.MinValue;
                        gPalette[7] = sbyte.MaxValue;
                    }

                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var pixelIndex = i * 4 + j;

                            var rStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var rIndexBits = (data[rStart + 2] << 16) | (data[rStart + 1] << 8) | data[rStart];

                            var gStart = srcIndex + (pixelIndex < 8 ? 10 : 13);
                            var gIndexBits = (data[gStart + 2] << 16) | (data[gStart + 1] << 8) | data[gStart];

                            var destIndex = (destY * width + destX) * bpp;
                            var shift = (pixelIndex % 8) * 3;

                            var rIndex = (byte)((rIndexBits >> shift) & 0x7);
                            var gIndex = (byte)((gIndexBits >> shift) & 0x7);

                            output[destIndex] = unchecked((byte)sbyte.MinValue); //opening a BC5_SNorm dds in visual studio treats the blue channel as -1f, so we may as well too
                            output[destIndex + 1] = Lerp(byte.MinValue, byte.MaxValue, (gPalette[gIndex] - sbyte.MinValue) / 255f);
                            output[destIndex + 2] = Lerp(byte.MinValue, byte.MaxValue, (rPalette[rIndex] - sbyte.MinValue) / 255f);
                            if (!bgr24)
                                output[destIndex + 3] = byte.MaxValue;
                        }
                    }
                }
            }

            return output;
        }

        [DxgiDecompressor(BC7_Typeless), DxgiDecompressor(BC7_UNorm)]
        internal static byte[] DecompressBC7(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];

            const int bytesPerBlock = 16;
            var xBlocks = (int)Math.Ceiling(width / (float)bcBlockWidth);
            var yBlocks = (int)Math.Ceiling(height / (float)bcBlockHeight);

            var reader = new BitReader(data);

            //these get reused for every compressed block
            var indices0 = new byte[16];
            var indices1 = new byte[16];

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    reader.Position = srcIndex * 8;

                    int mode;
                    for (mode = 0; mode < 8; mode++)
                    {
                        if (reader.ReadBit() == 1)
                            break;
                    }

                    if (mode > 7)
                        continue;

                    var info = Bc7Helper.BlocksTypes[mode];

                    var partitionIndex = reader.ReadBits(info.PartitionBits);
                    var rotation = reader.ReadBits(info.RotationBits);
                    var indexMode = reader.ReadBits(info.IndexModeBits);

                    var endpoints = new BgraColour[info.SubsetCount * 2];
                    var endpointBytes = MemoryMarshal.Cast<BgraColour, byte>(endpoints);
                    var channels = info.AlphaBits > 0 ? 4 : 3;

                    for (var i = 2; i >= 0; i--) // R, G, B
                    {
                        for (var j = 0; j < endpoints.Length; j++)
                            endpointBytes[j * 4 + i] = reader.ReadBits(info.ColourBits);
                    }

                    if (info.AlphaBits > 0)
                    {
                        for (var i = 0; i < endpoints.Length; i++)
                            endpoints[i].A = reader.ReadBits(info.AlphaBits);
                    }

                    //P-bit is a shared LSB across each channel. each endpoint may have its
                    //own P-bit or a P-bit may be shared between both endpoints of a subset
                    if (info.PBitMode != PBitMode.None)
                    {
                        for (var i = 0; i < info.SubsetCount; i++)
                        {
                            var p1 = reader.ReadBit();
                            var p2 = info.PBitMode == PBitMode.Shared ? p1 : reader.ReadBit();
                            var endpointBytesOffset = i * 8;

                            for (var c = 0; c < channels; c++)
                            {
                                endpointBytes[endpointBytesOffset + c] = (byte)((endpointBytes[endpointBytesOffset + c] << 1) | p1);
                                endpointBytes[endpointBytesOffset + 4 + c] = (byte)((endpointBytes[endpointBytesOffset + 4 + c] << 1) | p2);
                            }
                        }
                    }

                    var palette0Size = 1 << info.Index0Bits;
                    var palette1Size = 1 << info.Index1Bits;

                    var palette0 = new BgraColour[info.SubsetCount * palette0Size];
                    var palette1 = new BgraColour[info.SubsetCount * palette1Size];

                    var palette0Bytes = MemoryMarshal.Cast<BgraColour, byte>(palette0);
                    var palette1Bytes = MemoryMarshal.Cast<BgraColour, byte>(palette1);

                    for (var i = 0; i < info.SubsetCount; i++)
                    {
                        var endpointBytesOffset = i * 8;

                        for (var c = 0; c < channels; c++)
                        {
                            var channelBits = c < 3 ? info.ColourBits : info.AlphaBits;
                            if (info.PBitMode != PBitMode.None)
                                channelBits++;

                            int e0 = endpointBytes[endpointBytesOffset + c];
                            int e1 = endpointBytes[endpointBytesOffset + 4 + c];

                            //shift left until the MSB of the endpoint value is in the leftmost bit of the byte
                            //then copy the X highest bits into the X lowest bits where X is (8 - # of colour bits)
                            e0 = (e0 << (8 - channelBits)) | (e0 >> (2 * channelBits - 8));
                            e1 = (e1 << (8 - channelBits)) | (e1 >> (2 * channelBits - 8));

                            Bc7Helper.InterpolatePaletteValues(e0, e1, i, c, ref info, palette0Bytes, palette1Bytes);
                        }
                    }

                    var fixups = new byte[] { 0, Bc7Helper.FixUpTable[info.SubsetCount - 1, partitionIndex, 1], Bc7Helper.FixUpTable[info.SubsetCount - 1, partitionIndex, 2] };
                    var subsets = Bc7Helper.PartitionTable[info.SubsetCount - 1, partitionIndex];

                    //get the index bits
                    //since every index of indices0 gets assigned, there is no need to clear it first
                    for (var i = 0; i < 16; i++)
                    {
                        var subset = subsets[i];
                        indices0[i] = reader.ReadBits((byte)(i == fixups[subset] ? info.Index0Bits - 1 : info.Index0Bits));
                    }

                    //there is only a second set of indices in modes 4 and 5.
                    //in both of these cases there is only one subset, which 
                    //means the fixup table and partition table will be all zeroes.
                    //this means only index 0 will be detected as a fixup index
                    //which is what we want for these modes, including with indices0.
                    if (info.Index1Bits > 0)
                    {
                        for (var i = 0; i < 16; i++)
                        {
                            var subset = subsets[i];
                            indices1[i] = reader.ReadBits((byte)(i == fixups[subset] ? info.Index1Bits - 1 : info.Index1Bits));
                        }
                    }
                    else
                        Array.Fill(indices1, (byte)0); //reset to zeros

                    for (var i = 0; i < 4; i++)
                    {
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            if (destX >= width || destY >= height)
                                continue;

                            var pixelIndex = i * 4 + j;
                            var destIndex = (destY * width + destX) * bpp;

                            var subsetIndex = subsets[pixelIndex];

                            var (colourPalette, paletteDepth, colourIndex) = indexMode == 0
                                ? (palette0, palette0Size, indices0[pixelIndex])
                                : (palette1, palette1Size, indices1[pixelIndex]);

                            var result = colourPalette[subsetIndex * paletteDepth + colourIndex];

                            if (info.Index1Bits > 0)
                            {
                                var (alphaPalette, alphaIndex) = indexMode == 0
                                    ? (palette1, indices1[pixelIndex])
                                    : (palette0, indices0[pixelIndex]);

                                result.A = alphaPalette[subsetIndex * paletteDepth + alphaIndex].A;
                            }
                            else if (info.AlphaBits == 0)
                                result.A = byte.MaxValue;

                            switch (rotation)
                            {
                                case 0: // no rotation
                                    break;
                                case 1: // swap A+R
                                    (result.A, result.R) = (result.R, result.A);
                                    break;
                                case 2: //swap A+G
                                    (result.A, result.G) = (result.G, result.A);
                                    break;
                                case 3: //swap A+B
                                    (result.A, result.B) = (result.B, result.A);
                                    break;
                            }

                            result.CopyTo(output, destIndex, bgr24);
                        }
                    }

                    if (reader.Position != (srcIndex + 16) * 8)
                        System.Diagnostics.Debugger.Break();
                }
            }

            return output;
        }

        #endregion

        #region FP Formats

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte RealToByte(float value) => (byte)MathF.Round(Math.Clamp(value, 0f, 1f) * byte.MaxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte RealToByte(Half value) => (byte)MathF.Round(Math.Clamp((float)value, 0f, 1f) * byte.MaxValue);

        [DxgiDecompressor(R32G32B32_Float)]
        internal static byte[] DecompressRGBFP32(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, float>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex += 3, outputIndex += bpp)
            {
                //note: input is rgb, output is bgr
                output[outputIndex + 0] = RealToByte(input[inputIndex + 2]);
                output[outputIndex + 1] = RealToByte(input[inputIndex + 1]);
                output[outputIndex + 2] = RealToByte(input[inputIndex + 0]);
                if (!bgr24)
                    output[outputIndex + 3] = byte.MaxValue;
            }

            return output;
        }

        [DxgiDecompressor(R32G32B32A32_Float)]
        internal static byte[] DecompressRGBAFP32(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, float>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex += 4, outputIndex += bpp)
            {
                //note: input is rgba, output is bgra
                output[outputIndex + 0] = RealToByte(input[inputIndex + 2]);
                output[outputIndex + 1] = RealToByte(input[inputIndex + 1]);
                output[outputIndex + 2] = RealToByte(input[inputIndex + 0]);
                if (!bgr24)
                    output[outputIndex + 3] = RealToByte(input[inputIndex + 3]);
            }

            return output;
        }

        [DxgiDecompressor(R16G16B16A16_Float)]
        internal static byte[] DecompressRGBAFP16(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var input = MemoryMarshal.Cast<byte, Half>(data);

            for (int inputIndex = 0, outputIndex = 0; inputIndex < input.Length && outputIndex < output.Length; inputIndex += 4, outputIndex += bpp)
            {
                //note: input is rgba, output is bgra
                output[outputIndex + 0] = RealToByte(input[inputIndex + 2]);
                output[outputIndex + 1] = RealToByte(input[inputIndex + 1]);
                output[outputIndex + 2] = RealToByte(input[inputIndex + 0]);
                if (!bgr24)
                    output[outputIndex + 3] = RealToByte(input[inputIndex + 3]);
            }

            return output;
        }

        #endregion

        #endregion

        #region Xbox Decompression Methods

        [XboxDecompressor(AY8)]
        internal static byte[] DecompressAY8(byte[] data, int height, int width, bool bgr24)
        {
            return ToArray(data.SelectMany(b => Enumerable.Range(0, bgr24 ? 3 : 4).Select(i => b)), bgr24, height, width);
        }

        [XboxDecompressor(L16)]
        internal static byte[] DecompressL16(byte[] data, int height, int width, bool bgr24)
        {
            data = DecompressR16(data, height, width, bgr24);

            //copy R across G and B
            var bpp = bgr24 ? 3 : 4;
            for (var i = 0; i < data.Length; i += bpp)
                data[i + 0] = data[i + 1] = data[i + 2];

            return data;
        }

        [XboxDecompressor(V8U8)]
        internal static byte[] DecompressV8U8(byte[] data, int height, int width, bool bgr24)
        {
            data = DecompressR8G8Signed(data, height, width, bgr24);
            CalculateZVector(data, bgr24);
            return data;
        }

        [XboxDecompressor(Y8)]
        internal static byte[] DecompressY8(byte[] data, int height, int width, bool bgr24)
        {
            return ToArray(data.SelectMany(b => Enumerable.Range(0, bgr24 ? 3 : 4).Select(i => i < 3 ? b : byte.MaxValue)), bgr24, height, width);
        }

        [XboxDecompressor(Y8A8)]
        internal static byte[] DecompressY8A8(byte[] data, int height, int width, bool bgr24)
        {
            return ToArray(Enumerable.Range(0, height * width).SelectMany(i => Enumerable.Range(0, bgr24 ? 3 : 4).Select(j => j < 3 ? data[i * 2 + 1] : data[i * 2])), bgr24, height, width);
        }

        internal static byte[] DecompressBC1DualChannel(byte[] data, int height, int width, bool bgr24)
        {
            const float oneThird = 1 / 3f;
            const float twoThirds = 2 / 3f;

            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    palette[0] = new BgraColour { G = data[srcIndex + 0], R = data[srcIndex + 1], A = byte.MaxValue };
                    palette[1] = new BgraColour { G = data[srcIndex + 2], R = data[srcIndex + 3], A = byte.MaxValue };

                    palette[2] = Lerp(palette[0], palette[1], oneThird);
                    palette[3] = Lerp(palette[0], palette[1], twoThirds);

                    for (var i = 0; i < 4; i++)
                    {
                        var indexBits = data[srcIndex + 4 + i];
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = (destY * width + destX) * bpp;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);
                            var colour = palette[pIndex];
                            colour.B = CalculateZVector(colour.R, colour.G);
                            colour.CopyTo(output, destIndex, bgr24);
                        }
                    }
                }
            }

            return output;
        }

        internal static byte[] DecompressBC2AlphaOnly(byte[] data, int height, int width, bool bgr, bool a, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            var output = new byte[width * height * bpp];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            var paletteData = MemoryMarshal.Cast<byte, ushort>(data);

            for (var yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (var xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    var paletteIndex = srcIndex / 2;

                    if (paletteIndex >= paletteData.Length)
                        return output;

                    for (var i = 0; i < 4; i++)
                    {
                        var alphaBits = paletteData[paletteIndex + i];
                        for (var j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;
                            var destIndex = (destY * width + destX) * bpp;

                            var value = (byte)(((alphaBits >> j * 4) & 0xF) * (0xFF / 0xF));
                            if (bgr)
                                output[destIndex] = output[destIndex + 1] = output[destIndex + 2] = value;
                            if (!bgr24)
                                output[destIndex + 3] = a ? value : byte.MaxValue;
                        }
                    }
                }
            }

            return output;
        }

        internal static byte[] DecompressBC3AlphaOnly(byte[] data, int height, int width, bool bgr, bool a, bool bgr24)
        {
            //same bit layout as BC4
            data = DecompressBC4(data, height, width, bgr24);

            for (var i = 0; i < data.Length; i += 4)
            {
                data[i + 1] = data[i + 2] = bgr ? data[i] : byte.MinValue; //gr = b
                if (!bgr24)
                    data[i + 3] = a ? data[i] : byte.MaxValue; //a = b
            }

            return data;
        }

        [XboxDecompressor(CTX1)]
        internal static byte[] DecompressCTX1(byte[] data, int height, int width, bool bgr24) => DecompressBC1DualChannel(data, height, width, bgr24);

        [XboxDecompressor(DXN)]
        internal static byte[] DecompressDXN(byte[] data, int height, int width, bool bgr24)
        {
            data = DecompressBC5(data, height, width, bgr24);
            CalculateZVector(data, bgr24);
            return data;
        }

        [XboxDecompressor(DXN_SNorm)]
        internal static byte[] DecompressDXNSigned(byte[] data, int height, int width, bool bgr24)
        {
            data = DecompressBC5Signed(data, height, width, bgr24);
            CalculateZVector(data, bgr24);
            return data;
        }

        [XboxDecompressor(DXN_mono_alpha)]
        internal static byte[] DecompressDXN_mono_alpha(byte[] data, int height, int width, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            data = DecompressBC5(data, height, width, bgr24);
            for (var i = 0; i < data.Length; i += bpp)
            {
                if (!bgr24)
                    data[i + 3] = data[i + 1]; //a = g
                data[i] = data[i + 1] = data[i + 2]; //bg = r
            }

            return data;
        }

        [XboxDecompressor(DXT3a_scalar)]
        internal static byte[] DecompressDXT3a_scalar(byte[] data, int height, int width, bool bgr24) => DecompressBC2AlphaOnly(data, height, width, true, true, bgr24);

        [XboxDecompressor(DXT3a_mono)]
        internal static byte[] DecompressDXT3a_mono(byte[] data, int height, int width, bool bgr24) => DecompressBC2AlphaOnly(data, height, width, true, false, bgr24);

        [XboxDecompressor(DXT3a_alpha)]
        internal static byte[] DecompressDXT3a_alpha(byte[] data, int height, int width, bool bgr24) => DecompressBC2AlphaOnly(data, height, width, false, true, bgr24);

        [XboxDecompressor(DXT5a_scalar)]
        internal static byte[] DecompressDXT5a_scalar(byte[] data, int height, int width, bool bgr24) => DecompressBC3AlphaOnly(data, height, width, true, true, bgr24);

        [XboxDecompressor(DXT5a_mono)]
        internal static byte[] DecompressDXT5a_mono(byte[] data, int height, int width, bool bgr24) => DecompressBC3AlphaOnly(data, height, width, true, false, bgr24);

        [XboxDecompressor(DXT5a_alpha)]
        internal static byte[] DecompressDXT5a_alpha(byte[] data, int height, int width, bool bgr24) => DecompressBC3AlphaOnly(data, height, width, false, true, bgr24);

        #endregion

        private static sbyte Lerp(sbyte p1, sbyte p2, float fraction) => (sbyte)MathF.Round((p1 * (1 - fraction)) + (p2 * fraction));
        private static byte Lerp(byte p1, byte p2, float fraction) => (byte)MathF.Round((p1 * (1 - fraction)) + (p2 * fraction));
        private static float Lerp(float p1, float p2, float fraction) => (p1 * (1 - fraction)) + (p2 * fraction);

        private static void CalculateZVector(byte[] data, bool bgr24)
        {
            var bpp = bgr24 ? 3 : 4;
            for (var i = 0; i < data.Length; i += bpp)
                data[i] = CalculateZVector(data[i + 2], data[i + 1]);
        }

        private static byte CalculateZVector(byte r, byte g)
        {
            var x = Lerp(-1f, 1f, r / 255f);
            var y = Lerp(-1f, 1f, g / 255f);
            var z = (float)Math.Sqrt(1 - x * x - y * y);

            return (byte)MathF.Round((z + 1) / 2 * 255f);
        }

        private static void Rotate(byte[] buffer, int width, int height, int bpp, RotateFlipType rotation)
        {
            var rot = (int)rotation;

            if (rot == 0)
                return;

            var turns = 4 - rot % 4; //starting at 4 because we need to undo the rotations, not apply them
            var output = ArrayPool<byte>.Shared.Rent(buffer.Length);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var sourceIndex = y * width * bpp + x * bpp;

                    var destW = rot % 2 == 0 ? width : height;
                    //int destH = rot % 2 == 0 ? height : width;

                    int destX, destY;
                    if (turns == 0)
                    {
                        destX = x;
                        destY = y;
                    }
                    else if (turns == 1)
                    {
                        destY = x;
                        destX = (height - 1) - y;
                    }
                    else if (turns == 2)
                    {
                        destY = (height - 1) - y;
                        destX = (width - 1) - x;
                    }
                    else //if (turns == 3)
                    {
                        destY = (width - 1) - x;
                        destX = y;
                    }

                    if (rot > 3) //flip X
                        destX = (destW - 1) - destX;

                    var destIndex = destY * destW * bpp + destX * bpp;
                    for (var i = 0; i < bpp; i++)
                        output[destIndex + i] = buffer[sourceIndex + i];
                }
            }

            output.AsSpan(..buffer.Length).CopyTo(buffer);
            ArrayPool<byte>.Shared.Return(output);
        }

        private static BgraColour Lerp(in BgraColour c0, in BgraColour c1, in float fraction)
        {
            return new BgraColour
            {
                B = Lerp(c0.B, c1.B, fraction),
                G = Lerp(c0.G, c1.G, fraction),
                R = Lerp(c0.R, c1.R, fraction),
                A = Lerp(c0.A, c1.A, fraction)
            };
        }

        private static IEnumerable<T> SkipNth<T>(IEnumerable<T> enumerable, int n)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                if (++i != n)
                    yield return item;
                else
                    i = 0;
            }
        }

        private static byte[] ToArray(IEnumerable<byte> source, bool bgr24, int height, int width)
        {
            var len = height * width * (bgr24 ? 3 : 4);
            var arraySource = source as byte[];
            if (arraySource?.Length >= len)
            {
                if (arraySource.Length == len)
                    return arraySource;

                var subArray = new byte[len];
                Array.Copy(arraySource, subArray, len);
                return subArray;
            }

            var output = new byte[len];
            var i = 0;
            foreach (var b in source)
            {
                output[i++] = b;
                if (i >= output.Length)
                    break;
            }

            return output;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BgraColour
        {
            public delegate BgraColour ColorUnpackMethod(ushort value);

            public byte B, G, R, A;

            public byte this[int index]
            {
                readonly get
                {
                    return index switch
                    {
                        0 => B,
                        1 => G,
                        2 => R,
                        3 => A,
                        _ => throw new ArgumentOutOfRangeException(nameof(index))
                    };
                }
                set
                {
                    switch (index)
                    {
                        case 0: B = value; break;
                        case 1: G = value; break;
                        case 2: R = value; break;
                        case 3: A = value; break;
                        default: throw new ArgumentOutOfRangeException(nameof(index));
                    }
                }
            }

            public readonly IEnumerable<byte> AsEnumerable(bool bgr24)
            {
                yield return B;
                yield return G;
                yield return R;
                if (!bgr24)
                    yield return A;
            }

            public readonly void CopyTo(byte[] destination, int destinationIndex, bool bgr24)
            {
                destination[destinationIndex] = B;
                destination[destinationIndex + 1] = G;
                destination[destinationIndex + 2] = R;
                if (!bgr24)
                    destination[destinationIndex + 3] = A;
            }

            public static BgraColour From565(ushort value)
            {
                const byte maskB = 0x1F;
                const byte maskG = 0x3F;
                const byte maskR = 0x1F;

                return new BgraColour
                {
                    B = (byte)MathF.Round(255f / maskB * (value & maskB)),
                    G = (byte)MathF.Round(255f / maskG * ((value >> 5) & maskG)),
                    R = (byte)MathF.Round(255f / maskR * ((value >> 11) & maskR)),
                    A = byte.MaxValue
                };
            }

            public static BgraColour From5551(ushort value)
            {
                const byte maskB = 0x1F;
                const byte maskG = 0x1F;
                const byte maskR = 0x1F;
                const byte maskA = 0x01;

                return new BgraColour
                {
                    B = (byte)MathF.Round(255f / maskB * (value & maskB)),
                    G = (byte)MathF.Round(255f / maskG * ((value >> 5) & maskG)),
                    R = (byte)MathF.Round(255f / maskR * ((value >> 10) & maskR)),
                    A = (byte)MathF.Round(255f / maskA * ((value >> 15) & maskA))
                };
            }

            public static BgraColour From4444(ushort value)
            {
                const byte maskB = 0x0F;
                const byte maskG = 0x0F;
                const byte maskR = 0x0F;
                const byte maskA = 0x0F;

                return new BgraColour
                {
                    B = (byte)MathF.Round(255f / maskB * (value & maskB)),
                    G = (byte)MathF.Round(255f / maskG * ((value >> 4) & maskG)),
                    R = (byte)MathF.Round(255f / maskR * ((value >> 8) & maskR)),
                    A = (byte)MathF.Round(255f / maskA * ((value >> 12) & maskA)),
                };
            }

            public override readonly string ToString()
            {
                return string.Format("{{ {0,3}, {1,3}, {2,3}, {3,3} }} #{0:X2}{1:X2}{2:X2}{3:X2}", B, G, R, A);
            }
        }
    }
}
