using Reclaimer.Geometry;

namespace Adjutant.Spatial
{
    public interface IRealBounds5D
    {
        RealBounds XBounds { get; }
        RealBounds YBounds { get; }
        RealBounds ZBounds { get; }
        RealBounds UBounds { get; }
        RealBounds VBounds { get; }
    }
}
