using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reclaimer.Blam.Halo2
{
    public static class ShaderParameters
    {
        public const string BlendMap = "blend_map";
        public const string BaseMap = "base_map";
        public const string DetailMap = "detail_map";
        public const string SecondaryDetailMap = "secondary_detail_map";
        public const string OverlayDetailMap = "overlay_detail_map";
        public const string ChangeColorMap = "change_color_map";
        public const string BumpMap = "bump_map";
        public const string SelfIllumMap = "self_illum_map";
        public const string SpecularMap = "specular_map";
        public const string CloudMask = "cloud_mask";
        public const string CloudMap = "cloud_map";
        public const string AlphaMap = "alpha_map";
        public const string AlphaBlendMap = "alpha_blend_map";
        public const string ReflectionMap = "reflection_map";
        public const string EnvironmentMap = "environment_map";

        public const string TintColor = "tint_color";
        public const string SpecularColor = "specular_color";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { BlendMap, TextureUsage.BlendMap },
            { BaseMap, TextureUsage.Diffuse },
            { DetailMap, TextureUsage.DiffuseDetail },
            { SecondaryDetailMap, TextureUsage.DiffuseDetail },
            { OverlayDetailMap, TextureUsage.DiffuseDetail },
            { ChangeColorMap, TextureUsage.ColorChange },
            { BumpMap, TextureUsage.Normal },
            { SelfIllumMap, TextureUsage.SelfIllumination },
            { SpecularMap, TextureUsage.Specular },
            { CloudMask, TextureUsage.Diffuse },
            { CloudMap, TextureUsage.Diffuse },
            { AlphaMap, TextureUsage.Transparency },
            { AlphaBlendMap, TextureUsage.Transparency },
            { ReflectionMap, TextureUsage.ReflectionCube },
            { EnvironmentMap, TextureUsage.ReflectionCube }
        };

        public static readonly Dictionary<string, string> TintLookup = new()
        {
            { TintColor, TintUsage.Albedo },
            { SpecularColor, TintUsage.Specular }
        };
    }

    //not actual Halo2 values - just used in the shader pass json to identify certain material properties
    internal static class ShaderOptionCategories
    {
        public const string AlphaTest = "alpha_test";
        public const string SpecularMask = "specular_mask";
        public const string BlendMode = "blend_mode";
    }

    //not actual Halo2 values - just used in the shader pass json to identify certain material properties
    internal static class ShaderOptionValues
    {
        public const string None = "none";
        public const string Opaque = "opaque";
        public const string Additive = "additive";
        public const string Multiply = "multiply";
        public const string AlphaBlend = "alpha_blend";
        public const string PreMultipliedAlpha = "pre_multiplied_alpha";
        public const string SpecularMaskFromDiffuse = "specular_mask_from_diffuse";
    }

    internal class ShaderPassDefinition
    {
        [JsonPropertyName("options")]
        public Dictionary<string, string> Options { get; init; } = new();

        [JsonPropertyName("bitmaps")]
        public string[] Bitmaps { get; init; }

        [JsonPropertyName("constants")]
        public string[] Constants { get; init; }
    }

    internal static class MaterialHelper
    {
        private static readonly Lazy<Dictionary<string, ShaderPassDefinition>> ShaderPassLookup = new(() => JsonSerializer.Deserialize<Dictionary<string, ShaderPassDefinition>>(Resources.Halo2XboxShaderPass));

        private static void GetTemplateArguments(ShaderTag shader, out Dictionary<string, string> options, out string[] usages, out string[] arguments)
        {
            //this produces a list of "options", "usages" and "arguments" similar to what is available in Halo3's render method templates.
            //in Halo2 those strings dont exist in the compiled tags so we need to figure it out by comparing pre-mapped values with the shader pass references.

            var shaderProps = shader.ShaderProperties[0];

            options = new Dictionary<string, string>();
            usages = new string[shaderProps.Bitmaps.Count];
            arguments = new string[shaderProps.TilingData.Count];

            var templateProps = shaderProps.TemplateReference.Tag.ReadMetadata<ShaderTemplateTag>().ShaderTemplateProperties[0];
            foreach (var passBlock in templateProps.ShaderPasses)
            {
                if (!ShaderPassLookup.Value.TryGetValue(passBlock.ShaderPassReference.Tag.FileName, out var passDefinition))
                    continue;

                foreach (var (key, value) in passDefinition.Options)
                    options.TryAdd(key, value);

                //the shader pass definition json is mapped out based only on the first implementation of each shader pass
                var implementation = templateProps.Implementations[passBlock.ImplementationRange.Index];
                var bitmapsRange = implementation.BitmapRemappingRange;
                var constantsRange = implementation.VertexConstantsRemappingRange;

                for (var i = 0; i < bitmapsRange.Count; i++)
                {
                    if (i >= passDefinition.Bitmaps.Length)
                        break;

                    if (!ValidateDefinitionString(passDefinition.Bitmaps[i]))
                        continue;

                    var remapping = templateProps.Remappings[bitmapsRange.Index + i];
                    usages[remapping.BlockIndex] ??= passDefinition.Bitmaps[i];
                }

                for (var i = 0; i < constantsRange.Count; i++)
                {
                    if (i >= passDefinition.Constants.Length)
                        break;

                    if (!ValidateDefinitionString(passDefinition.Constants[i]))
                        continue;

                    var remapping = templateProps.Remappings[constantsRange.Index + i];
                    arguments[remapping.BlockIndex] ??= passDefinition.Constants[i];
                }
            }

            //this just ignores the values that serve as placeholders and notes
            static bool ValidateDefinitionString(string value)
            {
                return !string.IsNullOrWhiteSpace(value) && !value.StartsWith("?");
            }
        }

        public static void PopulateTextureMappings(Dictionary<int, BitmapTag> bitmapCache, Material material, ShaderTag shader)
        {
            GetTemplateArguments(shader, out var options, out var usages, out var arguments);

            var shaderProps = shader.ShaderProperties[0];

            var textureParams = from index in Enumerable.Range(0, usages.Length)
                                let usage = usages[index]
                                let tileIndex = Array.IndexOf(arguments, usage)
                                where usage != null
                                select new
                                {
                                    Usage = usage,
                                    BlendChannel = default(ChannelMask),
                                    shaderProps.Bitmaps[index].BitmapReference.Tag,
                                    TileData = tileIndex >= 0 ? shaderProps.TilingData[tileIndex] : new RealVector4(1, 1, 1, 1),
                                };

            var floatParams = from index in Enumerable.Range(0, arguments.Length)
                              let usage = arguments[index]
                              where usage != null
                              && !usages.Contains(usage)
                              where ShaderParameters.TintLookup.ContainsKey(usage)
                              select new
                              {
                                  Usage = usage,
                                  BlendChannel = default(ChannelMask),
                                  Value = shaderProps.TilingData[index]
                              };

            if (options.TryGetValue(ShaderOptionCategories.BlendMode, out var blendMode))
            {
                material.AlphaMode = blendMode switch
                {
                    ShaderOptionValues.Additive => AlphaMode.Add,
                    ShaderOptionValues.Multiply => AlphaMode.Multiply,
                    ShaderOptionValues.AlphaBlend => AlphaMode.Blend,
                    ShaderOptionValues.PreMultipliedAlpha => AlphaMode.PreMultiplied,
                    _ => AlphaMode.Opaque
                };
            }

            if (options.TryGetValue(ShaderOptionCategories.AlphaTest, out var alphaTest) && alphaTest != ShaderOptionValues.None)
                material.AlphaMode = AlphaMode.Clip;

            if (string.IsNullOrEmpty(material.AlphaMode))
                material.AlphaMode = AlphaMode.Opaque;

            var baseMap = textureParams.FirstOrDefault(t => t.Usage == ShaderParameters.BaseMap);
            var baseScale = new Vector2(baseMap?.TileData.X ?? 1, baseMap?.TileData.Y ?? 1);

            foreach (var texParam in textureParams)
            {
                if (texParam.Tag == null)
                    continue;

                var tagId = texParam.Tag.Id;
                if (!bitmapCache.TryGetValue(tagId, out var bitmap))
                    bitmapCache.Add(tagId, bitmap = texParam.Tag.ReadMetadata<BitmapTag>());

                var texture = new Texture
                {
                    Id = tagId,
                    ContentProvider = bitmap
                };

                texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, texParam.Tag.TagName);

                var textureScale = new Vector2(texParam.TileData.X, texParam.TileData.Y);
                if (texParam.Usage != ShaderParameters.BaseMap)
                    textureScale *= baseScale;

                material.TextureMappings.Add(new TextureMapping
                {
                    Usage = ShaderParameters.UsageLookup.GetValueOrDefault(texParam.Usage, TextureUsage.Other),
                    Tiling = textureScale,
                    BlendChannel = texParam.BlendChannel,
                    Texture = texture
                });
            }

            foreach (var floatParam in floatParams)
            {
                material.Tints.Add(new MaterialTint
                {
                    Usage = ShaderParameters.TintLookup[floatParam.Usage],
                    BlendChannel = floatParam.BlendChannel,
                    Color = floatParam.Value.ToArgb()
                });
            }

            if (options.GetValueOrDefault(ShaderOptionCategories.SpecularMask) == ShaderOptionValues.SpecularMaskFromDiffuse)
            {
                var diffuse = material.TextureMappings.FirstOrDefault(t => t.Usage == TextureUsage.Diffuse);
                if (diffuse != null)
                {
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Specular,
                        Tiling = diffuse.Tiling,
                        BlendChannel = diffuse.BlendChannel,
                        ChannelMask = ChannelMask.Alpha,
                        Texture = diffuse.Texture
                    });
                }
            }

            //add transparency mapping for alpha blending
            if (material.AlphaMode != AlphaMode.Opaque)
            {
                //should only ever be one diffuse
                var diffuse = material.TextureMappings.FirstOrDefault(t => t.Usage == TextureUsage.Diffuse);
                if (diffuse != null)
                {
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Transparency,
                        Tiling = diffuse.Tiling,
                        BlendChannel = diffuse.BlendChannel,
                        ChannelMask = ChannelMask.Alpha,
                        Texture = diffuse.Texture
                    });
                }
            }
        }
    }
}
