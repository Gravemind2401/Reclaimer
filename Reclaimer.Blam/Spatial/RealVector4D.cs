using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Runtime.InteropServices;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector4D : IRealVector4D
    {
        [Offset(0)]
        public float X { get; set; }

        [Offset(4)]
        public float Y { get; set; }

        [Offset(8)]
        public float Z { get; set; }

        [Offset(12)]
        public float W { get; set; }

        public RealVector4D(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public RealVector4D Conjugate => new RealVector4D(-X, -Y, -Z, W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");

        #region Equality Operators

        public static bool operator ==(RealVector4D value1, RealVector4D value2) => value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z && value1.W == value2.W;
        public static bool operator !=(RealVector4D value1, RealVector4D value2) => !(value1 == value2);

        public static bool Equals(RealVector4D value1, RealVector4D value2) => value1.X.Equals(value2.X) && value1.Y.Equals(value2.Y) && value1.Z.Equals(value2.Z) && value1.W.Equals(value2.W);
        public override bool Equals(object obj) => obj is RealVector4D value && RealVector4D.Equals(this, value);
        public bool Equals(RealVector4D value) => RealVector4D.Equals(this, value);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();

        #endregion
    }
}
