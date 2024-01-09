using Reclaimer.Geometry;

namespace Reclaimer.Blam.Common.Gen3
{
    public static class ShaderParameters
    {
        public const string BlendMap = "blend_map";
        public const string BaseMap = "base_map";
        public const string DetailMap = "detail_map";
        public const string DetailMapOverlay = "detail_map_overlay";
        public const string ChangeColorMap = "change_color_map";
        public const string BumpMap = "bump_map";
        public const string BumpDetailMap = "bump_detail_map";
        public const string SelfIllumMap = "self_illum_map";
        public const string SpecularMap = "specular_map";
        public const string FoamTexture = "foam_texture";

        public const string AlbedoColor = "albedo_color";
        public const string SelfIllumColor = "self_illum_color";
        public const string SpecularTint = "specular_tint";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { BlendMap, MaterialUsage.BlendMap },
            { BaseMap, MaterialUsage.Diffuse },
            { DetailMap, MaterialUsage.DiffuseDetail },
            { DetailMapOverlay, MaterialUsage.DiffuseDetail },
            { ChangeColorMap, MaterialUsage.ColorChange },
            { BumpMap, MaterialUsage.Normal },
            { BumpDetailMap, MaterialUsage.NormalDetail },
            { SelfIllumMap, MaterialUsage.SelfIllumination },
            { SpecularMap, MaterialUsage.Specular },
            { FoamTexture, MaterialUsage.Diffuse }
        };

        public static readonly Dictionary<string, string> TintLookup = new()
        {
            { AlbedoColor, TintUsage.Albedo },
            { SelfIllumColor, TintUsage.SelfIllumination },
            { SpecularTint, TintUsage.Specular }
        };
    }
}
