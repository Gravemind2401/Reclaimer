using System.Text;

namespace Reclaimer.Drawing
{
    /// <summary>
    /// A writer for uncompressed, single-strip, 8-bit RGB(A) TIFF files.
    /// </summary>
    /// <remarks>
    /// <see href="https://en.wikipedia.org/wiki/TIFF" />
    /// <see href="https://download.osgeo.org/libtiff/doc/TIFF6.pdf" />
    /// </remarks>
    public static class TiffWriter
    {
        private const ushort ByteOrderLittleEndian = 0x4949; //"II"
        private const ushort MagicNumber = 42;
        private const int HeaderSize = 8;
        private const int IfdCountSize = 2;
        private const int IfdEntrySize = 12;
        private const int IfdNextOffsetSize = 4;
        private const uint FirstIfdOffset = HeaderSize;
        private const uint NoNextIfd = 0;
        private const ushort BitsPerChannel = 8;

        private enum Tag : ushort
        {
            ImageWidth = 256,
            ImageLength = 257,
            BitsPerSample = 258,
            Compression = 259,
            Photometric = 262,
            StripOffsets = 273,
            Orientation = 274,
            SamplesPerPixel = 277,
            RowsPerStrip = 278,
            StripByteCounts = 279,
            PlanarConfig = 284,
            ExtraSamples = 338
        }

        private enum FieldType : ushort
        {
            Short = 3,
            Long = 4
        }

        private enum Compression : ushort
        {
            None = 1
        }

        private enum Photometric : ushort
        {
            Rgb = 2
        }

        private enum Orientation : ushort
        {
            TopLeft = 1
        }

        private enum PlanarConfig : ushort
        {
            Chunky = 1
        }

        private enum ExtraSample : ushort
        {
            UnassociatedAlpha = 2
        }

        /// <summary>
        /// Writes a TIFF image to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="pixels">Top-down RGB or RGBA pixel data with no row padding.</param>
        /// <param name="width">The image width in pixels.</param>
        /// <param name="height">The image height in pixels.</param>
        /// <param name="hasAlpha"><see langword="true" /> if <paramref name="pixels"/> is RGBA, <see langword="false" /> if it is RGB.</param>
        /// <remarks>
        /// This expects <b>RGB(A)</b> instead of BGR(A)!
        /// </remarks>
        public static void Write(Stream stream, Span<byte> pixels, int width, int height, bool hasAlpha)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            var samplesPerPixel = hasAlpha ? 4 : 3;
            var rowSize = width * samplesPerPixel;
            if (pixels.Length < (long)rowSize * height)
                throw new ArgumentException("The pixel data is too small for the specified dimensions.", nameof(pixels));

            var entryCount = hasAlpha ? 12 : 11; //the extra sample entry is only needed with alpha
            var ifdSize = IfdCountSize + entryCount * IfdEntrySize + IfdNextOffsetSize;
            var bitsPerSampleOffset = HeaderSize + ifdSize;
            var dataOffset = bitsPerSampleOffset + samplesPerPixel * sizeof(ushort);
            var dataSize = (long)rowSize * height;

            if (dataOffset + dataSize > uint.MaxValue)
                throw new NotSupportedException("The image is too large for the TIFF format.");

            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            //header
            writer.Write(ByteOrderLittleEndian);
            writer.Write(MagicNumber);
            writer.Write(FirstIfdOffset);

            //image file directory - entries must be in ascending tag order
            writer.Write((ushort)entryCount);
            WriteEntry(writer, Tag.ImageWidth, FieldType.Long, 1, (uint)width);
            WriteEntry(writer, Tag.ImageLength, FieldType.Long, 1, (uint)height);
            WriteEntry(writer, Tag.BitsPerSample, FieldType.Short, (uint)samplesPerPixel, (uint)bitsPerSampleOffset);
            WriteEntry(writer, Tag.Compression, FieldType.Short, 1, (uint)Compression.None);
            WriteEntry(writer, Tag.Photometric, FieldType.Short, 1, (uint)Photometric.Rgb);
            WriteEntry(writer, Tag.StripOffsets, FieldType.Long, 1, (uint)dataOffset);
            WriteEntry(writer, Tag.Orientation, FieldType.Short, 1, (uint)Orientation.TopLeft);
            WriteEntry(writer, Tag.SamplesPerPixel, FieldType.Short, 1, (uint)samplesPerPixel);
            WriteEntry(writer, Tag.RowsPerStrip, FieldType.Long, 1, (uint)height); //whole image is one strip
            WriteEntry(writer, Tag.StripByteCounts, FieldType.Long, 1, (uint)dataSize);
            WriteEntry(writer, Tag.PlanarConfig, FieldType.Short, 1, (uint)PlanarConfig.Chunky);
            if (hasAlpha)
                WriteEntry(writer, Tag.ExtraSamples, FieldType.Short, 1, (uint)ExtraSample.UnassociatedAlpha);
            writer.Write(NoNextIfd);

            //values too large to fit inside an entry
            for (var i = 0; i < samplesPerPixel; i++)
                writer.Write(BitsPerChannel);

            //pixel data
            for (var y = 0; y < height; y++)
                writer.Write(pixels.Slice(y * rowSize, rowSize));

            static void WriteEntry(BinaryWriter writer, Tag tag, FieldType type, uint count, uint value)
            {
                writer.Write((ushort)tag);
                writer.Write((ushort)type);
                writer.Write(count);
                writer.Write(value);
            }
        }
    }
}
