namespace Reclaimer.Geometry.Compatibility
{
    [Obsolete("Backwards compatibility for AMF")]
    public static class TintUsageCompat
    {
        public const int Other = int.MinValue;

        public const int Albedo = 0;
        public const int SelfIllumination = 1;
        public const int Specular = 2;

        public static int GetValue(string value)
        {
            return value switch
            {
                TintUsage.Albedo => Albedo,
                TintUsage.SelfIllumination => SelfIllumination,
                TintUsage.Specular => Specular,
                _ => Other
            };
        }
    }
}
