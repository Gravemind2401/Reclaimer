using Adjutant.Spatial;
using Reclaimer;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Adjutant.Geometry
{
    public static class Extensions
    {
        public static Matrix4x4 AsTransform(this IRealBounds5D bounds)
        {
            return new Matrix4x4
            {
                M11 = bounds.XBounds.Length,
                M22 = bounds.YBounds.Length,
                M33 = bounds.ZBounds.Length,
                M41 = bounds.XBounds.Min,
                M42 = bounds.YBounds.Min,
                M43 = bounds.ZBounds.Min
            };
        }

        public static Matrix4x4 AsTextureTransform(this IRealBounds5D bounds)
        {
            return new Matrix4x4
            {
                M11 = bounds.UBounds.Length,
                M22 = bounds.VBounds.Length,
                M41 = bounds.UBounds.Min,
                M42 = bounds.VBounds.Min
            };
        }
    }
}
