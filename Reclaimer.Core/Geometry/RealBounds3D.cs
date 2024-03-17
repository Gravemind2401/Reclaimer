namespace Reclaimer.Geometry
{
    public record struct RealBounds3D(Vector3 Min, Vector3 Max)
    {
        public readonly bool IsEmpty => Min == Max;
        public readonly float XLength => Max.X - Min.X;
        public readonly float YLength => Max.Y - Min.Y;
        public readonly float ZLength => Max.Z - Min.Z;

        public RealBounds3D(RealBounds xBounds, RealBounds yBounds, RealBounds zBounds)
            : this(new Vector3(xBounds.Min, yBounds.Min, zBounds.Min), new Vector3(xBounds.Max, yBounds.Max, zBounds.Max))
        { }

        public Matrix4x4 CreateExpansionMatrix()
        {
            return IsEmpty ? Matrix4x4.Identity : new Matrix4x4
            {
                M11 = XLength,
                M22 = YLength,
                M33 = ZLength,
                M41 = Min.X,
                M42 = Min.Y,
                M43 = Min.Z,
                M44 = 1
            };
        }
    }
}
