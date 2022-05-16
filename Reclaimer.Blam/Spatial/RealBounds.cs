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
        [Offset(0)]
        public float Min { get; set; }

        [Offset(4)]
        public float Max { get; set; }

        public RealBounds(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Length => Max - Min;

        public float Midpoint => (Min + Max) / 2;

        public override string ToString() => Utils.CurrentCulture($"[{Min:F6}, {Max:F6}]");

        #region Equality Operators

        public static bool operator ==(RealBounds value1, RealBounds value2) => value1.Min == value2.Min && value1.Max == value2.Max;
        public static bool operator !=(RealBounds value1, RealBounds value2) => !(value1 == value2);

        public static bool Equals(RealBounds value1, RealBounds value2) => value1.Min.Equals(value2.Min) && value1.Max.Equals(value2.Max);
        public override bool Equals(object obj)=> obj is RealBounds value && RealBounds.Equals(this, value);
        public bool Equals(RealBounds value) => RealBounds.Equals(this, value);

        public override int GetHashCode() => Min.GetHashCode() ^ Max.GetHashCode();

        #endregion
    }
}
