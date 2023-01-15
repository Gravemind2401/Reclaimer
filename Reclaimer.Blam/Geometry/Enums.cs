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

    public enum MaterialUsage
    {
        BlendMap = -1,
        Diffuse = 0,
        DiffuseDetail = 1,
        ColourChange = 2,
        Normal = 3,
        NormalDetail = 4,
        SelfIllumination = 5,
        Specular = 6
    }

    public enum TintUsage
    {
        Albedo,
        SelfIllumination,
        Specular
    }
}
