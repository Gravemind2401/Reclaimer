using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    [FixedSize(8)]
    public struct RealBounds : IRealBounds
    {
        private float min, max;

        [Offset(0)]
        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        [Offset(4)]
        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        public RealBounds(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Length => max - min;

        public float Midpoint => (min + max) / 2;

        public override string ToString() => Utils.CurrentCulture($"[{Min:F6}, {Max:F6}]");

        #region Equality Operators

        public static bool operator ==(RealBounds bounds1, RealBounds bounds2)
        {
            return bounds1.Min == bounds2.Min &&
                   bounds1.Max == bounds2.Max;
        }

        public static bool operator !=(RealBounds point1, RealBounds point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(RealBounds bounds1, RealBounds bounds2)
        {
            return bounds1.Min.Equals(bounds2.Min) &&
                   bounds1.Max.Equals(bounds2.Max);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is RealBounds))
                return false;

            return RealBounds.Equals(this, (RealBounds)obj);
        }

        public bool Equals(RealBounds value)
        {
            return RealBounds.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode()
                ^ Max.GetHashCode();
        }

        #endregion
    }
}
