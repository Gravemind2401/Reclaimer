using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/* https://docs.microsoft.com/en-us/windows/desktop/direct3d10/d3d10-graphics-programming-guide-resources-block-compression */
namespace System.Drawing.Dds
{
    public partial class DdsImage
    {
        private delegate IEnumerable<byte> Decompress(byte[] data, int height, int width);

        private static readonly Dictionary<DxgiFormat, Decompress> decompressMethodsDxgi = new Dictionary<DxgiFormat, Decompress>
        {
            { DxgiFormat.BC1_Typeless, DecompressBC1 },
            { DxgiFormat.BC1_UNorm, DecompressBC1 },
            { DxgiFormat.BC2_Typeless, DecompressBC2 },
            { DxgiFormat.BC2_UNorm, DecompressBC2 },
            { DxgiFormat.BC3_Typeless, DecompressBC3 },
            { DxgiFormat.BC3_UNorm, DecompressBC3 },
            { DxgiFormat.BC4_Typeless, DecompressBC4 },
            { DxgiFormat.BC4_UNorm, DecompressBC4 },
            { DxgiFormat.BC4_SNorm, DecompressBC4 },
            { DxgiFormat.BC5_Typeless, DecompressBC5 },
            { DxgiFormat.BC5_UNorm, DecompressBC5 },
            { DxgiFormat.BC5_SNorm, DecompressBC5 },
            { DxgiFormat.B5G6R5_UNorm, DecompressB5G6R5 },
            { DxgiFormat.B5G5R5A1_UNorm, DecompressB5G5R5A1 },
            { DxgiFormat.P8, DecompressY8 },
            { DxgiFormat.B4G4R4A4_UNorm, DecompressB4G4R4A4 }
        };

        private static readonly Dictionary<XboxFormat, Decompress> decompressMethodsXbox = new Dictionary<XboxFormat, Decompress>
        {
            { XboxFormat.A8, DecompressA8 },
            { XboxFormat.AY8, DecompressAY8 },
            { XboxFormat.CTX1, DecompressCTX1 },
            { XboxFormat.DXN, DecompressDXN },
            { XboxFormat.DXN_mono_alpha, DecompressDXN_mono_alpha },
            { XboxFormat.DXT3a_scalar, DecompressDXT3a_scalar },
            { XboxFormat.DXT3a_mono, DecompressDXT3a_mono },
            { XboxFormat.DXT3a_alpha, DecompressDXT3a_alpha },
            { XboxFormat.DXT5a_scalar, DecompressDXT5a_scalar },
            { XboxFormat.DXT5a_mono, DecompressDXT5a_mono },
            { XboxFormat.DXT5a_alpha, DecompressDXT5a_alpha },
            { XboxFormat.Y8, DecompressY8 },
            { XboxFormat.Y8A8, DecompressY8A8 },
        };

        private static readonly Dictionary<FourCC, Decompress> decompressMethodsFourCC = new Dictionary<FourCC, Decompress>
        {
            { FourCC.DXT1, DecompressBC1 },
            { FourCC.DXT2, DecompressBC2 },
            { FourCC.DXT3, DecompressBC2 },
            { FourCC.DXT4, DecompressBC3 },
            { FourCC.DXT5, DecompressBC3 },
            { FourCC.BC4U, DecompressBC4 },
            { FourCC.BC4S, DecompressBC4 },
            { FourCC.ATI1, DecompressBC4 },
            { FourCC.BC5U, DecompressBC5 },
            { FourCC.BC5S, DecompressBC5 },
            { FourCC.ATI2, DecompressBC5 },
        };

        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <param name="format">The image format to write with.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToDisk(string fileName, ImageFormat format) => WriteToDisk(fileName, format, DecompressOptions.Default);

        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format,
        /// optionally unwrapping cubemap images.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <param name="format">The image format to write with.</param>
        /// <param name="unwrapCubemap">True to unwrap a cubemap. False to output each tile horizontally.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToDisk(string fileName, ImageFormat format, DecompressOptions options)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

