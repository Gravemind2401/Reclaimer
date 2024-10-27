using Reclaimer.IO.Dynamic;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Text;

namespace Reclaimer.IO
{
    /// <summary>
    /// Writes primitive and dynamic data types to a stream in a specific byte order and encoding.
    /// </summary>
    public class EndianWriter : BinaryWriter, IEndianStream
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
        /// Gets or sets the endianness used when writing to the stream.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the system byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianWriter(Stream output)
            : this(output, BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian, new UTF8Encoding(), false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianWriter(Stream output, ByteOrder byteOrder)
            : this(output, byteOrder, new UTF8Encoding(), false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order using UTF-8 encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianWriter object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianWriter(Stream output, ByteOrder byteOrder, bool leaveOpen)
            : this(output, byteOrder, new UTF8Encoding(), leaveOpen)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public EndianWriter(Stream output, ByteOrder byteOrder, Encoding encoding)
            : this(output, byteOrder, encoding, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="EndianWriter"/> class
        /// based on the specified stream with the specified byte order and character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="byteOrder">The byte order of the stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after the EndianWriter object is disposed; otherwise, false.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
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
            ArgumentNullException.ThrowIfNull(parent);

            this.virtualOrigin = virtualOrigin;
            encoding = parent.encoding;
            ByteOrder = parent.ByteOrder;
        }

        private static Stream BaseStreamOrThrow(EndianWriter parent) => parent?.BaseStream ?? throw new ArgumentNullException(nameof(parent));

        private static Encoding EncodingOrThrow(EndianWriter parent) => parent?.encoding ?? throw new ArgumentNullException(nameof(parent));

        #endregion

        #region Overrides

        /// <summary>
        /// Writes a two-byte floating-point value to the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="Write(Half, ByteOrder)"/>
        public override void Write(Half value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="Write(float, ByteOrder)"/>
        public override void Write(float value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="Write(double, ByteOrder)"/>
        public override void Write(double value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a decimal value to the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="Write(decimal, ByteOrder)"/>
        public override void Write(decimal value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="Write(short, ByteOrder)"/>
        public override void Write(short value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public override void Write(int value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="Write(long, ByteOrder)"/>
        public override void Write(long value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="Write(ushort, ByteOrder)"/>
        public override void Write(ushort value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="Write(uint, ByteOrder)"/>
        public override void Write(uint value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using the current byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="Write(ulong, ByteOrder)"/>
        public override void Write(ulong value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a length-prefixed string to the current stream using the current byte order
        /// and encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <inheritdoc cref="Write(string, ByteOrder)"/>
        public override void Write(string value) => Write(value, ByteOrder);

        /// <summary>
        /// Writes a globally unique identifier to the current stream using the current byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <inheritdoc cref="Write(Guid, ByteOrder)"/>
        public virtual void Write(Guid value) => Write(value, ByteOrder);

        #endregion

        #region ByteOrder Write

        /// <summary>
        /// Writes a two-byte floating-point value to the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(Half)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(Half value, ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
            {
                base.Write(value);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(float)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(float value, ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
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
        /// <inheritdoc cref="BinaryWriter.Write(double)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(double value, ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
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
        /// <inheritdoc cref="BinaryWriter.Write(decimal)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(decimal value, ByteOrder byteOrder)
        {
            if (byteOrder == NativeByteOrder)
            {
                base.Write(value);
                return;
            }

            var bits = decimal.GetBits(value);
            var bytes = new byte[16];

            for (var i = 0; i < 4; i++)
                Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * 4, 4);

            Array.Reverse(bytes);
            base.Write(bytes);
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(short)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(short value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <inheritdoc cref="BinaryWriter.Write(int)"/>
        public virtual void Write(int value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(long)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(long value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(ushort)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(ushort value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(uint)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(uint value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream using the specified byte order
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <inheritdoc cref="BinaryWriter.Write(ulong)"/>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(ulong value, ByteOrder byteOrder) => base.Write(byteOrder == NativeByteOrder ? value : BinaryPrimitives.ReverseEndianness(value));

        /// <summary>
        /// Writes a globally unique identifier to the current stream using the specified byte order
        /// and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <param name="value">The unique identifier to write.</param>
        /// <inheritdoc cref="Write(int, ByteOrder)"/>
        public virtual void Write(Guid value, ByteOrder byteOrder)
        {
            var bytes = value.ToByteArray();
            var a = BitConverter.ToInt32(bytes, 0);
            var b = BitConverter.ToInt16(bytes, 4);
            var c = BitConverter.ToInt16(bytes, 6);

            Write(a, byteOrder);
            Write(b, byteOrder);
            Write(c, byteOrder);
            Write(bytes.AsSpan()[8..]);
        }

        #endregion

        #region String Write

        /// <summary>
        /// Writes a length-prefixed string to the current stream using the specified byte order
        /// and the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <param name="byteOrder">The byte order to use when writing the length value.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void Write(string value, ByteOrder byteOrder)
        {
            Write(encoding.GetByteCount(value), byteOrder);
            Write(encoding.GetBytes(value));
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="WriteStringFixedLength(string, int, char)"/>
        public virtual void WriteStringFixedLength(string value) => WriteStringFixedLength(value, value.Length);

        /// <remarks>
        /// If the string is shorter than the specified length it will be padded with white-space.
        /// If the string is longer than the specified length it will be truncated.
        /// </remarks>
        /// <inheritdoc cref="WriteStringFixedLength(string, int, char)"/>
        public virtual void WriteStringFixedLength(string value, int length) => WriteStringFixedLength(value, length, ' ');

        /// <summary>
        /// Writes a fixed-length string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <remarks>
        /// If the string is shorter than the specified length it will be padded using the specified character.
        /// If the string is longer than the specified length it will be truncated.
        /// </remarks>
        /// <param name="value">The string value to write.</param>
        /// <param name="length">The number of characters to write.</param>
        /// <param name="padding">The character to be used as padding.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void WriteStringFixedLength(string value, int length, char padding)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (length < 0)
                throw Exceptions.ParamMustBeNonNegative(length);

            if (length == 0)
                return;

            if (value.Length > length)
                value = value[..length];
            else
            {
                while (value.Length < length)
                    value += padding;
            }

            base.Write(encoding.GetBytes(value));
        }

        /// <summary>
        /// Writes a null-terminated string to the current stream using the current encoding of the <seealso cref="EndianWriter"/>.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="ObjectDisposedException"/>
        public virtual void WriteStringNullTerminated(string value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Write(encoding.GetBytes(value + '\0'));
        }

        #endregion

        #region Other

        /// <summary>
        /// Gets the position of the base stream.
        /// If the current instance was created using <see cref="CreateVirtualWriter"/>
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

            if (BaseStream.Position != address)
                BaseStream.Position = address;
        }

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream
        /// with the same byte order and encoding that will treat the current position
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        public virtual EndianWriter CreateVirtualWriter() => CreateVirtualWriter(BaseStream.Position);

        /// <summary>
        /// Creates an <seealso cref="EndianWriter"/> based on the same stream
        /// with the same byte order and encoding that will treat the specified offset
        /// as the beginning of the stream and will not dispose of the underlying stream when it is closed.
        /// </summary>
        /// <param name="origin">The position in the stream that will be treated as the beginning.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public virtual EndianWriter CreateVirtualWriter(long origin)
        {
            //don't check stream bounds for writer - it can typically write beyond EOF
            return new EndianWriter(this, origin);
        }

        /// <summary>
        /// Calls <see cref="WriteObject{T}(T)"/> for each value in the set.
        /// </summary>
        /// <inheritdoc cref="WriteEnumerable{T}(IEnumerable{T}, double)"/>
        public void WriteEnumerable<T>(IEnumerable<T> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            foreach (var value in values)
                WriteObject(value);
        }

        /// <summary>
        /// Calls <see cref="WriteObject{T}(T, double)"/> for each value in the set.
        /// </summary>
        /// <typeparam name="T">The type of object the set contains.</typeparam>
        /// <param name="values">The set of values to write.</param>
        /// <param name="version">The version of the type to write.</param>
        /// <inheritdoc cref="WriteObject{T}(T, double)"/>
        public void WriteEnumerable<T>(IEnumerable<T> values, double version)
        {
            ArgumentNullException.ThrowIfNull(values);

            foreach (var value in values)
                WriteObject(value, version);
        }

        /// <summary>
        /// Inserts data at the current position. Any following data will be moved forward and the stream will be expanded.
        /// </summary>
        /// <param name="buffer">The data to insert.</param>
        public void Insert(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (buffer.Length == 0)
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
                throw Exceptions.ParamMustBeNonNegative(length);
            else if (length == 0)
                return;

            var source = BaseStream.Position;
            if (source < BaseStream.Length)
            {
                //shift everything to the right by [length] places to make room for inserted bytes
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
                throw Exceptions.ParamMustBeNonNegative(length);
            else if (length == 0)
                return;

            var buffer = new byte[length];

            if (pad != default(byte))
            {
                for (var i = 0; i < length; i++)
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
                throw Exceptions.ParamMustBePositive(sourceAddress);

            if (destinationAddress <= 0)
                throw Exceptions.ParamMustBePositive(destinationAddress);

            if (length <= 0)
                throw Exceptions.ParamMustBePositive(length);

            const int blockSize = 0x10000;
            var origin = BaseStream.Position;

            var buffer = new byte[blockSize];
            for (var remaining = length; remaining > 0;)
            {
                var readLength = Math.Min(blockSize, remaining);
                var offset = sourceAddress > destinationAddress
                    ? length - remaining
                    : remaining - readLength;

                Seek(sourceAddress + offset, SeekOrigin.Begin);
                BaseStream.ReadAll(buffer, 0, readLength);

                Seek(destinationAddress + offset, SeekOrigin.Begin);
                BaseStream.Write(buffer, 0, readLength);

                remaining -= readLength;
            }

            Seek(origin, SeekOrigin.Begin);
        }

        #endregion

        #region Dynamic Write

        /// <inheritdoc cref="WriteObject{T}(T, double)"/>
        public void WriteObject<T>(T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            WriteObjectGeneric(value, null);
        }

        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <inheritdoc cref="WriteObject(object, double)"/>
        public void WriteObject<T>(T value, double version)
        {
            ArgumentNullException.ThrowIfNull(value);
            WriteObjectGeneric(value, version);
        }

        /// <inheritdoc cref="WriteObject(object, double)"/>
        public void WriteObject(object value)
        {
            ArgumentNullException.ThrowIfNull(value);
            InvokeWriteObject(value, null);
        }

        /// <summary>
        /// Writes a dynamic object to the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// The type being written must have a public parameterless constructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// <para>
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </para>
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject(object value, double version)
        {
            ArgumentNullException.ThrowIfNull(value);
            InvokeWriteObject(value, version);
        }

        private static readonly MethodInfo DynamicWriteMethod = typeof(EndianWriter)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(m => m.Name == nameof(WriteObjectGeneric) && m.IsGenericMethodDefinition);

        protected void InvokeWriteObject(object value, double? version)
        {
            DynamicWriteMethod.MakeGenericMethod(value.GetType())
                .Invoke(this, new object[] { value, version });
        }

        /// <summary>
        /// This function is called by all public WriteObject overloads.
        /// </summary>
        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </param>
        protected virtual void WriteObjectGeneric<T>(T value, double? version)
        {
            ArgumentNullException.ThrowIfNull(value);

            //cannot detect string type automatically (fixed/prefixed/terminated)
            if (typeof(T).Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();

            if (DelegateHelper.IsTypeSupported<T>())
                DelegateHelper<T>.InvokeDefaultWrite(this, value);
            else
                StructureDefinition<T>.Write(ref value, this, ref version);
        }

        /// <inheritdoc cref="WriteBufferable{T}(T, ByteOrder)"/>
        public void WriteBufferable<T>(T value) where T : IBufferable
            => WriteBufferable(value, ByteOrder);

        /// <summary>
        /// Writes a bufferable type to the underlying stream and advances the current position of the stream
        /// by the number of bytes specified by the type's implementation of <see cref="IBufferable.SizeOf"/>.
        /// </summary>
        /// <typeparam name="T">The bufferable type to write.</typeparam>
        /// <param name="value">The object to write.</param>
        /// <param name="byteOrder">The byte order to use.</param>
        /// <remarks>
        /// Bufferable types are expected to be a contiguous span of bytes containing all data required to instanciate the type.
        /// All relevant properties of the type must be serialized during <see cref="IBufferable.WriteToBuffer(Span{byte})"/>.
        /// <see cref="OffsetAttribute"/> and other related attributes will be ignored.
        /// </remarks>
        public void WriteBufferable<T>(T value, ByteOrder byteOrder) where T : IBufferable
        {
            var buffer = new byte[T.SizeOf];
            value.WriteToBuffer(buffer);

            if (T.PackSize > 1 && byteOrder != NativeByteOrder)
                Utils.ReverseEndianness(buffer, T.PackSize);

            Write(buffer);
        }

        #endregion
    }
}