﻿using Reclaimer.Geometry;

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
        public const string AlphaTestMap = "alpha_test_map";
        public const string AlphaMaskMap = "alpha_mask_map";
        public const string EnvironmentMap = "environment_map";

        public const string AlbedoColor = "albedo_color";
        public const string SelfIllumColor = "self_illum_color";
        public const string SpecularTint = "specular_tint";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { BlendMap, TextureUsage.BlendMap },
            { BaseMap, TextureUsage.Diffuse },
            { DetailMap, TextureUsage.DiffuseDetail },
            { DetailMapOverlay, TextureUsage.DiffuseDetail },
            { ChangeColorMap, TextureUsage.ColorChange },
            { BumpMap, TextureUsage.Normal },
            { BumpDetailMap, TextureUsage.NormalDetail },
            { SelfIllumMap, TextureUsage.SelfIllumination },
            { SpecularMap, TextureUsage.Specular },
            { FoamTexture, TextureUsage.Diffuse },
            { AlphaTestMap, TextureUsage.Transparency },
            { AlphaMaskMap, TextureUsage.Diffuse },
            { EnvironmentMap, TextureUsage.ReflectionCube }
        };

        public static readonly Dictionary<string, string> TintLookup = new()
        {
            { AlbedoColor, TintUsage.Albedo },
            { SelfIllumColor, TintUsage.SelfIllumination },
            { SpecularTint, TintUsage.Specular }
        };
    }

    public static class ShaderOptionCategories
    {
        public const string Albedo = "albedo";
        public const string BumpMapping = "bump_mapping";
        public const string AlphaTest = "alpha_test";
        public const string MaterialModel = "material_model";
        public const string SpecularMask = "specular_mask";
        public const string EnvironmentMapping = "environment_mapping";
        public const string SelfIllumination = "self_illumination";
        public const string BlendMode = "blend_mode";
        public const string Parallax = "parallax";
        public const string Misc = "misc";
        public const string Distortion = "distortion";
        public const string SoftFade = "soft_fade";
        public const string MiscAttrAnimation = "misc_attr_animation";
    }

    public static class ShaderOptions
    {
        public static class Albedo
        {
            public const string DiffuseOnly = "diffuse_only";
            public const string Palettized = "palettized";
            public const string PalettizedPlusAlpha = "palettized_plus_alpha";
            public const string DiffusePlusAlpha = "diffuse_plus_alpha";
            public const string EmblemChangeColor = "emblem_change_color";
            public const string ChangeColor = "change_color";
            public const string DiffusePlusAlphaMask = "diffuse_plus_alpha_mask";
            public const string PalettizedPlusAlphaMask = "palettized_plus_alpha_mask";
            public const string VectorAlpha = "vector_alpha";
            public const string VectorAlphaDropShadow = "vector_alpha_drop_shadow";
        }

        public static class BumpMapping
        {
            public const string Off = "off";
            public const string Standard = "standard";
            public const string Detail = "detail";
            public const string DetailMasked = "detail_masked";
            public const string DetailPlusDetailMasked = "detail_plus_detail_masked";
            public const string DetailUnorm = "detail_unorm";
        }

        public static class AlphaTest
        {
            public const string None = "none";
            public const string Simple = "simple";
        }

        public static class SpecularMask
        {
            public const string NoSpecularMask = "no_specular_mask";
            public const string SpecularMaskFromDiffuse = "specular_mask_from_diffuse";
            public const string SpecularMaskFromTexture = "specular_mask_from_texture";
            public const string SpecularMaskFromColorTexture = "specular_mask_from_color_texture";
        }

        public static class BlendMode
        {
            public const string Opaque = "opaque";
            public const string Additive = "additive";
            public const string Multiply = "multiply";
            public const string AlphaBlend = "alpha_blend";
            public const string DoubleMultiply = "double_multiply";
            public const string PreMultipliedAlpha = "pre_multiplied_alpha";
        }
    }

    public static class TerrainShaderOptionCategories
    {
        public const string Blending = "blending";
        public const string EnvironmentMap = "environment_map";
        public const string Material0 = "material_0";
        public const string Material1 = "material_1";
        public const string Material2 = "material_2";
        public const string Material3 = "material_3";
    }

    public static class TerrainShaderOptions
    {
        public static class MaterialN
        {
            public const string Off = "off";
            public const string DiffuseOnly = "diffuse_only";
            public const string DiffusePlusSpecular = "diffuse_plus_specular";
            //plus more
        }
    }
}
