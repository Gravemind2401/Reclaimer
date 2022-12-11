namespace Adjutant.Spatial
{
    public interface IRealBounds
    {
        float Min { get; set; }
        float Max { get; set; }
        float Length { get; }
        float Midpoint { get; }
    }
}
