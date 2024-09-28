namespace Reclaimer.Drawing
{
    public class PixelFormatDescriptor
    {
        /// <summary>
        /// The number of bits required to store each pixel in the image.
        /// </summary>
        public int BitsPerPixel { get; init; }

        /// <summary>
        /// The number of bytes used to store each read/write unit.
        /// </summary>
        /// <remarks>
        /// This is used for endian swaps to convert the data between little endian and big endian.
        /// </remarks>
        public int ReadUnitSize { get; init; }

        /// <summary>
        /// The width and height of each compressed block in pixels.
        /// </summary>
        /// <remarks>
        /// For formats that don't use blocks, this will just be 1.
        /// </remarks>
        public int BlockWidth { get; init; }

        /// <summary>
        /// The number of bytes used to store each compressed block.
        /// </summary>
        /// <remarks>
        /// For formats that don't use blocks, this will just be the read unit size.
        /// </remarks>
        public int BytesPerBlock { get; init; }

        /// <summary>
        /// The number of pixels that the width and hight of the image must align to on Xbox 360.
        /// <br/> Images with dimensions that are not multiples of the alignment are padded with empty pixels to meet the required alignment.
        /// </summary>
        public int PaddingAlignment { get; init; }

        /// <summary>
        /// Rounds up to the nearest valid padded dimensions, accounting for the block width and padding alignment.
        /// </summary>
        /// <param name="width">The unpadded width of the image.</param>
        /// <param name="height">The unpadded height of the image.</param>
        public (int PaddedWidth, int PaddedHeight) GetPaddedSize(int width, int height)
        {
            var blockSize = (double)BlockWidth;
            var tileSize = (double)PaddingAlignment;

            var virtualWidth = (int)(Math.Ceiling(width / tileSize) * tileSize);
            var virtualHeight = (int)(Math.Ceiling(height / tileSize) * tileSize);

            virtualWidth = (int)(Math.Ceiling(virtualWidth / blockSize) * blockSize);
            virtualHeight = (int)(Math.Ceiling(virtualHeight / blockSize) * blockSize);

            return (virtualWidth, virtualHeight);
        }
    }
}
