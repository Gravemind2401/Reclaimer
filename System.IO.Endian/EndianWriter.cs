using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// Writes primitive data types to a stream in a specific byte order and encoding.
    /// </summary>
    public class EndianWriter : BinaryWriter
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
            : this(output, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false, 0)
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
            : this(output, byteOrder, new UTF8Encoding(), false, 0)
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
            : this(output, byteOrder, encoding, false, 0)
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
            : this(output, byteOrder, encoding, leaveOpen, 0)
        {

        }

        private EndianWriter(Stream input, ByteOrder byteOrder, Encoding encoding, bool leaveOpen, long virtualOrigin) 
            : base(input, encoding, leaveOpen)
        {
            ByteOrder = byteOrder;
            this.virtualOrigin = virtualOrigin;
            this.encoding = encoding;
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
        public override void Write(ulong value)
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
        public void Write(float value, ByteOrder byteOrder)
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
        public void Write(double value, ByteOrder byteOrder)
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
        public void Write(decimal value, ByteOrder byteOrder)
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
        public void Write(short value, ByteOrder byteOrder)
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
        public void Write(int value, ByteOrder byteOrder)
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
        public void Write(long value, ByteOrder byteOrder)
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
        public void Write(ushort value, ByteOrder byteOrder)
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
        public void Write(uint value, ByteOrder byteOrder)
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
        public void Write(ulong value, ByteOrder byteOrder)
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

        #endregion

        #region String Write

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// If the string is shorter than the specified length it will be padded with white-space.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="length">The number of characters to write.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public void Write(string value, int length)
        {
            Write(value, length, ' ');
        }

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// If the string is shorter than the specified length it will be padded using the specified character.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="length">The number of characters to write.</param>
        /// <param name="padding">The character to be used as padding.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public void Write(string value, int length, char padding)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "The length parameter must be non-negative.");

            if (length == 0) return;

            if (value.Length > length)
                value = value.Substring(0, length);
            else while (value.Length < length)
                    value += padding;

            base.Write(encoding.GetBytes(value));
        }

        /// <summary>
        /// Writes a fixed-length or null-terminated string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="isNullTerminated">true to terminate the string with a null character. false to write a length-prefixed string.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public void Write(string value, bool isNullTerminated)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!isNullTerminated)
                base.Write(value);

            base.Write(encoding.GetBytes(value + '\0'));
        }

        #endregion

        #region Other

        /// <summary>
        /// Sets the position of the underlying stream relative to a given origin.
        /// </summary>
        /// <param name="offset">A byte offest relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        public void Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    BaseStream.Position = virtualOrigin + offset;
                    break;
                case SeekOrigin.Current:
                    BaseStream.Position += offset;
                    break;
                case SeekOrigin.End:
                    BaseStream.Position = BaseStream.Length + offset;
                    break;
            }
        }

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream
        /// with the same byte order and encoding that will treat the current position
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        public EndianWriter CreateVirtualWriter()
        {
            return CreateVirtualWriter(BaseStream.Position);
        }

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream 
        /// with the same byte order and encoding that will treat the specified offset
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        /// <param name="origin">The position in the stream that will be treated as the beginning.</param>
        public EndianWriter CreateVirtualWriter(long origin)
        {
            return new EndianWriter(BaseStream, ByteOrder, encoding, true, origin);
        }

        #endregion
    }
}
