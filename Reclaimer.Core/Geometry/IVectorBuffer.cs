using Reclaimer.Geometry.Vectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        int Dimensions { get; }
        IVectorBuffer Slice(int index, int count);
        void SwapEndianness();
        
        sealed IVector this[Index index] => ((IReadOnlyList<IVector>)this)[index.GetOffset(Count)];
        sealed IEnumerable<IVector> this[Range range] => Extensions.GetRange(this, range);

        sealed IVectorBuffer AsTransformed(Vector3 offset, Vector3 scale) => AsTransformed(Matrix4x4.CreateTranslation(offset) * Matrix4x4.CreateScale(scale));
        sealed IVectorBuffer AsTransformed(Matrix4x4 transform) => new TransformedVectorBuffer(this, transform);

        private sealed class TransformedVectorBuffer : IVectorBuffer
        {
            private readonly IVectorBuffer source;
            private readonly Matrix4x4 transform;

            public TransformedVectorBuffer(IVectorBuffer source, Matrix4x4 transform)
            {
                this.source = source;
                this.transform = transform;
            }

            public IVector this[int index]
            {
                get
                {
                    var result = source[index];
                    if (transform.IsIdentity)
                        return result;

                    //TODO: maybe use different vector struct depending on source.Dimensions?
                    var vec = Vector4.Transform(new Vector4(result.X, result.Y, result.Z, result.W), transform);
                    return new RealVector4(vec);
                }
            }

            public int Dimensions => source.Dimensions;
            public int Count => source.Count;

            IVectorBuffer IVectorBuffer.Slice(int index, int count) => new TransformedVectorBuffer(source.Slice(index, count), transform);
            void IVectorBuffer.SwapEndianness() => throw new NotSupportedException("Operation not supported on a transformed buffer");

            IEnumerator<IVector> IEnumerable<IVector>.GetEnumerator() => Enumerator.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Enumerator).GetEnumerator();

            private IEnumerable<IVector> Enumerator
            {
                get
                {
                    for (var i = 0; i < source.Count; i++)
                        yield return this[i];
                }
            }
        }
    }
}
