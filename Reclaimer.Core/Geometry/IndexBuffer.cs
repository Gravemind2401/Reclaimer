using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Reclaimer.Geometry
{
    public partial class IndexBuffer : DataBuffer<int>, IIndexBuffer
    {
        private delegate int ReadMethod(ReadOnlySpan<byte> buffer);
        private delegate void WriteMethod(int value, Span<byte> buffer);

        private static int CoerceSize(Type dataType)
        {
            if (dataType == typeof(byte))
                return sizeof(byte);
            else if (dataType == typeof(ushort))
                return sizeof(ushort);
            else if (dataType == typeof(int))
                return sizeof(int);
            else
                throw new ArgumentException("Data type must be byte, ushort or int.", nameof(dataType));
        }

        public static IndexBuffer FromCollection(IEnumerable<int> collection, IndexFormat layout) => FromArray(collection.ToArray(), layout);
        public static IndexBuffer FromCollection(IEnumerable<ushort> collection, IndexFormat layout) => FromArray(collection.ToArray(), layout);
        public static IndexBuffer FromCollection(IEnumerable<byte> collection, IndexFormat layout) => FromArray(collection.ToArray(), layout);

        public static IndexBuffer FromArray(int[] collection, IndexFormat layout) => new IndexBuffer(MemoryMarshal.AsBytes<int>(collection).ToArray(), sizeof(int)) { Layout = layout };
        public static IndexBuffer FromArray(ushort[] collection, IndexFormat layout) => new IndexBuffer(MemoryMarshal.AsBytes<ushort>(collection).ToArray(), sizeof(ushort)) { Layout = layout };
        public static IndexBuffer FromArray(byte[] collection, IndexFormat layout) => new IndexBuffer(collection, sizeof(byte)) { Layout = layout };

        private readonly ReadMethod GetValue;
        private readonly WriteMethod SetValue;

        protected override int SizeOf { get; }

        public IndexFormat Layout { get; set; }

        public IndexBuffer(byte[] buffer, Type dataType)
            : this(buffer, CoerceSize(dataType))
        { }

        private IndexBuffer(byte[] buffer, int size)
            : this(buffer, buffer.Length / size, 0, size, 0, size)
        { }

        private IndexBuffer(byte[] buffer, int count, int start, int stride, int offset, int size)
            : base(buffer, count, start, stride, offset)
        {
            SizeOf = size;

            if (size == sizeof(byte))
            {
                GetValue = data => data[0];
                SetValue = (i, data) =>
                {
                    if (i > byte.MaxValue)
                        throw new ArgumentOutOfRangeException();

                    data[0] = (byte)i;
                };
            }
            else if (size == sizeof(ushort))
            {
                GetValue = data => BitConverter.ToUInt16(data);
                SetValue = (i, data) =>
                {
                    if (i > byte.MaxValue)
                        throw new ArgumentOutOfRangeException();

                    BitConverter.GetBytes((ushort)i).CopyTo(data);
                };
            }
            else
            {
                GetValue = BitConverter.ToInt32;
                SetValue = (i, data) => BitConverter.GetBytes(i).CopyTo(data);
            }
        }

        public override int this[int index]
        {
            get => GetValue(CreateSpan(index));
            set => SetValue(value, CreateSpan(index));
        }

        public IndexBuffer Slice(int index, int count)
        {
            var newStart = start + (index * stride);
            return new IndexBuffer(buffer, count, newStart, stride, offset, SizeOf);
        }

        public void ReverseEndianness()
        {
            if (SizeOf == 1)
                return;

            for (var i = 0; i < Count; i++)
                CreateSpan(i).Reverse();
        }

        IIndexBuffer IIndexBuffer.Slice(int index, int count) => Slice(index, count);
    }
}
