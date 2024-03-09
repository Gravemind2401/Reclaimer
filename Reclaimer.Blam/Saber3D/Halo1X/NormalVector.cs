using Reclaimer.Geometry;
using Reclaimer.IO;
using System.Numerics;

// https://github.com/Wildenhaus/IndexV2/blob/game-modules/external/LibSaber/src/LibSaber/Common/SaberMath.cs

namespace Reclaimer.Saber3D.Halo1X
{
    /// <summary>
    /// A 3-dimensional vector packed into 16 bits using a form of ancient forbidden math that has been lost to time.
    /// <br/> I have no idea what the precision is. The Y coordinate is calculated from the X and Z coordinates.
    /// <br/> Each axis has a possible value range lying somewhere between <see cref="float.NegativeInfinity"/> and <see cref="float.PositiveInfinity"/>.
    /// </summary>
    public record struct NormalVector : IVector3, IBufferableVector<NormalVector>
    {
        private const int packSize = sizeof(short);
        private const int structureSize = sizeof(short);

        private const float xfrac1 = 1f / 181f;
        private const float xfrac2 = 181f / 179f;

        private const float zfrac1 = 1f / 181f / 181f;
        private const float zfrac2 = 181f / 180f;

        private readonly short bits;

        #region Axis Properties

        public float X
        {
            readonly get
            {
                var frac = xfrac1 * Math.Abs(bits) % 1f;
                return (-1f + 2f * frac) * xfrac2;
            }
            set => throw new NotImplementedException();
        }

        public float Y
        {
            readonly get => Math.Sign(bits) * MathF.Sqrt(float.Clamp(1f - X * X - Z * Z, 0, 1));
            set => throw new NotSupportedException("The Y value is a function of the X and Z values and cannot be set directly.");
        }

        public float Z
        {
            readonly get
            {
                var frac = zfrac1 * Math.Abs(bits) % 1f;
                return (-1f + 2f * frac) * zfrac2;
            }
            set => throw new NotImplementedException();
        }

        #endregion

        public NormalVector(short value)
        {
            bits = value;
        }

        public NormalVector(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public NormalVector(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static NormalVector IBufferable<NormalVector>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new NormalVector(BitConverter.ToInt16(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(NormalVector value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator NormalVector(Vector3 value) => new NormalVector(value);

        public static explicit operator short(NormalVector value) => value.bits;
        public static explicit operator NormalVector(short value) => new NormalVector(value);

        #endregion
    }
}