            var dir = Directory.GetParent(fileName).FullName;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                WriteToStream(fs, format, options);
        }

        /// <summary>
        /// Decompresses any compressed pixel data and writes the image to a stream using a standard image format.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="format">The image format to write with.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToStream(Stream stream, ImageFormat format) => WriteToStream(stream, format, DecompressOptions.Default);

        /// <summary>
        /// Decompresses any compressed pixel data and writes the image to a stream using a standard image format.
        /// optionally unwrapping cubemap images.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="format">The image format to write with.</param>
        /// <param name="unwrapCubemap">True to unwrap a cubemap. False to output each tile horizontally.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToStream(Stream stream, ImageFormat format, DecompressOptions options)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (format == null)
                throw new ArgumentNullException(nameof(format));

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
            else throw new NotSupportedException("The ImageFormat is not supported.");

            var source = ToBitmapSource(options);
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
        }

        /// <summary>
        /// Decompresses any compressed pixel data and returns the image data as a <see cref="BitmapSource"/>
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public BitmapSource ToBitmapSource() => ToBitmapSource(DecompressOptions.Default);

        /// <summary>
        /// Decompresses any compressed pixel data and returns the image data as a <see cref="BitmapSource"/>
        /// </summary>
        /// <param name="options">Options to use during decompression.</param>
        /// <exception cref="NotSupportedException" />
        public BitmapSource ToBitmapSource(DecompressOptions options)
        {
            const double dpi = 96;
            var virtualHeight = Height;

            var isCubeMap = TextureFlags.HasFlag(TextureFlags.DdsSurfaceFlagsCubemap) && CubemapFlags.HasFlag(CubemapFlags.DdsCubemapAllFaces);
            if (isCubeMap) virtualHeight *= 6;

            IEnumerable<byte> bgra;
            if (header.PixelFormat.FourCC == (uint)FourCC.DX10)
            {
                if (decompressMethodsDxgi.ContainsKey(dx10Header.DxgiFormat))
                    bgra = decompressMethodsDxgi[dx10Header.DxgiFormat](data, virtualHeight, Width);
                else
                {
                    switch (dx10Header.DxgiFormat)
                    {
                        case DxgiFormat.B8G8R8X8_UNorm:
                        case DxgiFormat.B8G8R8A8_UNorm:
                            bgra = data;
                            break;

                        default: throw new NotSupportedException("The DxgiFormat is not supported.");
                    }
                }
            }
            else if (header.PixelFormat.FourCC == (uint)FourCC.XBOX)
            {
                if (decompressMethodsXbox.ContainsKey(xboxHeader.XboxFormat))
                    bgra = decompressMethodsXbox[xboxHeader.XboxFormat](data, virtualHeight, Width);
                else throw new NotSupportedException("The XboxFormat is not supported.");
            }
            else
            {
                var fourcc = (FourCC)header.PixelFormat.FourCC;
                if (decompressMethodsFourCC.ContainsKey(fourcc))
                    bgra = decompressMethodsFourCC[fourcc](data, virtualHeight, Width);
                else throw new NotSupportedException("The FourCC is not supported.");
            }

            var format = PixelFormats.Bgra32;
            var bpp = 4;

            //at least one 'remove channel' flag is set
            if ((options & DecompressOptions.RemoveAllChannels) != 0)
                bgra = SelectChannels(bgra, options);

            if (options.HasFlag(DecompressOptions.Bgr24))
            {
                format = PixelFormats.Bgr24;
                bpp = 3;
                bgra = TakeSkipRepeat(bgra, 3, 1);
            }

            var source = BitmapSource.Create(Width, virtualHeight, dpi, dpi, format, null, bgra.ToArray(), Width * bpp);

            if (isCubeMap && options.HasFlag(DecompressOptions.UnwrapCubemap))
                source = UnwrapCubemapSource(source, dpi, format);

            return source;
        }

        private IEnumerable<byte> SelectChannels(IEnumerable<byte> sourcePixels, DecompressOptions channels)
        {
            var blue = !channels.HasFlag(DecompressOptions.RemoveBlueChannel);
            var green = !channels.HasFlag(DecompressOptions.RemoveGreenChannel);
            var red = !channels.HasFlag(DecompressOptions.RemoveRedChannel);
            var alpha = !channels.HasFlag(DecompressOptions.RemoveAlphaChannel);

            var channelIndex = -1;
            if (Convert.ToInt32(blue) + Convert.ToInt32(green) + Convert.ToInt32(red) + Convert.ToInt32(alpha) == 1)
            {
                if (blue) channelIndex = 0;
                else if (green) channelIndex = 1;
                else if (red) channelIndex = 2;
                else if (alpha) channelIndex = 3;
            }

            var temp = new List<byte>(4);
            foreach (var b in sourcePixels)
            {
                temp.Add(b);
                if (temp.Count < 4)
                    continue;

                if (channelIndex >= 0)
                {
                    yield return temp[channelIndex];
                    yield return temp[channelIndex];
                    yield return temp[channelIndex];
                    yield return byte.MaxValue;
                }
                else
                {
                    yield return blue ? temp[0] : (byte)0;
                    yield return green ? temp[1] : (byte)0;
                    yield return red ? temp[2] : (byte)0;
                    yield return alpha ? temp[3] : byte.MaxValue;
                }

                temp.Clear();
            }
        }

        private BitmapSource UnwrapCubemapSource(BitmapSource source, double dpi, Windows.Media.PixelFormat format)
        {
            var bpp = format.BitsPerPixel / 8;
            var stride = bpp * Width;
            var dest = new WriteableBitmap(Width * 4, Height * 3, dpi, dpi, format, null);

            var xTiles = new[] { 2, 1, 0, 1, 1, 3 };
            var yTiles = new[] { 1, 0, 1, 2, 1, 1 };

            for (int i = 0; i < 6; i++)
            {
                var sourceRect = new Int32Rect(0, Height * i, Width, Height);
                var destRect = new Int32Rect(xTiles[i] * Width, yTiles[i] * Height, Width, Height);

                var buffer = new byte[Width * Height * bpp];
                source.CopyPixels(sourceRect, buffer, stride, 0);
                dest.WritePixels(destRect, buffer, stride, 0);
            }

            return dest;
        }

        #region Standard Decompression Methods
        internal static IEnumerable<byte> DecompressB5G6R5(byte[] data, int height, int width)
        {
            return Enumerable.Range(0, height * width).SelectMany(i => BgraColour.From565(BitConverter.ToUInt16(data, i * 2)).AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressB5G5R5A1(byte[] data, int height, int width)
        {
            return Enumerable.Range(0, height * width).SelectMany(i => BgraColour.From5551(BitConverter.ToUInt16(data, i * 2)).AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressB4G4R4A4(byte[] data, int height, int width)
        {
            return Enumerable.Range(0, height * width).SelectMany(i => BgraColour.From4444(BitConverter.ToUInt16(data, i * 2)).AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC1(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    var c0 = BitConverter.ToUInt16(data, srcIndex);
                    var c1 = BitConverter.ToUInt16(data, srcIndex + 2);

                    palette[0] = BgraColour.From565(c0);
                    palette[1] = BgraColour.From565(c1);

                    if (c0 <= c1)
                    {
                        palette[2] = Lerp(palette[0], palette[1], 1 / 2f);
                        palette[3] = new BgraColour(); //zero on all channels
                    }
                    else
                    {
                        palette[2] = Lerp(palette[0], palette[1], 1 / 3f);
                        palette[3] = Lerp(palette[0], palette[1], 2 / 3f);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var indexBits = data[srcIndex + 4 + i];
                        for (int j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);
                            output[destIndex] = palette[pIndex];
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC2(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 16;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    palette[0] = BgraColour.From565(BitConverter.ToUInt16(data, srcIndex + 8));
                    palette[1] = BgraColour.From565(BitConverter.ToUInt16(data, srcIndex + 10));

                    palette[2] = Lerp(palette[0], palette[1], 1 / 3f);
                    palette[3] = Lerp(palette[0], palette[1], 2 / 3f);

                    for (int i = 0; i < 4; i++)
                    {
                        var alphaBits = BitConverter.ToUInt16(data, srcIndex + i * 2);
                        var indexBits = data[srcIndex + 12 + i];
                        for (int j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);

                            var result = palette[pIndex];
                            result.a = (byte)(((alphaBits >> j * 4) & 0xF) * (0xFF / 0xF));
                            output[destIndex] = result;
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC3(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var rgbPalette = new BgraColour[4];
            var alphaPalette = new byte[8];

            const int bytesPerBlock = 16;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    rgbPalette[0] = BgraColour.From565(BitConverter.ToUInt16(data, srcIndex + 8));
                    rgbPalette[1] = BgraColour.From565(BitConverter.ToUInt16(data, srcIndex + 10));

                    rgbPalette[2] = Lerp(rgbPalette[0], rgbPalette[1], 1 / 3f);
                    rgbPalette[3] = Lerp(rgbPalette[0], rgbPalette[1], 2 / 3f);

                    alphaPalette[0] = data[srcIndex];
                    alphaPalette[1] = data[srcIndex + 1];

                    var gradients = alphaPalette[0] > alphaPalette[1] ? 7f : 5f;
                    for (int i = 1; i < gradients; i++)
                        alphaPalette[i + 1] = Lerp(alphaPalette[0], alphaPalette[1], i / gradients);

                    if (alphaPalette[0] <= alphaPalette[1])
                    {
                        alphaPalette[6] = byte.MinValue;
                        alphaPalette[7] = byte.MaxValue;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var rgbIndexBits = data[srcIndex + 12 + i];
                        for (int j = 0; j < 4; j++)
                        {
                            var pixelIndex = i * 4 + j;
                            var alphaStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var alphaIndexBits = (data[alphaStart + 2] << 16) | (data[alphaStart + 1] << 8) | data[alphaStart];

                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var pIndex = (byte)((rgbIndexBits >> j * 2) & 0x3);

                            var result = rgbPalette[pIndex];
                            result.a = alphaPalette[(alphaIndexBits >> (pixelIndex % 8) * 3) & 0x7];
                            output[destIndex] = result;
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC4(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var palette = new byte[8];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    palette[0] = data[srcIndex];
                    palette[1] = data[srcIndex + 1];

                    var gradients = palette[0] > palette[1] ? 7f : 5f;
                    for (int i = 1; i < gradients; i++)
                        palette[i + 1] = Lerp(palette[0], palette[1], i / gradients);

                    if (palette[0] <= palette[1])
                    {
                        palette[6] = byte.MinValue;
                        palette[7] = byte.MaxValue;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            var pixelIndex = i * 4 + j;
                            var pStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var pIndexBits = (data[pStart + 2] << 16) | (data[pStart + 1] << 8) | data[pStart];

                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var pIndex = (byte)((pIndexBits >> (pixelIndex % 8) * 3) & 0x7);

                            output[destIndex] = new BgraColour
                            {
                                b = palette[pIndex],
                                g = palette[pIndex],
                                r = palette[pIndex],
                                a = byte.MaxValue,
                            };
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC5(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var rPalette = new byte[8];
            var gPalette = new byte[8];

            const int bytesPerBlock = 16;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;

                    rPalette[0] = data[srcIndex];
                    rPalette[1] = data[srcIndex + 1];

                    var gradients = rPalette[0] > rPalette[1] ? 7f : 5f;
                    for (int i = 1; i < gradients; i++)
                        rPalette[i + 1] = Lerp(rPalette[0], rPalette[1], i / gradients);

                    if (rPalette[0] <= rPalette[1])
                    {
                        rPalette[6] = byte.MinValue;
                        rPalette[7] = byte.MaxValue;
                    }

                    gPalette[0] = data[srcIndex + 8];
                    gPalette[1] = data[srcIndex + 9];

                    gradients = gPalette[0] > gPalette[1] ? 7f : 5f;
                    for (int i = 1; i < gradients; i++)
                        gPalette[i + 1] = Lerp(gPalette[0], gPalette[1], i / gradients);

                    if (gPalette[0] <= gPalette[1])
                    {
                        gPalette[6] = byte.MinValue;
                        gPalette[7] = byte.MaxValue;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            var pixelIndex = i * 4 + j;

                            var rStart = srcIndex + (pixelIndex < 8 ? 2 : 5);
                            var rIndexBits = (data[rStart + 2] << 16) | (data[rStart + 1] << 8) | data[rStart];

                            var gStart = srcIndex + (pixelIndex < 8 ? 10 : 13);
                            var gIndexBits = (data[gStart + 2] << 16) | (data[gStart + 1] << 8) | data[gStart];

                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var shift = (pixelIndex % 8) * 3;

                            var rIndex = (byte)((rIndexBits >> shift) & 0x7);
                            var gIndex = (byte)((gIndexBits >> shift) & 0x7);

                            output[destIndex] = new BgraColour
                            {
                                //b = rPalette[rIndex],
                                g = gPalette[gIndex],
                                r = rPalette[rIndex],
                                a = byte.MaxValue,
                            };
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }
        #endregion

        #region Xbox Decompression Methods
        internal static IEnumerable<byte> DecompressA8(byte[] data, int height, int width)
        {
            return data.SelectMany(b => Enumerable.Range(0, 4).Select(i => i < 3 ? byte.MinValue : b));
        }

        internal static IEnumerable<byte> DecompressAY8(byte[] data, int height, int width)
        {
            return data.SelectMany(b => Enumerable.Range(0, 4).Select(i => b));
        }

        internal static IEnumerable<byte> DecompressY8(byte[] data, int height, int width)
        {
            return data.SelectMany(b => Enumerable.Range(0, 4).Select(i => i < 3 ? b : byte.MaxValue));
        }

        internal static IEnumerable<byte> DecompressY8A8(byte[] data, int height, int width)
        {
            for (int i = 0; i < height * width; i++)
            {
                yield return data[i * 2 + 1];
                yield return data[i * 2 + 1];
                yield return data[i * 2 + 1];
                yield return data[i * 2];
            }
        }

        internal static IEnumerable<byte> DecompressBC1DualChannel(byte[] data, int height, int width)
        {
            var output = new BgraColour[width * height];
            var palette = new BgraColour[4];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    palette[0] = new BgraColour { g = data[srcIndex + 0], r = data[srcIndex + 1], a = byte.MaxValue };
                    palette[1] = new BgraColour { g = data[srcIndex + 2], r = data[srcIndex + 3], a = byte.MaxValue };

                    palette[2] = Lerp(palette[0], palette[1], 1 / 3f);
                    palette[3] = Lerp(palette[0], palette[1], 2 / 3f);

                    for (int i = 0; i < 4; i++)
                    {
                        var indexBits = data[srcIndex + 4 + i];
                        for (int j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;

                            var destIndex = destY * width + destX;
                            var pIndex = (byte)((indexBits >> j * 2) & 0x3);
                            var colour = palette[pIndex];
                            colour.b = CalculateZVector(colour.r, colour.g);
                            output[destIndex] = colour;
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC2AlphaOnly(byte[] data, int height, int width, bool bgr, bool a)
        {
            var output = new BgraColour[width * height];

            const int bytesPerBlock = 8;
            var xBlocks = width / 4;
            var yBlocks = height / 4;

            for (int yBlock = 0; yBlock < yBlocks; yBlock++)
            {
                for (int xBlock = 0; xBlock < xBlocks; xBlock++)
                {
                    var srcIndex = (yBlock * xBlocks + xBlock) * bytesPerBlock;
                    for (int i = 0; i < 4; i++)
                    {
                        var alphaBits = BitConverter.ToUInt16(data, srcIndex + i * 2);
                        for (int j = 0; j < 4; j++)
                        {
                            var destX = xBlock * 4 + j;
                            var destY = yBlock * 4 + i;
                            var destIndex = destY * width + destX;

                            var value = (byte)(((alphaBits >> j * 4) & 0xF) * (0xFF / 0xF));
                            output[destIndex] = new BgraColour
                            {
                                b = bgr ? value : byte.MinValue,
                                g = bgr ? value : byte.MinValue,
                                r = bgr ? value : byte.MinValue,
                                a = a ? value : byte.MaxValue,
                            };
                        }
                    }
                }
            }

            return output.SelectMany(c => c.AsEnumerable());
        }

        internal static IEnumerable<byte> DecompressBC3AlphaOnly(byte[] data, int height, int width, bool bgr, bool a)
        {
            //same bit layout as BC4
            data = DecompressBC4(data, height, width).ToArray();

            for (int i = 0; i < data.Length; i += 4)
            {
                var scalar = data[i];
                yield return bgr ? scalar : byte.MinValue;
                yield return bgr ? scalar : byte.MinValue;
                yield return bgr ? scalar : byte.MinValue;
                yield return a ? scalar : byte.MaxValue;
            }
        }

        internal static IEnumerable<byte> DecompressCTX1(byte[] data, int height, int width)
        {
            return DecompressBC1DualChannel(data, height, width);
        }

        internal static IEnumerable<byte> DecompressDXN(byte[] data, int height, int width)
        {
            data = DecompressBC5(data, height, width).ToArray();
            for (int i = 0; i < data.Length; i += 4)
                data[i] = CalculateZVector(data[i + 2], data[i + 1]);

            return data;
        }

        internal static IEnumerable<byte> DecompressDXN_mono_alpha(byte[] data, int height, int width)
        {
            data = DecompressBC5(data, height, width).ToArray();
            for (int i = 0; i < data.Length; i += 4)
            {
                var g = data[i + 1];
                var r = data[i + 2];

                yield return r;
                yield return r;
                yield return r;
                yield return g;
            }
        }

        internal static IEnumerable<byte> DecompressDXT3a_scalar(byte[] data, int height, int width)
        {
            return DecompressBC2AlphaOnly(data, height, width, true, true);
        }

        internal static IEnumerable<byte> DecompressDXT3a_mono(byte[] data, int height, int width)
        {
            return DecompressBC2AlphaOnly(data, height, width, true, false);
        }

        internal static IEnumerable<byte> DecompressDXT3a_alpha(byte[] data, int height, int width)
        {
            return DecompressBC2AlphaOnly(data, height, width, false, true);
        }

        internal static IEnumerable<byte> DecompressDXT5a_scalar(byte[] data, int height, int width)
        {
            return DecompressBC3AlphaOnly(data, height, width, true, true);
        }

        internal static IEnumerable<byte> DecompressDXT5a_mono(byte[] data, int height, int width)
        {
            return DecompressBC3AlphaOnly(data, height, width, true, false);
        }

        internal static IEnumerable<byte> DecompressDXT5a_alpha(byte[] data, int height, int width)
        {
            return DecompressBC3AlphaOnly(data, height, width, false, true);
        }
        #endregion

        private static byte Lerp(byte p1, byte p2, float fraction)
        {
            return (byte)((p1 * (1 - fraction)) + (p2 * fraction));
        }

        private static float Lerp(float p1, float p2, float fraction)
        {
            return (p1 * (1 - fraction)) + (p2 * fraction);
        }

        private static byte CalculateZVector(byte r, byte g)
        {
            var x = Lerp(-1f, 1f, r / 255f);
            var y = Lerp(-1f, 1f, g / 255f);
            var z = (float)Math.Sqrt(1 - x * x - y * y);

            return (byte)((z + 1) / 2 * 255f);
        }

        private static BgraColour Lerp(BgraColour c0, BgraColour c1, float fraction)
        {
            return new BgraColour
            {
                b = Lerp(c0.b, c1.b, fraction),
                g = Lerp(c0.g, c1.g, fraction),
                r = Lerp(c0.r, c1.r, fraction),
                a = Lerp(c0.a, c1.a, fraction)
            };
        }

        private static IEnumerable<T> TakeSkipRepeat<T>(IEnumerable<T> enumerable, int take, int skip)
        {
            int i = 0;
            foreach (var item in enumerable)
            {
                if (i < take)
                    yield return item;

                if (++i >= take + skip)
                    i = 0;
            }
        }
    }

    [Flags]
    public enum DecompressOptions
    {
        /// <summary>
        /// The default option. If no other flags are specified, 32bpp BGRA will be used.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Outputs pixel data in 24bpp BGR format. Does not output an alpha channel regardless of any other flags specified.
        /// </summary>
        Bgr24 = 1,

        /// <summary>
        /// When used on a cubemap image, unwraps each cube face onto a single bitmap.
        /// </summary>
        UnwrapCubemap = 2,

        /// <summary>
        /// Replaces all blue channel data with zeros.
        /// </summary>
        RemoveBlueChannel = 4,

        /// <summary>
        /// Replaces all green channel data with zeros.
        /// </summary>
        RemoveGreenChannel = 8,

        /// <summary>
        /// Replaces all red channel data with zeros.
        /// </summary>
        RemoveRedChannel = 16,

        /// <summary>
        /// Replaces all alpha channel data with full opacity.
        /// </summary>
        RemoveAlphaChannel = 32,

        /// <summary>
        /// Replicates the blue channel data over the green and red channels. The alpha channel will be fully opaque.
        /// </summary>
        BlueChannelOnly = RemoveGreenChannel | RemoveRedChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the green channel data over the blue and red channels. The alpha channel will be fully opaque.
        /// </summary>
        GreenChannelOnly = RemoveBlueChannel | RemoveRedChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the red channel data over the blue and green and channels. The alpha channel will be fully opaque.
        /// </summary>
        RedChannelOnly = RemoveBlueChannel | RemoveGreenChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the alpha channel data over the blue, green and red channels. The alpha channel will be fully opaque.
        /// </summary>
        AlphaChannelOnly = RemoveBlueChannel | RemoveGreenChannel | RemoveRedChannel,

        /// <summary>
        /// Produces a solid black image with opaque alpha.
        /// </summary>
        RemoveAllChannels = RemoveBlueChannel | RemoveGreenChannel | RemoveRedChannel | RemoveAlphaChannel
    }

    internal struct BgraColour
    {
        public byte b, g, r, a;

        public IEnumerable<byte> AsEnumerable()
        {
            yield return b;
            yield return g;
            yield return r;
            yield return a;
        }

        public static BgraColour From565(ushort value)
        {
            const byte BMask = 0x1F;
            const byte GMask = 0x3F;
            const byte RMask = 0x1F;

            return new BgraColour
            {
                b = (byte)((0xFF / BMask) * (value & BMask)),
                g = (byte)((0xFF / GMask) * ((value >> 5) & GMask)),
                r = (byte)((0xFF / RMask) * ((value >> 11) & RMask)),
                a = byte.MaxValue
            };
        }

        public static BgraColour From5551(ushort value)
        {
            const byte BMask = 0x1F;
            const byte GMask = 0x1F;
            const byte RMask = 0x1F;
            const byte AMask = 0x01;

            return new BgraColour
            {
                b = (byte)((0xFF / BMask) * (value & BMask)),
                g = (byte)((0xFF / GMask) * ((value >> 5) & GMask)),
                r = (byte)((0xFF / RMask) * ((value >> 10) & RMask)),
                a = (byte)((0xFF / AMask) * ((value >> 15) & AMask))
            };
        }

        public static BgraColour From4444(ushort value)
        {
            const byte BMask = 0x0F;
            const byte GMask = 0x0F;
            const byte RMask = 0x0F;
            const byte AMask = 0x0F;

            return new BgraColour
            {
                b = (byte)((0xFF / BMask) * (value & BMask)),
                g = (byte)((0xFF / GMask) * ((value >> 4) & GMask)),
                r = (byte)((0xFF / RMask) * ((value >> 8) & RMask)),
                a = (byte)((0xFF / AMask) * ((value >> 12) & AMask)),
            };
        }
    }
}
