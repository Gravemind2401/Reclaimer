using StbImageWriteSharp;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Reclaimer.Drawing
{
    public partial class DdsImage
    {
        private byte[] GetExportBuffer(DdsOutputArgs args, out int exportWidth, out int exportHeight)
        {
            ArgumentNullException.ThrowIfNull(args);

            var virtualHeight = Height;

            var isCubeMap = TextureFlags.HasFlag(TextureFlags.DdsSurfaceFlagsCubemap) && CubemapFlags.HasFlag(CubemapFlags.DdsCubemapAllFaces);
            if (isCubeMap)
                virtualHeight *= 6;

            //TODO: use custom array pool with max size of 4096*4096*4 (MaxArraysPerBucket = 8?)
            //pass rented arrays to decompression methods as output buffers (or output spans, even)
            var buffer = DecompressPixelData(args.Bgr24);

            //MaskChannels() will alter the array that gets passed in, so make sure it doesnt modify the original dds pixels array
            //also prevents later channel swaps from affecting the original array
            if (ReferenceEquals(buffer, data))
                buffer = (byte[])data.Clone();

            if (args.UseChannelMask)
                MaskChannels(buffer, args.Options);

            //if (isCubeMap && args.ValidCubeLayout)
            //    buffer = UnwrapCubemapSource(buffer, args.Layout);

            exportWidth = Width;
            exportHeight = virtualHeight;

            return buffer;
        }

        #region WriteToDisk
        /// <remarks>If the file already exists, it will be overwritten.</remarks>
        /// <inheritdoc cref="WriteToDisk(string, ImageFormat, bool)" />
        public void WriteToDisk(string fileName, ImageFormat format) => WriteToDisk(fileName, format, new DdsOutputArgs(), true);

        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format
        /// using the default decompression options and a non-cubemap layout.
        /// </summary>
        /// <inheritdoc cref="WriteToDisk(string, ImageFormat, DdsOutputArgs, bool)" />
        public void WriteToDisk(string fileName, ImageFormat format, bool overwrite) => WriteToDisk(fileName, format, new DdsOutputArgs(), overwrite);

        /// <remarks>If the file already exists, it will be overwritten.</remarks>
        /// <inheritdoc cref="WriteToDisk(string, ImageFormat, DdsOutputArgs, bool)" />
        public void WriteToDisk(string fileName, ImageFormat format, DdsOutputArgs args) => WriteToDisk(fileName, format, args, true);

        /// <summary>
        /// Decompresses any compressed pixel data and saves the image to a file on disk using a standard image format.
        /// </summary>
        /// <param name="fileName">The full path of the file to write.</param>
        /// <param name="format">The image format to write with.</param>
        /// <param name="args">Parameters to use when writing the image.</param>
        /// <param name="overwrite">Whether to overwrite the file if it already exists.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="NotSupportedException" />
        public void WriteToDisk(string fileName, ImageFormat format, DdsOutputArgs args, bool overwrite)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(args);

            if (!overwrite && File.Exists(fileName))
                return;

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
        /// <inheritdoc cref="WriteToStream(Stream, ImageFormat, DdsOutputArgs)" />
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
            ArgumentNullException.ThrowIfNull(args);

            if (!Enum.IsDefined(format))
                throw new ArgumentException("Undefined format", nameof(format));

            if (format == ImageFormat.Dds)
            {
                WriteToStream(stream);
                return;
            }

            var buffer = GetExportBuffer(args, out var exportWidth, out var exportHeight);
            var hasAlpha = !args.Bgr24;

            switch (format)
            {
                case ImageFormat.Bmp:
                    BmpWriter.Write(stream, buffer, exportWidth, exportHeight, hasAlpha);
                    break;
                case ImageFormat.Tga:
                    TgaWriter.Write(stream, buffer, exportWidth, exportHeight, hasAlpha);
                    break;
                case ImageFormat.Tiff:
                    //TIF format only supports RGB(A) ordering
                    BgrToRgb(buffer, hasAlpha);
                    TiffWriter.Write(stream, buffer, exportWidth, exportHeight, hasAlpha);
                    break;
                case ImageFormat.Png:
                    //stb library requires RGB(A) so swap from BGR(A) first
                    BgrToRgb(buffer, hasAlpha);
                    var components = hasAlpha ? ColorComponents.RedGreenBlueAlpha : ColorComponents.RedGreenBlue;
                    new ImageWriter().WritePng(buffer, exportWidth, exportHeight, components, stream);
                    break;
                default:
                    throw new NotSupportedException($"The format '{format}' is not supported.");
            }
        }

        private static void BgrToRgb(Span<byte> buffer, bool hasAlpha)
        {
            var bpp = hasAlpha ? 4 : 3;
            var i = 0;

            if (Vector128.IsHardwareAccelerated)
            {
                ref var start = ref MemoryMarshal.GetReference(buffer);

                if (hasAlpha)
                {
                    var mask = Vector128.Create((byte)2, 1, 0, 3, 6, 5, 4, 7, 10, 9, 8, 11, 14, 13, 12, 15);
                    for (; i <= buffer.Length - Vector128<byte>.Count; i += 16)
                        Vector128.Shuffle(Vector128.LoadUnsafe(ref start, (nuint)i), mask).StoreUnsafe(ref start, (nuint)i);
                }
                else
                {
                    var mask = Vector128.Create((byte)2, 1, 0, 5, 4, 3, 8, 7, 6, 11, 10, 9, 14, 13, 12, 15);
                    for (; i <= buffer.Length - Vector128<byte>.Count; i += 15)
                        Vector128.Shuffle(Vector128.LoadUnsafe(ref start, (nuint)i), mask).StoreUnsafe(ref start, (nuint)i);
                }
            }

            //remaining pixels that dont fill a whole vector
            for (; i < buffer.Length; i += bpp)
                (buffer[i], buffer[i + 2]) = (buffer[i + 2], buffer[i]);
        }
        #endregion
    }
}
