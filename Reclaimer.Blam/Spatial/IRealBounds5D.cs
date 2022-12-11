namespace Adjutant.Spatial
{
    public interface IRealBounds5D
    {
        IRealBounds XBounds { get; }
        IRealBounds YBounds { get; }
        IRealBounds ZBounds { get; }
        IRealBounds UBounds { get; }
        IRealBounds VBounds { get; }
    }
}
