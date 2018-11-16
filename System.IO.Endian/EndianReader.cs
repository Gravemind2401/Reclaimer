using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    /// <summary>
    /// Reads primitive data types from a stream in a specific byte order and encoding.
    /// </summary>
    public partial class EndianReader : BinaryReader
    {
        private readonly long virtualOrigin;
        private readonly Encoding encoding;

        public ByteOrder ByteOrder { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the system byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianReader(Stream input)
            : this(input, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianReader(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, new UTF8Encoding(), false, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianReader(Stream input, ByteOrder byteOrder, Encoding encoding)
            : this(input, byteOrder, encoding, false, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianReader object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public EndianReader(Stream input, ByteOrder byteOrder, Encoding encoding, bool leaveOpen)
            : this(input, byteOrder, encoding, leaveOpen, 0)
        {

        }

        private EndianReader(Stream input, ByteOrder byteOrder, Encoding encoding, bool leaveOpen, long virtualOrigin)
            : base(input, encoding, leaveOpen)
        {
            ByteOrder = byteOrder;
            this.virtualOrigin = virtualOrigin;
            this.encoding = encoding;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the current byte order 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override float ReadSingle()
        {
            return ReadSingle(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the current byte order 
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override double ReadDouble()
        {
            return ReadDouble(ByteOrder);
        }

        /// <summary>
        /// Reads a decimal value from the current stream using the current byte order 
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override decimal ReadDecimal()
        {
            return ReadDecimal(ByteOrder);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the current byte order 
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override short ReadInt16()
        {
            return ReadInt16(ByteOrder);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the current byte order 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override int ReadInt32()
        {
            return ReadInt32(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the current byte order 
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override long ReadInt64()
        {
            return ReadInt64(ByteOrder);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the current byte order 
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override ushort ReadUInt16()
        {
            return ReadUInt16(ByteOrder);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override uint ReadUInt32()
        {
            return ReadUInt32(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override ulong ReadUInt64()
        {
            return ReadUInt64(ByteOrder);
        }

        /// <summary>
        /// Reads a length-prefixed string from the current stream using the current byte order
        /// and encoding of the <seealso cref="EndianReader"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public override string ReadString()
        {
            return ReadString(ByteOrder);
        }

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual Guid ReadGuid()
        {
            return ReadGuid(ByteOrder);
        }

        #endregion

        #region ByteOrder Read

        /// <summary>
        /// Reads a 4-byte floating-point value from the current stream using the specified byte order 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual float ReadSingle(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadSingle();

            var bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reads an 8-byte floating-point value from the current stream using the specified byte order 
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual double ReadDouble(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadDouble();

            var bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Reads a decimal value from the current stream using the specified byte order 
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual decimal ReadDecimal(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadDecimal();

            var bits = new int[4];
            var bytes = base.ReadBytes(16);
            Array.Reverse(bytes);
            for (int i = 0; i < 4; i++)
                bits[i] = BitConverter.ToInt32(bytes, i * 4);
            return new decimal(bits);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual short ReadInt16(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadInt16();

            var bytes = base.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual int ReadInt32(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadInt32();

            var bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual long ReadInt64(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadInt64();

            var bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ushort ReadUInt16(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt16();

            var bytes = base.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual uint ReadUInt32(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt32();

            var bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the specified byte order 
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ulong ReadUInt64(ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt64();

            var bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual Guid ReadGuid(ByteOrder byteOrder)
        {
            var a = ReadInt32(byteOrder);
            var b = ReadInt16(byteOrder);
            var c = ReadInt16(byteOrder);
            var d = ReadBytes(8);

            return new Guid(a, b, c, d);
        }

        #endregion

        #region String Read

        /// <summary>
        /// Reads a length-prefixed string from the current stream using the specified byte order
        /// and the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual string ReadString(ByteOrder byteOrder)
        {
            var length = ReadInt32(byteOrder);

            if (length == 0)
                return string.Empty;

            return encoding.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Reads a fixed-length string from the current stream, and optionally removes trailing white-space characters.
        /// </summary>
        /// <param name="length">The length of the string, in bytes.</param>
        /// <param name="trim">true to remove trailing white-space characters; otherwise, false.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual string ReadString(int length, bool trim)
        {
            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(nameof(length), length);

            if (length == 0)
                return string.Empty;

            if (trim) return encoding.GetString(ReadBytes(length)).TrimEnd();
            else return encoding.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Reads a variable-length string from the current stream. 
        /// The position of the stream is advanced to the position after the next occurence of a null character.
        /// </summary>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual string ReadNullTerminatedString()
        {
            var bytes = new List<byte>();

            byte val;
            while ((val = ReadByte()) != 0)
                bytes.Add(val);

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a variable-length string from the current stream. 
        /// The length of the string is determined by the first occurence of a null character.
        /// <para /> The position of the stream is advanced by the specified number of bytes, regardless of the resulting string length.
        /// </summary>
        /// <param name="maxLength">The maximum length of the string, in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual string ReadNullTerminatedString(int maxLength)
        {
            if (maxLength < 0)
                throw Exceptions.ParamMustBeNonNegative(nameof(maxLength), maxLength);

            if (maxLength == 0)
                return string.Empty;

            var value = encoding.GetString(base.ReadBytes(maxLength));

            if (!value.Contains('\0'))
                return value;

            return value.Substring(0, value.IndexOf('\0'));
        }

        #endregion

        #region Peek

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual float PeekSingle()
        {
            return PeekSingle(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual double PeekDouble()
        {
            return PeekDouble(ByteOrder);
        }

        /// <summary>
        /// Reads a decimal value from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual decimal PeekDecimal()
        {
            return PeekDecimal(ByteOrder);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual short PeekInt16()
        {
            return PeekInt16(ByteOrder);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual int PeekInt32()
        {
            return PeekInt32(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual long PeekInt64()
        {
            return PeekInt64(ByteOrder);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the current byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ushort PeekUInt16()
        {
            return PeekUInt16(ByteOrder);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual uint PeekUInt32()
        {
            return PeekUInt32(ByteOrder);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ulong PeekUInt64()
        {
            return PeekUInt64(ByteOrder);
        }

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual Guid PeekGuid()
        {
            return PeekGuid(ByteOrder);
        }

        #endregion

        #region ByteOrder Peek

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual float PeekSingle(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadSingle(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual double PeekDouble(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadDouble(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a decimal value from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual decimal PeekDecimal(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadDecimal(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual short PeekInt16(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadInt16(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual int PeekInt32(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadInt32(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual long PeekInt64(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadInt64(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the specified byte order 
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ushort PeekUInt16(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadUInt16(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual uint PeekUInt32(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadUInt32(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual ulong PeekUInt64(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadUInt64(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <exception cref="EndOfStreamException" />
        /// <exception cref="IOException" />
        /// <exception cref="ObjectDisposedException" />
        public virtual Guid PeekGuid(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadGuid(byteOrder);
            BaseStream.Position = origin;
            return value;
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
        /// Creates an <seealso cref="EndianReader"/> based on the same stream
        /// with the same byte order and encoding that will treat the current position
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        public EndianReader CreateVirtualReader()
        {
            return CreateVirtualReader(BaseStream.Position);
        }

        /// <summary>
        /// Creates an <seealso cref="EndianReader"/> based on the same stream 
        /// with the same byte order and encoding that will treat the specified offset
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        /// <param name="origin">The position in the stream that will be treated as the beginning.</param>
        public EndianReader CreateVirtualReader(long origin)
        {
            return new EndianReader(BaseStream, ByteOrder, encoding, true, origin);
        }

        #endregion
    }
}
