using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public class EndianWriter : BinaryWriter
    {
        public ByteOrder ByteOrder { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the system byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianWriter(Stream output) : this(output, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false)
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
        public EndianWriter(Stream output, ByteOrder byteOrder) : this(output, byteOrder, new UTF8Encoding(), false)
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
        public EndianWriter(Stream output, ByteOrder byteOrder, Encoding encoding) : this(output, byteOrder, encoding, false)
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
        public EndianWriter(Stream output, ByteOrder byteOrder, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
            ByteOrder = byteOrder;
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
    }
}
