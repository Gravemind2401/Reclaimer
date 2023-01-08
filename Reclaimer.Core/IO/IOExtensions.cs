using System;
using System.IO;

namespace Reclaimer.IO
{
    public static class IOExtensions
    {
        /// <summary>
        /// This is the same as <see cref="Stream.Read(byte[], int, int)"/> except it is guaranteed not read less than the specified
        /// number of bytes unless the end of stream has been reached.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <remarks>
        /// <see cref="Stream"/> implementations are not required to read all bytes in one operation. This method is a wrapper
        /// that continues reading until either all bytes have been read or the end of stream has been reached.
        /// </remarks>
        /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
        public static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
        {
            var totalBytes = 0;

            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, offset, count);
                totalBytes += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }
            while (bytesRead > 0 && count > 0);

            return totalBytes;
        }

        /// <summary>
        /// This is the same as <see cref="Stream.Read(Span{byte})"/> except it is guaranteed not read less than the specified
        /// number of bytes unless the end of stream has been reached.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <remarks>
        /// <see cref="Stream"/> implementations are not required to read all bytes in one operation. This method is a wrapper
        /// that continues reading until either all bytes have been read or the end of stream has been reached.
        /// </remarks>
        /// <inheritdoc cref="Stream.Read(Span{byte})"/>
        public static int ReadAll(this Stream stream, Span<byte> buffer)
        {
            var totalBytes = 0;

            int bytesRead;
            while (buffer.Length > 0 && (bytesRead = stream.Read(buffer)) > 0)
            {
                totalBytes += bytesRead;
                buffer = buffer[bytesRead..];
            }

            return totalBytes;
        }
    }
}
