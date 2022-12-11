using System;
using System.Collections.Generic;

namespace Reclaimer.IO
{
    public class BufferedCollection<TBufferable> : DataBuffer<TBufferable>, ICollection<TBufferable>
        where TBufferable : struct, IBufferable<TBufferable>
    {
        private static int TPack => TBufferable.PackSize;
        private static int TSize => TBufferable.SizeOf;

        protected override int SizeOf => TSize;

        public BufferedCollection(int count)
            : base(new byte[count * TSize], count, 0, TSize, 0)
        { }

        public BufferedCollection(byte[] buffer)
            : base(buffer, buffer?.Length / TSize ?? default, 0, TSize, 0)
        { }

        public BufferedCollection(byte[] buffer, int count)
            : base(buffer, count, 0, TSize, 0)
        { }

        public BufferedCollection(byte[] buffer, int count, int stride)
            : base(buffer, count, 0, stride, 0)
        { }

        public BufferedCollection(byte[] buffer, int count, int stride, int offset)
            : base(buffer, count, 0, stride, offset)
        { }

        public BufferedCollection(byte[] buffer, int count, int start, int stride, int offset)
            : base(buffer, count, start, stride, offset)
        { }

        public override TBufferable this[int index]
        {
            get => TBufferable.ReadFromBuffer(CreateSpan(index));
            set => value.WriteToBuffer(CreateSpan(index));
        }

        public void ReverseEndianness()
        {
            if (TPack == 1)
                return;

            for (var i = 0; i < Count; i++)
                Utils.ReverseEndianness(CreateSpan(i), TPack);
        }

        /// <summary>
        /// Returns the array of unsigned bytes from which this collection was created.
        /// </summary>
        /// <returns>
        /// The byte array from which this collection was created, or the underlying array if
        /// a byte array was not provided to the <seealso cref="BufferedCollection{T}"/> constructor
        /// during construction of the current instance.
        /// </returns>
        public byte[] GetBuffer() => buffer;

        #region ICollection
        public void CopyTo(TBufferable[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            while (arrayIndex < array.Length && arrayIndex < Count)
                array[arrayIndex] = this[arrayIndex++];
        }

        bool ICollection<TBufferable>.IsReadOnly => true;
        void ICollection<TBufferable>.Add(TBufferable item) => throw new NotSupportedException();
        void ICollection<TBufferable>.Clear() => throw new NotSupportedException();
        bool ICollection<TBufferable>.Contains(TBufferable item) => throw new NotSupportedException();
        bool ICollection<TBufferable>.Remove(TBufferable item) => throw new NotSupportedException();
        #endregion
    }
}
