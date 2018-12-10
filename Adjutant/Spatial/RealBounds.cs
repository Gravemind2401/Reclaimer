using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
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
    }
}
