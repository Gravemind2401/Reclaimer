using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 3-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(12)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector3D : IRealVector3D, IXMVector
    {
        [Offset(0)]
        public float X { get; set; }

        [Offset(4)]
        public float Y { get; set; }

        [Offset(8)]
        public float Z { get; set; }

        public RealVector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        #region IXMVector

        float IXMVector.W
        {
            get => float.NaN;
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.Float32_3;

        #endregion

        #region Equality Operators

        public static bool operator ==(RealVector3D value1, RealVector3D value2) => value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        public static bool operator !=(RealVector3D value1, RealVector3D value2) => !(value1 == value2);

        public static bool Equals(RealVector3D value1, RealVector3D value2) => value1.X.Equals(value2.X) && value1.Y.Equals(value2.Y) && value1.Z.Equals(value2.Z);
        public override bool Equals(object obj)=> obj is RealVector3D value && RealVector3D.Equals(this, value);
        public bool Equals(RealVector3D value) => RealVector3D.Equals(this, value);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        #endregion
    }
}
