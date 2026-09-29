using System.Text;

namespace Reclaimer.Drawing
{
    /// <summary>
    /// Minimal writer for uncompressed true-colour TGA files. Pixel data is written as-is, so no channel swapping is needed.
    /// </summary>
    /// <remarks>
    /// <see href="https://en.wikipedia.org/wiki/Truevision_TGA" />
    /// <see href="https://paulbourke.net/dataformats/tga/" />
    /// </remarks>
    public static class TgaWriter
    {
        private const int BitsPerByte = 8;
        private const byte NoImageId = 0;
        private const byte AlphaBits = 8;
        private const byte TopToBottom = 0x20; //image descriptor bit 5 - rows are stored top-down
        private const ushort Unused = 0;

        private enum ColourMapType : byte
        {
            None = 0
        }

        private enum ImageType : byte
        {
            UncompressedTrueColour = 2
        }

        /// <summary>
        /// Writes a TGA image to a stream.
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
            ArgumentOutOfRangeException.ThrowIfGreaterThan(width, ushort.MaxValue);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(height, ushort.MaxValue);

            var bytesPerPixel = hasAlpha ? 4 : 3;
            var dataSize = (long)width * height * bytesPerPixel;
            if (pixels.Length < dataSize)
                throw new ArgumentException("The pixel data is too small for the specified dimensions.", nameof(pixels));

            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            writer.Write(NoImageId);
            writer.Write((byte)ColourMapType.None);
            writer.Write((byte)ImageType.UncompressedTrueColour);
            writer.Write(Unused); //colour map first entry index
            writer.Write(Unused); //colour map length
            writer.Write((byte)Unused); //colour map entry size
            writer.Write(Unused); //x origin
            writer.Write(Unused); //y origin
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            writer.Write((byte)(bytesPerPixel * BitsPerByte));
            writer.Write((byte)((hasAlpha ? AlphaBits : 0) | TopToBottom));

            writer.Write(pixels[..(int)dataSize]);
        }
    }
}
