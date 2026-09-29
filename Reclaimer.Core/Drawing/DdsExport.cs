using StbImageWriteSharp;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Reclaimer.Drawing
{
    public partial class DdsImage
    {
        private const int CubemapFaceCount = 6;
        private const int CubemapTilesX = 4;
        private const int CubemapTilesY = 3;

        private byte[] GetExportBuffer(DdsOutputArgs args, out int exportWidth, out int exportHeight)
        {
            ArgumentNullException.ThrowIfNull(args);

            var virtualHeight = Height;

            var isCubeMap = TextureFlags.HasFlag(TextureFlags.DdsSurfaceFlagsCubemap) && CubemapFlags.HasFlag(CubemapFlags.DdsCubemapAllFaces);
            if (isCubeMap)
                virtualHeight *= CubemapFaceCount;

            //TODO: use custom array pool with max size of 4096*4096*4 (MaxArraysPerBucket = 8?)
            //pass rented arrays to decompression methods as output buffers (or output spans, even)
            var buffer = DecompressPixelData(args.Bgr24);

            var unwrapCubemap = isCubeMap && args.ValidCubeLayout && Width == Height;

            //MaskChannels() will alter the array that gets passed in, so make sure it doesnt modify the original dds pixels array
            //also prevents later channel swaps from affecting the original array.
            //unwrapping the cubemap already copies everything to a new array anyway so it does not need a clone
            if (ReferenceEquals(buffer, data) && !unwrapCubemap)
                buffer = (byte[])data.Clone();

            exportWidth = Width;
            exportHeight = virtualHeight;

            if (unwrapCubemap)
            {
                buffer = UnwrapCubemap(buffer, args.Bpp, args.Layout);
                exportWidth = Width * CubemapTilesX;
                exportHeight = Height * CubemapTilesY;
            }

            //mask after unwrapping so the cubemap output is affected too
            if (args.UseChannelMask)
                MaskChannels(buffer, args.Options);

            return buffer;
        }

        private byte[] UnwrapCubemap(byte[] source, int bpp, CubemapLayout layout)
        {
            var faceArray = new[] { layout.Face1, layout.Face2, layout.Face3, layout.Face4, layout.Face5, layout.Face6 };
            var rotateArray = new[] { layout.Orientation1, layout.Orientation2, layout.Orientation3, layout.Orientation4, layout.Orientation5, layout.Orientation6 };

            var faceStride = bpp * Width;
            var outputStride = faceStride * CubemapTilesX;
            var output = new byte[outputStride * Height * CubemapTilesY];

            for (var i = 0; i < CubemapFaceCount; i++)
            {
                if (faceArray[i] == CubemapFace.None)
                    continue;

                var (tileX, tileY) = GetCubemapTile(faceArray[i]);
                var sourceOffset = i * Height * faceStride;
                var outputOffset = tileY * Height * outputStride + tileX * faceStride;

                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        var (destX, destY) = GetOutputCoords(x, y, Width, rotateArray[i]);
                        var sourceIndex = sourceOffset + y * faceStride + x * bpp;
                        var outputIndex = outputOffset + destY * outputStride + destX * bpp;
                        source.AsSpan(sourceIndex, bpp).CopyTo(output.AsSpan(outputIndex, bpp));
                    }
                }
            }

            return output;

            static (int X, int Y) GetCubemapTile(CubemapFace face) => face switch
            {
                CubemapFace.Top => (1, 0),
                CubemapFace.Left => (0, 1),
                CubemapFace.Front => (1, 1),
                CubemapFace.Right => (2, 1),
                CubemapFace.Back => (3, 1),
                CubemapFace.Bottom => (1, 2),
                _ => throw new ArgumentOutOfRangeException(nameof(face))
            };

            static (int X, int Y) GetOutputCoords(int x, int y, int size, RotateFlipType orientation)
            {
                var value = (int)orientation;
                var last = size - 1;

                //each orientation is a clockwise rotation followed by a horizontal flip, so undo the flip first
                if (value > (int)RotateFlipType.Rotate270FlipNone)
                    x = last - x;

                //then undo the rotation by rotating clockwise for the remaining number of quarter turns
                var remainingTurns = (4 - value % 4) % 4;
                return remainingTurns switch
                {
                    1 => (last - y, x),
                    2 => (last - x, last - y),
                    3 => (y, last - x),
                    _ => (x, y)
                };
            }
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
