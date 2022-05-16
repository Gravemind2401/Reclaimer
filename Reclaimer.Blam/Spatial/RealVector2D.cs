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
    /// A 2-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(8)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector2D : IRealVector2D, IXMVector
    {
        [Offset(0)]
        public float X { get; set; }

        [Offset(4)]
        public float Y { get; set; }

        public RealVector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}]");

        #region IXMVector

        float IXMVector.Z
        {
            get => float.NaN;
            set { }
        }

        float IXMVector.W
        {
            get => float.NaN;
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.Float32_2;

        #endregion

        #region Equality Operators

        public static bool operator ==(RealVector2D value1, RealVector2D value2) => value1.X == value2.X && value1.Y == value2.Y;
        public static bool operator !=(RealVector2D value1, RealVector2D value2) => !(value1 == value2);

        public static bool Equals(RealVector2D value1, RealVector2D value2) => value1.X.Equals(value2.X) && value1.Y.Equals(value2.Y);
        public override bool Equals(object obj)=> obj is RealVector2D value && RealVector2D.Equals(this, value);
        public bool Equals(RealVector2D value) => RealVector2D.Equals(this, value);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        #endregion
    }
}
