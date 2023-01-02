using Reclaimer.Geometry.Vectors;
using System.Collections;
using System.Numerics;

namespace Reclaimer.Geometry
{
    public static class VectorBuffer
    {
        public static IVectorBuffer Transform3d(IVectorBuffer buffer, Vector3 scale, Vector3 offset) => Transform3d(buffer, Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(offset));
        public static IVectorBuffer Transform3d(IVectorBuffer buffer, Matrix4x4 transform) => new TransformedVectorBuffer(buffer, transform);

        private sealed class TransformedVectorBuffer : IVectorBuffer
        {
            private readonly IVectorBuffer source;
            private readonly Matrix4x4 transform;

            public TransformedVectorBuffer(IVectorBuffer source, Matrix4x4 transform)
            {
                if (source is TransformedVectorBuffer t)
                {
                    this.source = t.source;
                    this.transform = t.transform * transform;
                }
                else
                {
                    this.source = source;
                    this.transform = transform;
                }
            }

            public IVector this[int index]
            {
                get
                {
                    var result = source[index];
                    if (transform.IsIdentity)
                        return result;

                    var vec = Vector3.Transform(new Vector3(result.X, result.Y, result.Z), transform);
                    return new RealVector3(vec);
                }
            }

            public int Dimensions => source.Dimensions;
            public int Count => source.Count;

            IVectorBuffer IVectorBuffer.Slice(int index, int count) => new TransformedVectorBuffer(source.Slice(index, count), transform);
            void IVectorBuffer.ReverseEndianness() => throw new NotSupportedException("Operation not supported on a transformed vector buffer");

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
