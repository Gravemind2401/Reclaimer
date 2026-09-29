using BitMiracle.LibTiff.Classic;
using StbImageWriteSharp;

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

            //swap from BGR(A) to RGB(A)
            //TODO: make this work with SIMD (or self implement output formats to eliminate the need to do it at all)
            var bpp = args.Bpp;
            var buffer = GetExportBuffer(args, out var exportWidth, out var exportHeight);
            var span = buffer.AsSpan();
            for (var i = 0; i < span.Length; i += bpp)
                (span[i + 2], span[i]) = (span[i], span[i + 2]);

            switch (format)
            {
                case ImageFormat.Tiff:
                    WriteToStreamTiff(stream, buffer, exportWidth, exportHeight, args);
                    break;
                default:
                    WriteToStreamStb(stream, buffer, exportWidth, exportHeight, args, format);
                    break;
            }
        }

        private static void WriteToStreamStb(Stream stream, byte[] buffer, int width, int height, DdsOutputArgs args, ImageFormat format)
        {
            var writer = new ImageWriter();
            var components = args.Bgr24 ? ColorComponents.RedGreenBlue : ColorComponents.RedGreenBlueAlpha;

            switch (format)
            {
                case ImageFormat.Png:
                    writer.WritePng(buffer, width, height, components, stream);
                    break;
                case ImageFormat.Bmp:
                    writer.WriteBmp(buffer, width, height, components, stream);
                    break;
                case ImageFormat.Tga:
                    writer.WriteTga(buffer, width, height, components, stream);
                    break;
            }
        }

        private static void WriteToStreamTiff(Stream stream, byte[] buffer, int width, int height, DdsOutputArgs args)
        {
            using var tiff = Tiff.ClientOpen(nameof(DdsImage), "w", stream, new TiffStream());
            if (tiff == null)
                throw new IOException("LibTiff failed to initialise the underlying stream wrapper.");

            tiff.SetField(TiffTag.IMAGEWIDTH, width);
            tiff.SetField(TiffTag.IMAGELENGTH, height);
            tiff.SetField(TiffTag.SAMPLESPERPIXEL, args.Bpp);
            tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
            tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
            tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            tiff.SetField(TiffTag.ROWSPERSTRIP, height);

            if (!args.Bgr24)
            {
                // tell the decoder that the 4th channel is an alpha channel
                tiff.SetField(TiffTag.EXTRASAMPLES, 1, new[] { (short)ExtraSample.UNASSALPHA });
            }

            var stride = width * args.Bpp;
            for (var y = 0; y < height; y++)
                tiff.WriteScanline(buffer, y * stride, y, 0);

            tiff.Flush();
        }
        #endregion
    }
    }
