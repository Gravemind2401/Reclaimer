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
    [FixedSize(8)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealBounds : IRealBounds
    {
        private float min, max;

        [Offset(0)]
        public float Min
        {
            get => min;
            set => min = value;
        }

        [Offset(4)]
        public float Max
        {
            get => max;
            set => max = value;
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

        public static bool operator ==(RealBounds value1, RealBounds value2) => value1.min == value2.min && value1.max == value2.max;
        public static bool operator !=(RealBounds value1, RealBounds value2) => !(value1 == value2);

        public static bool Equals(RealBounds value1, RealBounds value2) => value1.min.Equals(value2.min) && value1.max.Equals(value2.max);
        public override bool Equals(object obj)=> obj is RealBounds value && RealBounds.Equals(this, value);
        public bool Equals(RealBounds value) => RealBounds.Equals(this, value);

        public override int GetHashCode() => min.GetHashCode() ^ max.GetHashCode();

        #endregion
    }
}
