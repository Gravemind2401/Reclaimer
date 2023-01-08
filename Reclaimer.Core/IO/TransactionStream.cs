using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    /// <summary>
    /// Provides a wrapper around an inner stream that allows changes to be written without altering the inner stream, akin to a transaction.
    /// The inner stream may be readonly, allowing you to treat it as a read-write stream temporarily.
    /// Changes can be applied to the inner stream or discarded at any time.
    /// </summary>
    public class TransactionStream : Stream
    {
        private readonly Stream source;
        private readonly Dictionary<long, byte[]> changes;

        private long length;
        private bool isOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStream"/> class based on the provided source stream.
        /// </summary>
        /// <param name="source">
        /// The underlying stream to read from and write to.
        /// </param>
        public TransactionStream(Stream source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (!source.CanRead)
                throw new ArgumentException("Source must be readable.");

            if (!source.CanSeek)
                throw new ArgumentException("Source must be seekable.");

            this.source = source;
            changes = new Dictionary<long, byte[]>();
            length = source.Length;
            isOpen = true;
        }

        private bool IsOpen
        {
            get
            {
                if (!source.CanRead)
                    Dispose();

                return isOpen;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => source.CanRead && IsOpen;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => source.CanSeek && IsOpen;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => IsOpen;

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public override long Length => length;

        /// <summary>
        /// Gets or sets the current position within the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (!IsOpen)
                    throw new ObjectDisposedException(null);

                return source.Position;
            }
            set
            {
                if (!IsOpen)
                    throw new ObjectDisposedException(null);

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero.");

                source.Position = value;
            }
        }

        /// <summary>
        /// Overrides the <see cref="Stream.Flush"/> method so that no action is performed.
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a buffer.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified
        /// byte array with the values between offset and (offset + count - 1) replaced by
        /// the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read
        /// from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <returns>
        /// The total number of bytes written into the buffer. This can be less than the
        /// number of bytes requested if that number of bytes are not currently available,
        /// or zero if the end of the stream is reached before any bytes are read.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new ObjectDisposedException(null);

            var start = Position;
            count = (int)Math.Min(count, Length - start);

            if (count <= 0) // in case the virtual length has been set to less than the source length
                return 0;

            source.ReadAll(buffer, offset, count); // read in the original data
            Position = start + count; // in case we are beyond the source length, source.Read will not advance the position by itself

            // apply any changes that have been made
            var overlaps = changes.Where(c => IsOverlapping(c.Key, c.Key + c.Value.Length, start, start + count));
            foreach (var patch in overlaps)
            {
                var sourceIndex = Math.Max(0, start - patch.Key);
                var destIndex = Math.Max(0, patch.Key - start);
                var length = Math.Min(buffer.Length - destIndex, patch.Value.Length - sourceIndex);

                Array.Copy(patch.Value, sourceIndex, buffer, offset + destIndex, length);
            }

            return count;
        }

        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">
        /// The new position within the stream. This is relative to the loc parameter, and
        /// can be positive or negative.
        /// </param>
        /// <param name="origin">
        /// A value of type which acts as the seek reference point.
        /// </param>
        /// <returns>
        /// The new position within the stream, calculated by combining the initial reference
        /// point and the offset.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new ObjectDisposedException(null);

            if (origin == SeekOrigin.Begin)
            {
                if (offset < 0)
                    throw new IOException("Attempted to seek before the beginning of the stream.");

                Position = offset;
            }
            else if (origin == SeekOrigin.End)
            {
                if (-offset > Length)
                    throw new IOException("Attempted to seek before the beginning of the stream.");

                Position = Length + offset;
            }
            else
            {
                if (-offset > Position)
                    throw new IOException("Attempted to seek before the beginning of the stream.");

                Position += offset;
            }

            return Position;
        }

        /// <summary>
        /// Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="value">
        /// The value at which to set the length.
        /// </param>
        public override void SetLength(long value)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (!IsOpen)
                throw new ObjectDisposedException(null);

            length = value;
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to write data from.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to write.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new ObjectDisposedException(null);

            byte[] patch;
            long patchOffset;
            long patchAddress;

            var overlaps = changes.Where(c => IsOverlapping(c.Key, c.Key + c.Value.Length, Position, Position + count));
            if (!overlaps.Any())
            {
                patch = new byte[count];
                patchOffset = 0;
                patchAddress = Position;
            }
            else
            {
                var ordered = overlaps.OrderBy(c => c.Key).ToList();

                var begin = Math.Min(ordered[0].Key, Position);
                var end = ordered[ordered.Count - 1].Key + ordered[ordered.Count - 1].Value.Length;
                end = Math.Max(end, Position + count);

                patch = new byte[end - begin];
                foreach (var p in ordered)
                {
                    Array.Copy(p.Value, 0, patch, p.Key - begin, p.Value.Length);
                    changes.Remove(p.Key);
                }

                patchOffset = Position - begin;
                patchAddress = begin;
            }

            Array.Copy(buffer, offset, patch, patchOffset, count);
            changes.Add(patchAddress, patch);
            Position += count;

            if (Position > length)
                length = Position;
        }

        /// <summary>
        /// Copies all pending changes to another <see cref="TransactionStream"/>, overwriting any conflicts.
        /// </summary>
        /// <param name="target">
        /// The <see cref="TransactionStream"/> to copy the changes to.
        /// </param>
        public void CopyChanges(TransactionStream target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (!target.IsOpen)
                throw new ArgumentException("Target stream is closed.");

            if (!target.changes.Any())
            {
                foreach (var patch in changes)
                    target.changes.Add(patch.Key, patch.Value);
            }
            else
            {
                var origin = target.Position;

                foreach (var patch in changes)
                {
                    target.Position = patch.Key;
                    target.Write(patch.Value, 0, patch.Value.Length);
                }

                target.Position = origin;
            }
        }

        /// <summary>
        /// Discard all pending changes and revert to the original data.
        /// </summary>
        public void DiscardChanges()
        {
            if (!IsOpen)
                throw new ObjectDisposedException(null);

            changes.Clear();
            length = source.Length;
            Position = 0;
        }

        /// <summary>
        /// Commit all pending changes to the underlying stream and clears the change list.
        /// </summary>
        public void ApplyChanges() => ApplyChanges(source);

        /// <summary>
        /// Commit all pending changes to the specified stream and clears the change list.
        /// </summary>
        public void ApplyChanges(Stream destination)
        {
            if (!IsOpen)
                throw new ObjectDisposedException(null);

            if (!destination.CanSeek)
                throw new ArgumentException("Destination is not seekable.");

            if (!destination.CanWrite)
                throw new ArgumentException("Destination is not writable.");

            var original = destination.Position;
            foreach (var patch in changes)
            {
                destination.Position = patch.Key;
                destination.Write(patch.Value, 0, patch.Value.Length);
            }

            destination.Position = original;
            changes.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Dispose();
                changes.Clear();
            }

            isOpen = false;
        }

        private static bool IsOverlapping(long a1, long a2, long b1, long b2)
        {
            // B starts inside A
            if (b1 >= a1 && b1 < a2)
                return true;

            // B ends inside A
            if (b2 > a1 && b2 < a2)
                return true;

            // B envelops A
            if (b1 < a1 && b2 >= a2)
                return true;

            return false;
        }
    }
}
