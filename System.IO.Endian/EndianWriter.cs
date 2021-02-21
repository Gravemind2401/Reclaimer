using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    /// <summary>
    /// Writes primitive and complex data types to a stream in a specific byte order and encoding.
    /// </summary>
    public partial class EndianWriter : BinaryWriter
    {
        private readonly long virtualOrigin;
        private readonly Encoding encoding;

        public ByteOrder ByteOrder { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the system byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output)
            : this(output, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output, ByteOrder byteOrder)
            : this(output, byteOrder, new UTF8Encoding(), false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order using UTF-8 encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianWriter object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output, ByteOrder byteOrder, bool leaveOpen)
            : this(output, byteOrder, new UTF8Encoding(), leaveOpen)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output, ByteOrder byteOrder, Encoding encoding)
            : this(output, byteOrder, encoding, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianWriter object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output, ByteOrder byteOrder, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            virtualOrigin = 0;
            this.encoding = encoding;
            ByteOrder = byteOrder;
        }

        /// <summary>
        /// Creates a copy of <paramref name="parent"/> that will treat the specified origin as the the beginning of the stream.
        /// The resulting <seealso cref="EndianWriter"/> will not close the underlying stream when it is closed.
        /// </summary>
        /// <param name="parent">The <seealso cref="EndianWriter"/> instance to copy.</param>
        /// <param name="virtualOrigin">The position in the stream that will be treated as the beginning.</param>
        protected EndianWriter(EndianWriter parent, long virtualOrigin)
            : base(BaseStreamOrThrow(parent), EncodingOrThrow(parent), true)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            this.virtualOrigin = virtualOrigin;
            encoding = parent.encoding;
            ByteOrder = parent.ByteOrder;
        }

        private static Stream BaseStreamOrThrow(EndianWriter parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return parent.BaseStream;
        }

        private static Encoding EncodingOrThrow(EndianWriter parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            return parent.encoding;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte floating-point value to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(float value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte floating-point value to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(double value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a decimal value to the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(decimal value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The two-byte signed integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(short value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(int value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte signed integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(long value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public override void Write(ushort value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public override void Write(uint value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public override void Write(ulong value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a length-prefixed string to the current stream using the current byte order
        /// and encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override void Write(string value)
        {
            Write(value, ByteOrder);
        }

        /// <summary>
        /// Writes a globally unique identifier to the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(Guid value)
        {
            Write(value, ByteOrder);
        }

        #endregion

        #region ByteOrder Write

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte floating-point value to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(float value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte floating-point value to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(double value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a decimal value to the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(decimal value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bits = Decimal.GetBits(value);
            var bytes = new byte[16];

            for (int i = 0; i < 4; i++)
                Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * 4, 4);

            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The two-byte signed integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(short value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(int value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte signed integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(long value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public virtual void Write(ushort value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public virtual void Write(uint value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        [CLSCompliant(false)]
        public virtual void Write(ulong value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a globally unique identifier to the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(Guid value, ByteOrder byteOrder)
        {
            var bytes = value.ToByteArray();
            var a = BitConverter.ToInt32(bytes, 0);
            var b = BitConverter.ToInt16(bytes, 4);
            var c = BitConverter.ToInt16(bytes, 6);
            var d = bytes.Skip(8).ToArray();

            Write(a, byteOrder);
            Write(b, byteOrder);
            Write(c, byteOrder);
            Write(d);
        }

        #endregion

        #region String Write

        /// <summary>
        /// Writes a length-prefixed string to the current stream using the specified byte order
        /// and the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="byteOrder">The ByteOrder to write with.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void Write(string value, ByteOrder byteOrder)
        {
            Write(encoding.GetByteCount(value), byteOrder);
            Write(encoding.GetBytes(value));
        }

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void WriteStringFixedLength(string value)
        {
            WriteStringFixedLength(value, value.Length);
        }

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// If the string is shorter than the specified length it will be padded with white-space.
        /// If the string is longer than the specified length it will be truncated.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="length">The number of characters to write.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void WriteStringFixedLength(string value, int length)
        {
            WriteStringFixedLength(value, length, ' ');
        }

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// If the string is shorter than the specified length it will be padded using the specified character.
        /// If the string is longer than the specified length it will be truncated.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="length">The number of characters to write.</param>
        /// <param name="padding">The character to be used as padding.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void WriteStringFixedLength(string value, int length, char padding)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(nameof(length), length);

            if (length == 0) return;

            if (value.Length > length)
                value = value.Substring(0, length);
            else while (value.Length < length)
                    value += padding;

            base.Write(encoding.GetBytes(value));
        }

        /// <summary>
        /// Writes a null-terminated string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual void WriteStringNullTerminated(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Write(encoding.GetBytes(value + '\0'));
        }

        #endregion

        #region Other

        /// <summary>
        /// Sets the position of the underlying stream relative to a given origin.
        /// </summary>
        /// <param name="offset">A byte offest relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <exception cref="IOException" />
        /// <exception cref="NotSupportedException" />
        /// <exception cref="ObjectDisposedException" />
        public void Seek(long offset, SeekOrigin origin)
        {
            long address = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    address = virtualOrigin + offset;
                    break;
                case SeekOrigin.Current:
                    address = BaseStream.Position + offset;
                    break;
                case SeekOrigin.End:
                    address = BaseStream.Length + offset;
                    break;
            }

            SeekAbsolute(address);
        }

        private void SeekAbsolute(long address)
        {
            if (BaseStream.Position != address)
                BaseStream.Position = address;
        }

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream
        /// with the same byte order and encoding that will treat the current position
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        public virtual EndianWriter CreateVirtualWriter()
        {
            return CreateVirtualWriter(BaseStream.Position);
        }

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream 
        /// with the same byte order and encoding that will treat the specified offset
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        /// <param name="origin">The position in the stream that will be treated as the beginning.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public virtual EndianWriter CreateVirtualWriter(long origin)
        {
            //don't check stream bounds for writer - it can typically write beyond EOF
            return new EndianWriter(this, origin);
        }

        /// <summary>
        /// Calls <see cref="WriteObject{T}(T)"/> for each value in the set.
        /// </summary>
        /// <typeparam name="T">The type of object the set contains.</typeparam>
        /// <param name="values">The set of values.</param>
        public void WriteEnumerable<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            foreach (var value in values)
                WriteObject(value);
        }

        /// <summary>
        /// Calls <see cref="WriteObject{T}(T, double)"/> for each value in the set.
        /// </summary>
        /// <typeparam name="T">The type of object the set contains.</typeparam>
        /// <param name="values">The set of values.</param>
        /// <param name="version">The version of the type to write.</param>
        public void WriteEnumerable<T>(IEnumerable<T> values, double version)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            foreach (var value in values)
                WriteObject(value, version);
        }

        /// <summary>
        /// Inserts data at the current position. Any following data will be moved forward and the stream will be expanded.
        /// </summary>
        /// <param name="buffer">The data to insert.</param>
        public void Insert(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            else if (buffer.Length == 0)
                return;

            var source = BaseStream.Position;
            if (source < BaseStream.Length)
            {
                var destination = source + buffer.Length;
                Copy(source, destination, buffer.Length);
            }

            Write(buffer);
        }

        /// <summary>
        /// Inserts padding bytes at the current position. Any following data will be moved forward and the stream will be expanded.
        /// <para>Requires read access.</para>
        /// </summary>
        /// <param name="pad">The byte to pad with.</param>
        /// <param name="length">The number of bytes to insert.</param>
        public void Insert(byte pad, int length)
        {
            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(nameof(length), length);
            else if (length == 0)
                return;

            var source = BaseStream.Position;
            if (source < BaseStream.Length)
            {
                var destination = source + length;
                Copy(source, destination, (int)(BaseStream.Length - source));
            }

            Fill(pad, length);
        }

        /// <summary>
        /// Overwrites data with padding bytes.
        /// </summary>
        /// <param name="pad">The byte to pad with.</param>
        /// <param name="length">The number of bytes to insert.</param>
        public void Fill(byte pad, int length)
        {
            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(nameof(length), length);
            else if (length == 0)
                return;

            var buffer = new byte[length];

            if (pad != default(byte))
            {
                for (int i = 0; i < length; i++)
                    buffer[i] = pad;
            }

            Write(buffer);
        }

        /// <summary>
        /// Copies data from one part of the stream to another, overwriting data at the destination. The stream position is not advanced.
        /// <para>Requires read access.</para>
        /// </summary>
        /// <param name="sourceAddress">The address to start copying from.</param>
        /// <param name="destinationAddress">The address to copy to.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public void Copy(long sourceAddress, long destinationAddress, int length)
        {
            if (sourceAddress <= 0)
                throw Exceptions.ParamMustBePositive(nameof(sourceAddress), sourceAddress);

            if (destinationAddress <= 0)
                throw Exceptions.ParamMustBePositive(nameof(destinationAddress), destinationAddress);

            if (length <= 0)
                throw Exceptions.ParamMustBePositive(nameof(length), length);

            const int blockSize = 0x10000;
            var origin = BaseStream.Position;

            var buffer = new byte[blockSize];
            for (int remaining = length; remaining > 0;)
            {
                var readLength = Math.Min(blockSize, remaining);
                var offset = sourceAddress > destinationAddress
                    ? length - remaining
                    : remaining - readLength;

                Seek(sourceAddress + offset, SeekOrigin.Begin);
                BaseStream.Read(buffer, 0, readLength);

                Seek(destinationAddress + offset, SeekOrigin.Begin);
                BaseStream.Write(buffer, 0, readLength);

                remaining -= readLength;
            }

            Seek(origin, SeekOrigin.Begin);
        }

        #endregion
    }
}