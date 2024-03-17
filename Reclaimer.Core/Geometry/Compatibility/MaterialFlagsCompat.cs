namespace Reclaimer.Geometry.Compatibility
{
    [Flags, Obsolete("Backwards compatibility for AMF")]
    public enum MaterialFlagsCompat
    {
        None = 0,
        Transparent = 1,
        ColourChange = 2,
        TerrainBlend = 4
    }
}
