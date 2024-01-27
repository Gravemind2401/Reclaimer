namespace Adjutant.Geometry
{
    public enum VertexWeights
    {
        None,
        Skinned,
        Rigid
    }

    [Flags]
    public enum MaterialFlags
    {
        None = 0,
        Transparent = 1,
        ColourChange = 2,
        TerrainBlend = 4
    }
}
