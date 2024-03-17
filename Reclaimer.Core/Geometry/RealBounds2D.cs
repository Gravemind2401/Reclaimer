namespace Reclaimer.Geometry
{
    public record struct RealBounds2D(Vector2 Min, Vector2 Max)
    {
        public readonly bool IsEmpty => Min == Max;
        public readonly float XLength => Max.X - Min.X;
        public readonly float YLength => Max.Y - Min.Y;

        public RealBounds2D(RealBounds xBounds, RealBounds yBounds)
            : this(new Vector2(xBounds.Min, yBounds.Min), new Vector2(xBounds.Max, yBounds.Max))
        { }

        public Matrix4x4 CreateExpansionMatrix()
        {
            return IsEmpty ? Matrix4x4.Identity : new Matrix4x4
            {
                M11 = XLength,
                M22 = YLength,
                M41 = Min.X,
                M42 = Min.Y,
                M44 = 1
            };
        }
    }
}
