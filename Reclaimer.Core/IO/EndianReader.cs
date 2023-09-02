using Reclaimer.IO.Dynamic;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Reclaimer.IO
{
    /// <summary>
    /// Reads primitive and dynamic data types from a stream in a specific byte order and encoding.
    /// </summary>
    public class EndianReader : BinaryReader, IEndianStream
    {
        private static readonly ByteOrder NativeByteOrder = BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

        private readonly long virtualOrigin;
        private readonly Encoding encoding;

        /// <summary>
        /// Returns <see langword="true"/> if the current value of the <see cref="ByteOrder"/> property
        /// matches the byte order of the system's architecture.
        /// </summary>
        protected bool IsNativeByteOrder => ByteOrder == NativeByteOrder;

        /// <summary>
        /// Gets or sets the endianness used when reading from the stream.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the system byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianReader(Stream input)
            : this(input, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianReader(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, new UTF8Encoding(), false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order using UTF-8 encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianReader object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianReader(Stream input, ByteOrder byteOrder, bool leaveOpen)
            : this(input, byteOrder, new UTF8Encoding(), leaveOpen)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianReader(Stream input, ByteOrder byteOrder, Encoding encoding)
            : this(input, byteOrder, encoding, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianReader"/> class
        /// based on the specified stream with the specified byte order and character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianReader object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianReader(Stream input, ByteOrder byteOrder, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
            virtualOrigin = 0;
            this.encoding = encoding;
            ByteOrder = byteOrder;
        }

        /// <summary>
        /// Creates a copy of <paramref name="parent"/> that will treat the specified origin as the the beginning of the stream.
        /// The resulting <seealso cref="EndianReader"/> will not close the underlying stream when it is closed.
        /// </summary>
        /// <param name="parent">The <seealso cref="EndianReader"/> instance to copy.</param>
        /// <param name="virtualOrigin">The position in the stream that will be treated as the beginning.</param>
        protected EndianReader(EndianReader parent, long virtualOrigin)
            : base(BaseStreamOrThrow(parent), EncodingOrThrow(parent), true)
        {
            this.virtualOrigin = virtualOrigin;
            encoding = parent.encoding;
            ByteOrder = parent.ByteOrder;
        }

        private static Stream BaseStreamOrThrow(EndianReader parent) => parent?.BaseStream ?? throw new ArgumentNullException(nameof(parent));

        private static Encoding EncodingOrThrow(EndianReader parent) => parent?.encoding ?? throw new ArgumentNullException(nameof(parent));

        #endregion

        #region Overrides

        /// <summary>
        /// Reads a 2-byte floating point value from the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="ReadHalf(ByteOrder)"/>
        public override Half ReadHalf() => ReadHalf(ByteOrder);

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="ReadSingle(ByteOrder)"/>
        public override float ReadSingle() => ReadSingle(ByteOrder);

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="ReadDouble(ByteOrder)"/>
        public override double ReadDouble() => ReadDouble(ByteOrder);

        /// <summary>
        /// Reads a decimal value from the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="ReadDecimal(ByteOrder)"/>
        public override decimal ReadDecimal() => ReadDecimal(ByteOrder);

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="ReadInt16(ByteOrder)"/>
        public override short ReadInt16() => ReadInt16(ByteOrder);

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public override int ReadInt32() => ReadInt32(ByteOrder);

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="ReadInt64(ByteOrder)"/>
        public override long ReadInt64() => ReadInt64(ByteOrder);

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="ReadUInt16(ByteOrder)"/>
        public override ushort ReadUInt16() => ReadUInt16(ByteOrder);

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="ReadUInt32(ByteOrder)"/>
        public override uint ReadUInt32() => ReadUInt32(ByteOrder);

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="ReadUInt64(ByteOrder)"/>
        public override ulong ReadUInt64() => ReadUInt64(ByteOrder);

        /// <summary>
        /// Reads a length-prefixed string from the current stream using the current byte order
        /// and encoding of the <seealso cref="EndianReader"/>.
        /// </summary>
        /// <inheritdoc cref="ReadString(ByteOrder)"/>
        public override string ReadString() => ReadString(ByteOrder);

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="ReadGuid(ByteOrder)"/>
        public virtual Guid ReadGuid() => ReadGuid(ByteOrder);

        #endregion

        #region ByteOrder Read

        /// <summary>
        /// Reads a 2-byte floating-point value from the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadHalf"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual Half ReadHalf(ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
                return base.ReadHalf();

            var bytes = base.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToHalf(bytes, 0);
        }

        /// <summary>
        /// Reads a 4-byte floating-point value from the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadSingle"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual float ReadSingle(ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
                return base.ReadSingle();

            var bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reads an 8-byte floating-point value from the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadDouble"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual double ReadDouble(ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
                return base.ReadDouble();

            var bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Reads a decimal value from the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadDecimal"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual decimal ReadDecimal(ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
                return base.ReadDecimal();

            var bits = new int[4];
            var bytes = base.ReadBytes(16);
            Array.Reverse(bytes);
            for (var i = 0; i < 4; i++)
                bits[i] = BitConverter.ToInt32(bytes, i * 4);

            return new decimal(bits);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadInt16"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual short ReadInt16(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadInt16() : BinaryPrimitives.ReverseEndianness(base.ReadInt16());

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <inheritdoc cref="BinaryReader.ReadInt32"/>
        public virtual int ReadInt32(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadInt32() : BinaryPrimitives.ReverseEndianness(base.ReadInt32());

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadInt64"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual long ReadInt64(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadInt64() : BinaryPrimitives.ReverseEndianness(base.ReadInt64());

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadUInt16"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual ushort ReadUInt16(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadUInt16() : BinaryPrimitives.ReverseEndianness(base.ReadUInt16());

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadUInt32"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual uint ReadUInt32(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadUInt32() : BinaryPrimitives.ReverseEndianness(base.ReadUInt32());

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryReader.ReadUInt64"/>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
        public virtual ulong ReadUInt64(ByteOrder byteOrder) => byteOrder == NativeByteOrder ? base.ReadUInt64() : BinaryPrimitives.ReverseEndianness(base.ReadUInt64());

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
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
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual string ReadString(ByteOrder byteOrder)
        {
            var length = ReadInt32(byteOrder);

            return length > 0
                ? encoding.GetString(ReadBytes(length))
                : string.Empty;
        }

        /// <summary>
        /// Reads a fixed-length string from the current stream, and optionally removes trailing white-space characters.
        /// </summary>
        /// <param name="length">The length of the string, in bytes.</param>
        /// <param name="trim">true to remove trailing white-space characters; otherwise, false.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual string ReadString(int length, bool trim)
        {
            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(length);

            if (length == 0)
                return string.Empty;

            var result = encoding.GetString(ReadBytes(length));
            return trim ? result.TrimEnd() : result;
        }

        /// <summary>
        /// Reads a variable-length string from the current stream.
        /// The position of the stream is advanced to the position after the next occurence of a null character.
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual string ReadNullTerminatedString()
        {
            var bytes = new List<byte>();

            byte val;
            while (BaseStream.Position < BaseStream.Length && (val = ReadByte()) != 0)
                bytes.Add(val);

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a variable-length string from the current stream.
        /// The length of the string is determined by the first occurence of a null character.
        /// <para/> The position of the stream is advanced by the specified number of bytes, regardless of the resulting string length.
        /// </summary>
        /// <param name="maxLength">The maximum length of the string, in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual string ReadNullTerminatedString(int maxLength)
        {
            if (maxLength < 0)
                throw Exceptions.ParamMustBeNonNegative(maxLength);

            if (maxLength == 0)
                return string.Empty;

            var value = encoding.GetString(base.ReadBytes(maxLength));
            var nullIndex = value.IndexOf('\0');
            return nullIndex >= 0 ? value[..nullIndex] : value;
        }

        #endregion

        #region Peek

        /// <summary>
        /// Reads a 2-byte floating point value from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekHalf(ByteOrder)"/>
        public virtual Half PeekHalf() => PeekHalf(ByteOrder);

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekSingle(ByteOrder)"/>
        public virtual float PeekSingle() => PeekSingle(ByteOrder);

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekDouble(ByteOrder)"/>
        public virtual double PeekDouble() => PeekDouble(ByteOrder);

        /// <summary>
        /// Reads a decimal value from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekDecimal(ByteOrder)"/>
        public virtual decimal PeekDecimal() => PeekDecimal(ByteOrder);

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekInt16(ByteOrder)"/>
        public virtual short PeekInt16() => PeekInt16(ByteOrder);

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekInt32(ByteOrder)"/>
        public virtual int PeekInt32() => PeekInt32(ByteOrder);

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekInt64(ByteOrder)"/>
        public virtual long PeekInt64() => PeekInt64(ByteOrder);

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekUInt16(ByteOrder)"/>
        public virtual ushort PeekUInt16() => PeekUInt16(ByteOrder);

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekUInt32(ByteOrder)"/>
        public virtual uint PeekUInt32() => PeekUInt32(ByteOrder);

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using the current byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekUInt64(ByteOrder)"/>
        public virtual ulong PeekUInt64() => PeekUInt64(ByteOrder);

        /// <summary>
        /// Reads a globally unique identifier from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="PeekGuid(ByteOrder)"/>
        public virtual Guid PeekGuid() => PeekGuid(ByteOrder);

        #endregion

        #region ByteOrder Peek

        /// <summary>
        /// Reads a 2-byte floating point value from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="ReadHalf(ByteOrder)"/>
        public virtual Half PeekHalf(ByteOrder byteOrder)
        {
            var origin = BaseStream.Position;
            var value = ReadHalf(byteOrder);
            BaseStream.Position = origin;
            return value;
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using the specified byte order
        /// and does not advance the current position of the stream.
        /// </summary>
        /// <inheritdoc cref="ReadSingle(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadDouble(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadDecimal(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadInt16(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadInt32(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadInt64(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadUInt16(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadUInt32(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadUInt64(ByteOrder)"/>
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
        /// <inheritdoc cref="ReadGuid(ByteOrder)"/>
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
        /// Gets the position of the base stream.
        /// If the current instance was created using <see cref="CreateVirtualReader"/>
        /// the position returned will be relative to the virtual origin.
        /// </summary>
        public long Position => BaseStream.Position - virtualOrigin;

        /// <summary>
        /// Sets the position of the underlying stream relative to a given origin.
        /// </summary>
        /// <param name="offset">A byte offest relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public void Seek(long offset, SeekOrigin origin)
        {
            var address = origin switch
            {
                SeekOrigin.Current => BaseStream.Position + offset,
                SeekOrigin.End => BaseStream.Length + offset,
                _ => virtualOrigin + offset
            };

            SeekAbsolute(address);
        }

        private void SeekAbsolute(long address)
        {
            if (BaseStream.Position != address)
                BaseStream.Position = address;
        }

        /// <summary>
        /// Creates an <seealso cref="EndianReader"/> based on the same stream
        /// with the same byte order and encoding that will treat the current position
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        public virtual EndianReader CreateVirtualReader() => CreateVirtualReader(BaseStream.Position);

        /// <summary>
        /// Creates an <seealso cref="EndianReader"/> based on the same stream
        /// with the same byte order and encoding that will treat the specified offset
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        /// <param name="origin">The position in the stream that will be treated as the beginning.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public virtual EndianReader CreateVirtualReader(long origin)
        {
            return origin < 0 || origin > BaseStream.Length
                ? throw Exceptions.OutOfStreamBounds(origin)
                : new EndianReader(this, origin);
        }

        /// <summary>
        /// Calls and returns the value of <seealso cref="ReadObject{T}"/> until the specified number of objects have been read or the end of the stream has been reached.
        /// </summary>
        /// <inheritdoc cref="ReadEnumerable{T}(int, double)"/>
        public IEnumerable<T> ReadEnumerable<T>(int count)
        {
            if (count < 0)
                throw Exceptions.ParamMustBeNonNegative(count);

            var i = 0;
            while (i++ < count && BaseStream.Position < BaseStream.Length)
                yield return ReadObject<T>();
        }

        /// <summary>
        /// Calls and returns the value of <seealso cref="ReadObject{T}(double)"/> until either the specified number of objects have been read or the end of the stream has been reached.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="count">The maximum number of objects to read.</param>
        /// <param name="version">The version of the type to read.</param>
        /// <returns></returns>
        /// <inheritdoc cref="ReadObject{T}(double)"/>
        public IEnumerable<T> ReadEnumerable<T>(int count, double version)
        {
            if (count < 0)
                throw Exceptions.ParamMustBeNonNegative(count);

            var i = 0;
            while (i++ < count && BaseStream.Position < BaseStream.Length)
                yield return ReadObject<T>(version);
        }

        /// <summary>
        /// Populates a fixed length array using the result of <seealso cref="ReadObject{T}"/> for index of the array.
        /// </summary>
        /// <inheritdoc cref="ReadEnumerable{T}(int)"/>
        public T[] ReadArray<T>(int count)
        {
            if (count < 0)
                throw Exceptions.ParamMustBeNonNegative(count);

            var result = new T[count];
            for (var i = 0; i < count; i++)
                result[i] = ReadObject<T>();

            return result;
        }

        /// <summary>
        /// Populates a fixed length array using the result of <seealso cref="ReadObject{T}(double)"/> for index of the array.
        /// </summary>
        /// <inheritdoc cref="ReadEnumerable{T}(int, double)"/>
        public T[] ReadArray<T>(int count, double version)
        {
            if (count < 0)
                throw Exceptions.ParamMustBeNonNegative(count);

            var result = new T[count];
            for (var i = 0; i < count; i++)
                result[i] = ReadObject<T>(version);

            return result;
        }

        #endregion

        #region Dynamic Read

        /// <inheritdoc cref="ReadObject{T}(double)"/>
        public T ReadObject<T>() => (T)ReadObject(null, typeof(T), null);

        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <inheritdoc cref="ReadObject(Type, double)"/>
        public T ReadObject<T>(double version) => (T)ReadObject(null, typeof(T), version);

        /// <inheritdoc cref="ReadObject(Type, double)"/>
        public object ReadObject(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return ReadObject(null, type, null);
        }

        /// <summary>
        /// Reads a dynamic object from the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// The type being read must have a public parameterless constructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <param name="type">The type of object to read.</param>
        /// <returns>A new instance of the specified type whose values have been populated from the current stream.</returns>
        /// <inheritdoc cref="ReadObject(object, double)"/>
        public object ReadObject(Type type, double version)
        {
            ArgumentNullException.ThrowIfNull(type);
            return ReadObject(null, type, version);
        }

        /// <inheritdoc cref="ReadObject{T}(T, double)"/>
        public T ReadObject<T>(T instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return (T)ReadObject(instance, instance.GetType(), null);
        }

        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <inheritdoc cref="ReadObject(object, double)"/>
        public T ReadObject<T>(T instance, double version)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return (T)ReadObject(instance, instance.GetType(), version);
        }

        /// <inheritdoc cref="ReadObject(object, double)"/>
        public object ReadObject(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a dynamic object from the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <returns>The same object that was supplied as the <paramref name="instance"/> parameter.</returns>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// <para>
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </para>
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(object instance, double version)
        {
            ArgumentNullException.ThrowIfNull(instance);
            return ReadObject(instance, instance.GetType(), version);
        }

        /// <summary>
        /// This function is called by all public ReadObject overloads.
        /// </summary>
        /// <param name="instance">The object to populate. This value will be null if no instance was provided.</param>
        /// <param name="type">The type of object to read.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// This value will be null if no version was provided.
        /// </param>
        protected virtual object ReadObject(object instance, Type type, double? version)
        {
            //cannot detect string type automatically (fixed/prefixed/terminated)
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();

            //take note of origin before creating instance in case a derived class moves the stream.
            //this is important for attributes like FixedSizeAttribute to ensure the final position is correct.
            var origin = Position;
            instance ??= CreateInstance(type, version);

            return TypeConfiguration.Populate(instance, type, this, origin, version);
        }

        protected virtual object CreateInstance(Type type, double? version) => Activator.CreateInstance(type);

        /// <inheritdoc cref="ReadBufferable{T}(ByteOrder)"/>
        public T ReadBufferable<T>() where T : IBufferable<T>
            => ReadBufferable<T>(ByteOrder);

        /// <summary>
        /// Reads a bufferable type from the underlying stream and advances the current position of the stream
        /// by the number of bytes specified by the type's implementation of <see cref="IBufferable.SizeOf"/>.
        /// </summary>
        /// <typeparam name="T">The bufferable type to read.</typeparam>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <returns>A new instance of the specified bufferable type.</returns>
        /// <remarks>
        /// Bufferable types are expected to be a contiguous span of bytes containing all data required to instanciate the type.
        /// All relevant properties of the type must be deserialized during <see cref="IBufferable{TBufferable}.ReadFromBuffer(ReadOnlySpan{byte})"/>.
        /// <see cref="OffsetAttribute"/> and other related attributes will be ignored.
        /// </remarks>
        public T ReadBufferable<T>(ByteOrder byteOrder) where T : IBufferable<T>
        {
            var buffer = ReadBytes(T.SizeOf);
            if (T.PackSize > 1 && byteOrder != NativeByteOrder)
                Utils.ReverseEndianness(buffer, T.PackSize);

            return T.ReadFromBuffer(buffer);
        }

        #endregion
    }
}
