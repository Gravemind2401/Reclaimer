using Reclaimer.IO;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public class IndexBuffer : DataBuffer<int>, IIndexBuffer
    {
        private delegate int ReadMethod(ReadOnlySpan<byte> buffer);
        private delegate void WriteMethod(int value, Span<byte> buffer);

        public static IndexBuffer FromArray(byte[] data, Type dataType)
        {
            if (dataType == typeof(byte))
                return new IndexBuffer(data, sizeof(byte));
            else if (dataType == typeof(ushort))
                return new IndexBuffer(data, sizeof(ushort));
            else if (dataType == typeof(byte))
                return new IndexBuffer(data, sizeof(int));
            else
                throw new ArgumentException("Data type must be byte, ushort or int.", nameof(dataType));
        }

        private readonly ReadMethod GetValue;
        private readonly WriteMethod SetValue;

        protected override int SizeOf { get; }

        private IndexBuffer(byte[] buffer, int size)
            : base(buffer, buffer.Length / size, 0, size, 0)
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
                GetValue = data => BitConverter.ToInt32(data);
                SetValue = (i, data) => BitConverter.GetBytes(i).CopyTo(data);
            }
        }

        public override int this[int index]
        {
            get => GetValue(CreateSpan(index));
            set => SetValue(value, CreateSpan(index));
        }

        public void SwapEndianness()
        {
            if (SizeOf == 1)
                return;

            for (var i = 0; i < Count; i++)
                CreateSpan(i).Reverse();
        }
    }
}
