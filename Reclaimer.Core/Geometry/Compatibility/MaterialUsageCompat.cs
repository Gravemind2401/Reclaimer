namespace Reclaimer.Geometry.Compatibility
{
    [Obsolete("Backwards compatibility for AMF")]
    public static class MaterialUsageCompat
    {
        public const int Other = int.MinValue;

        public const int BlendMap = -1;
        public const int Diffuse = 0;
        public const int DiffuseDetail = 1;
        public const int ColourChange = 2;
        public const int Normal = 3;
        public const int NormalDetail = 4;
        public const int SelfIllumination = 5;
        public const int Specular = 6;

        public static int GetValue(string value)
        {
            return value switch
            {
                TextureUsage.BlendMap => BlendMap,
                TextureUsage.Diffuse => Diffuse,
                TextureUsage.DiffuseDetail => DiffuseDetail,
                TextureUsage.ColorChange => ColourChange,
                TextureUsage.Normal => Normal,
                TextureUsage.NormalDetail => NormalDetail,
                TextureUsage.SelfIllumination => SelfIllumination,
                TextureUsage.Specular => Specular,
                _ => Other
            };
        }
    }
}
