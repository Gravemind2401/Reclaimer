using Adjutant.Geometry;

namespace Reclaimer.Blam.Common
{
    internal static class BlamConstants
    {
        public const string SbspClustersGroupName = "<Clusters>";
        public const string ModelInstancesGroupName = "<Instances>";

        public const string ScenarioBspGroupName = "scenario_structure_bsps";
        public const string ScenarioSkyGroupName = "skies";

        //1 world unit = 10 feet
        public const float Gen3UnitScale = 10 * Geometry.StandardUnits.Feet;

        public static class Gen3Materials
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

            public static readonly Dictionary<string, MaterialUsage> UsageLookup = new()
            {
                { BlendMap, MaterialUsage.BlendMap },
                { BaseMap, MaterialUsage.Diffuse },
                { DetailMap, MaterialUsage.DiffuseDetail },
                { DetailMapOverlay, MaterialUsage.DiffuseDetail },
                { ChangeColorMap, MaterialUsage.ColourChange },
                { BumpMap, MaterialUsage.Normal },
                { BumpDetailMap, MaterialUsage.NormalDetail },
                { SelfIllumMap, MaterialUsage.SelfIllumination },
                { SpecularMap, MaterialUsage.Specular },
                { FoamTexture, MaterialUsage.Diffuse }
            };

            public static readonly Dictionary<string, TintUsage> TintLookup = new()
            {
                { AlbedoColor, TintUsage.Albedo },
                { SelfIllumColor, TintUsage.SelfIllumination },
                { SpecularTint, TintUsage.Specular }
            };
        }
    }
}
