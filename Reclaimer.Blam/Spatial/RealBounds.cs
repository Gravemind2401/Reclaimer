using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Runtime.InteropServices;

namespace Adjutant.Spatial
{
    [FixedSize(8)]
    [StructLayout(LayoutKind.Sequential)]
    public record struct RealBounds : IRealBounds
    {
        [Offset(0)]
        public float Min { get; set; }

        [Offset(4)]
        public float Max { get; set; }

        public float Length => Max - Min;
        public float Midpoint => (Min + Max) / 2;

        public override string ToString() => Utils.CurrentCulture($"[{Min:F6}, {Max:F6}]");
    }
}
