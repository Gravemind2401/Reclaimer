using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public class BufferedCollection<T> : DataBuffer<T>, ICollection<T>
        where T : struct, IBufferable<T>
    {
        private delegate T ReadMethod(ReadOnlySpan<byte> buffer);

        protected static readonly int TPack = (int)typeof(T).GetProperty(nameof(IBufferable<T>.PackSize), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
        protected static readonly int TSize = (int)typeof(T).GetProperty(nameof(IBufferable<T>.SizeOf), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
        protected static readonly System.Reflection.MethodInfo TReadMethod = typeof(T).GetMethod(nameof(IBufferable<T>.ReadFromBuffer), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        private static T TReadFromBuffer(ReadOnlySpan<byte> buffer)
        {
            var action = TReadMethod.CreateDelegate<ReadMethod>();
            return action(buffer);
        }

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

        public override T this[int index]
        {
            get => TReadFromBuffer(CreateSpan(index));
            set => value.WriteToBuffer(CreateSpan(index));
        }

        public void ReverseEndianness()
        {
            if (TPack == 1)
                return;

            for (var i = 0; i < Count; i++)
            {
                var span = CreateSpan(i);
                for (var j = 0; j < TSize; j += TPack)
                    span.Slice(j, TPack).Reverse();
            }
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
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            while (arrayIndex < array.Length && arrayIndex < Count)
                array[arrayIndex] = this[arrayIndex++];
        }

        bool ICollection<T>.IsReadOnly => true;
        void ICollection<T>.Add(T item) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();
        bool ICollection<T>.Contains(T item) => throw new NotSupportedException();
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        #endregion

        #region IEnumerable
        protected IEnumerable<T> Enumerate()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Enumerate()).GetEnumerator();
        #endregion
    }
}
