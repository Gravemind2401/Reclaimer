using System.Text;

namespace Reclaimer.Drawing
{
    /// <summary>
    /// Minimal writer for uncompressed BMP files. Pixel data is written as-is, so no channel swapping is needed.
    /// </summary>
    /// <remarks>
    /// <see href="https://en.wikipedia.org/wiki/BMP_file_format" />
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader" />
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapv4header" />
    /// </remarks>
    public static class BmpWriter
    {
        private const ushort Signature = 0x4D42; //"BM"
        private const int FileHeaderSize = 14;
        private const int InfoHeaderSize = 40;
        private const int V4HeaderSize = 108;
        private const int V4EndpointsAndGammaSize = 48; //CIE endpoints (36 bytes) and gamma (12 bytes), unused
        private const int RowAlignment = 4;
        private const int BitsPerByte = 8;
        private const ushort ColourPlanes = 1;
        private const uint ColourSpaceWindows = 0x57696E20; //"Win "
        private const uint RedMask = 0x00FF0000;
        private const uint GreenMask = 0x0000FF00;
        private const uint BlueMask = 0x000000FF;
        private const uint AlphaMask = 0xFF000000;
        private const uint Unused = 0;

        private enum Compression : uint
        {
            Rgb = 0,
            Bitfields = 3
        }

        /// <summary>
        /// Writes a BMP image to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="pixels">Top-down BGR or BGRA pixel data with no row padding.</param>
        /// <param name="width">The image width in pixels.</param>
        /// <param name="height">The image height in pixels.</param>
        /// <param name="hasAlpha"><see langword="true" /> if <paramref name="pixels"/> is BGRA, <see langword="false" /> if it is BGR.</param>
        public static void Write(Stream stream, ReadOnlySpan<byte> pixels, int width, int height, bool hasAlpha)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            var bytesPerPixel = hasAlpha ? 4 : 3;
            var rowSize = width * bytesPerPixel;
            if (pixels.Length < (long)rowSize * height)
                throw new ArgumentException("The pixel data is too small for the specified dimensions.", nameof(pixels));

            var paddedRowSize = (rowSize + RowAlignment - 1) / RowAlignment * RowAlignment;
            var padding = paddedRowSize - rowSize;
            var headerSize = hasAlpha ? V4HeaderSize : InfoHeaderSize;
            var dataOffset = FileHeaderSize + headerSize;
            var dataSize = (long)paddedRowSize * height;

            if (dataOffset + dataSize > uint.MaxValue)
                throw new NotSupportedException("The image is too large for the BMP format.");

            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            //file header
            writer.Write(Signature);
            writer.Write((uint)(dataOffset + dataSize));
            writer.Write(Unused); //reserved
            writer.Write((uint)dataOffset);

            //info header
            writer.Write((uint)headerSize);
            writer.Write(width);
            writer.Write(-height); //negative height means top-down rows
            writer.Write(ColourPlanes);
            writer.Write((ushort)(bytesPerPixel * BitsPerByte));
            writer.Write((uint)(hasAlpha ? Compression.Bitfields : Compression.Rgb));
            writer.Write((uint)dataSize);
            writer.Write(Unused); //horizontal resolution
            writer.Write(Unused); //vertical resolution
            writer.Write(Unused); //palette colours
            writer.Write(Unused); //important colours

            //BGRA needs the V4 header extension so the alpha channel is explicitly declared
            if (hasAlpha)
            {
                writer.Write(RedMask);
                writer.Write(GreenMask);
                writer.Write(BlueMask);
                writer.Write(AlphaMask);
                writer.Write(ColourSpaceWindows);
                writer.Write(new byte[V4EndpointsAndGammaSize]);
            }

            if (padding == 0)
            {
                writer.Write(pixels[..(rowSize * height)]);
                return;
            }

            Span<byte> padBytes = stackalloc byte[RowAlignment];
            for (var y = 0; y < height; y++)
            {
                writer.Write(pixels.Slice(y * rowSize, rowSize));
                writer.Write(padBytes[..padding]);
            }
        }
    }
}
