using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Halo4
{
    public static class ShaderParameters
    {
        public const string BlendMap = "blend_map";
        public const string TintMap = "tint_map";
        public const string BaseMap = "base_map";
        public const string BaseMap2 = "basemap";
        public const string DiffuseMap = "diffuse_map";
        public const string DiffuseMap2 = "diffusemap";
        public const string ColorMap = "color_map";
        public const string ColorMap2 = "comap";
        public const string ColorMap3 = "color";
        public const string ColorDetailMap = "color_detail_map";
        public const string NormalMap = "normal_map";
        public const string NormalMap2 = "nmmap";
        public const string NormalMap3 = "normal";
        public const string NormalDetailMap = "normal_detail_map";
        public const string NormalDetailMap2 = "detailnmmap";
        public const string OverlayMap = "overlay_map";
        public const string OverlayDetailMap = "overlay_detail_map";
        public const string SelfIllumMap = "selfillum_map";
        public const string SelfIllumMap2 = "selfillummap";
        public const string SelfIllumModMap = "selfillum_mod_map"; //???
        public const string SpecularMap = "specular_map";
        public const string SpecularMap2 = "spmap";
        public const string FoamTexture = "foam_texture";
        public const string FoamTextureDetail = "foam_texture_detail";
        public const string AlphaMap = "alpha_map";
        public const string AlphaMap2 = "alphamap";
        public const string AlphaMaskMap = "alpha_mask_map";
        public const string ReflectionMap = "reflection_map";
        public const string ReflectionMap2 = "reflmap";
        public const string PccAmountMap = "pcc_amount_map"; //???
        public const string NoiseAMap = "noise_a_map";
        public const string NoiseBMap = "noise_b_map";

        public const string ControlMap = "control_map";
        public const string ControlMap_GlSpRfDf = "control_map_glsprfdf";
        public const string ControlMap_Hair = "control_map_hair";
        public const string ControlMap_SiRf = "control_map_sirf";
        public const string ControlMap_SpDfGl = "control_map_spdfgl";
        public const string ControlMap_SpDfGlRf = "control_map_spdfglrf";
        public const string ControlMap_SpDiGlCm = "control_map_spdiglcm";
        public const string ControlMap_SpGlRf = "control_map_spglrf";
        public const string ControlMap_SpGlSc = "control_map_spglsc";
        public const string ControlMap_SpGlSi = "control_map_spglsi";
        public const string ControlMap_SpGlTr = "control_map_spgltr";

        public const string AlbedoTint = "albedo_tint";
        public const string ColorTint = "color_tint";
        public const string ColorTint2 = "co_tint";
        public const string TintColor = "tint_color";
        public const string BaseColor = "base_color";
        public const string SelfIllumColor = "self_illum_color";
        public const string SpecularColor = "specular_color";
        public const string SpecularTint = "sp_tint";
        public const string ReflectionColor = "reflection_color";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { BlendMap, TextureUsage.BlendMap },
            { TintMap, TextureUsage.Diffuse },
            { BaseMap, TextureUsage.Diffuse },
            { BaseMap2, TextureUsage.Diffuse },
            { DiffuseMap, TextureUsage.Diffuse },
            { DiffuseMap2, TextureUsage.Diffuse },
            { ColorMap, TextureUsage.Diffuse },
            { ColorMap2, TextureUsage.Diffuse },
            { ColorMap3, TextureUsage.Diffuse },
            { ColorDetailMap, TextureUsage.DiffuseDetail },
            { NormalMap, TextureUsage.Normal },
            { NormalMap2, TextureUsage.Normal },
            { NormalMap3, TextureUsage.Normal },
            { NormalDetailMap, TextureUsage.NormalDetail },
            { NormalDetailMap2, TextureUsage.NormalDetail },
            { SelfIllumMap, TextureUsage.SelfIllumination },
            { SelfIllumMap2, TextureUsage.SelfIllumination },
            { SpecularMap, TextureUsage.Specular },
            { SpecularMap2, TextureUsage.Specular },
            { FoamTexture, TextureUsage.Diffuse },
            { FoamTextureDetail, TextureUsage.DiffuseDetail },
            { AlphaMap, TextureUsage.Transparency },
            { AlphaMap2, TextureUsage.Transparency },
            { AlphaMaskMap, TextureUsage.Diffuse },
            { ReflectionMap, TextureUsage.ReflectionCube },
            { ReflectionMap2, TextureUsage.ReflectionCube }
        };

        public static readonly Dictionary<string, string> TintLookup = new()
        {
            { AlbedoTint, TintUsage.Albedo },
            { ColorTint, TintUsage.Albedo },
            { ColorTint2, TintUsage.Albedo },
            { TintColor, TintUsage.Albedo },
            { BaseColor, TintUsage.Albedo },
            { SelfIllumColor, TintUsage.SelfIllumination },
            { SpecularColor, TintUsage.Specular },
            { SpecularTint, TintUsage.Specular }
        };
    }

    internal class MaterialShaderDefinition
    {
        [JsonPropertyName("parameters")]
        public MaterialShaderParameter[] Parameters { get; init; }

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        public class MaterialShaderParameter
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            private string GetDebuggerDisplay() => $"[{Type}] {Name}";
        }
    }

    internal static partial class MaterialHelper
    {
        [GeneratedRegex("^(?:layer_?0?(\\d)_)(\\w+)$")]
        private static partial Regex LayerUsageRegex();

        [GeneratedRegex("^(?:([rgba])_)(\\w+)$")]
        private static partial Regex ChannelUsageRegex();

        [GeneratedRegex("(_0(\\d))(?:_map)?$")]
        private static partial Regex IndexUsageRegex();

        private static readonly Lazy<Dictionary<string, MaterialShaderDefinition>> Halo4MaterialShaderLookup = new(() => JsonSerializer.Deserialize<Dictionary<string, MaterialShaderDefinition>>(Resources.Halo4MaterialShader));
        private static readonly Lazy<Dictionary<string, MaterialShaderDefinition>> Halo2XMaterialShaderLookup = new(() => JsonSerializer.Deserialize<Dictionary<string, MaterialShaderDefinition>>(Resources.MccHalo2XMaterialShader));

        private static (string Usage, ChannelMask BlendChannel) GetBlendUsage(string usage)
        {
            var match = LayerUsageRegex().Match(usage);
            if (match.Success)
            {
                var blendChannel = (ChannelMask)(1 << int.Parse(match.Groups[1].Value));
                return (match.Groups[2].Value, blendChannel);
            }

            match = ChannelUsageRegex().Match(usage);
            if (match.Success)
            {
                var blendChannel = (ChannelMask)(1 << "rgba".IndexOf(match.Groups[1].Value));
                return (match.Groups[2].Value, blendChannel);
            }

            match = IndexUsageRegex().Match(usage);
            if (match.Success)
            {
                var blendChannel = (ChannelMask)(1 << int.Parse(match.Groups[2].Value) - 1);
                return (usage.Replace(match.Groups[1].Value, ""), blendChannel);
            }

            //not a blend usage
            return (usage, ChannelMask.Default);
        }

        private static IEnumerable<(string Usage, RealVector4 Value)> EnumerateFloatConstants(MaterialShaderDefinition shaderDefinition, IReadOnlyList<RealVector4> floatConstants)
        {
            var nextValueIndex = 0;

            foreach (var parameter in shaderDefinition.Parameters.Where(p => p.Type is "color" or "real"))
            {
                var blockIndex = nextValueIndex / 4;
                var offsetInBlock = nextValueIndex % 4;

                //if we reach the end of the Float Constants block, the rest are likely in the Float Vertex Constants block
                if (blockIndex >= floatConstants.Count)
                    break;

                if (parameter.Type == "real")
                {
                    var block = floatConstants[blockIndex];
                    var value = offsetInBlock switch
                    {
                        0 => block.X,
                        1 => block.Y,
                        2 => block.Z,
                        _ => block.W
                    };

                    yield return (parameter.Name, new RealVector4(value, 0, 0, 0));
                    nextValueIndex++;
                }
                else //type == "color"
                {
                    //from what I can tell, color values can only appear in position 0 or 1 - they cannot be split across blocks
                    //if there are still slots left in the current block, they will just be set to 0 and the color will be put in the next block instead
                    if (offsetInBlock > 1)
                    {
                        blockIndex++;
                        offsetInBlock = 0;
                    }

                    //if we reach the end of the Float Constants block, the rest are likely in the Float Vertex Constants block
                    if (blockIndex >= floatConstants.Count)
                        break;

                    var block = floatConstants[blockIndex];
                    var value = offsetInBlock == 0
                        ? new RealVector4(block.X, block.Y, block.Z, 0)
                        : new RealVector4(block.Y, block.Z, block.W, 0);

                    yield return (parameter.Name, value);
                    nextValueIndex += 3;
                }
            }
        }

        private static IEnumerable<TextureMapping> EnumerateControlChannelMappings(string usage, TextureMapping src)
        {
            if (usage == ShaderParameters.ControlMap_Hair)
            {
                yield return src;
                yield break;
            }

            //this takes advantage of the usage strings always being two characters per channel and in order of rgba
            var channelCodes = usage.Replace("control_map_", "").ToLower().Chunk(2).Select(x => new string(x)).ToArray();

            for (var i = 0; i < channelCodes.Length; i++)
            {
                var channelMask = (ChannelMask)(1 << i);
                var channelUsage = channelCodes[i] switch
                {
                    "sp" => TextureUsage.Specular,
                    "df" or "di" => TextureUsage.Diffuse,
                    "gl" => TextureUsage.Other, //gloss?
                    "rf" => TextureUsage.Other, //reflectiveness?
                    _ => TextureUsage.Other //no idea what the rest are
                };

                yield return new TextureMapping
                {
                    Usage = channelUsage,
                    Tiling = src.Tiling,
                    BlendChannel = src.BlendChannel,
                    ChannelMask = channelMask,
                    Texture = src.Texture
                };
            }
        }

        public static bool PopulateTextureMappings(Halo4GeometryArgs args, Dictionary<int, bitmap> bitmapCache, Material material, material shader)
        {
            //not confirmed if the material shaders happen to be configured the same in the betas
            if (args.Cache.Metadata.Game == HaloGame.Halo4 && args.Cache.Metadata.IsPreRelease)
                return false;

            var shaderLookup = args.Cache.Metadata.Game == HaloGame.Halo2X
                ? Halo2XMaterialShaderLookup.Value
                : Halo4MaterialShaderLookup.Value;

            if (!shaderLookup.TryGetValue(shader.BaseShaderReference.Tag.TagName, out var shaderDefinition))
                return false;

            var shaderProps = shader.ShaderProperties[0];

            var textureParams = shaderDefinition.Parameters
                .Where(p => p.Type == "bitmap")
                .Select((p, i) =>
                {
                    var (usage, channelMask) = GetBlendUsage(p.Name);

                    return new
                    {
                        Usage = usage,
                        BlendChannel = channelMask,
                        shaderProps.ShaderMaps[i].BitmapReference.Tag,
                        TileData = shaderProps.TilingData[i]
                    };
                });

            var floatParams = from p in EnumerateFloatConstants(shaderDefinition, shaderProps.FloatConstants)
                              where ShaderParameters.TintLookup.ContainsKey(p.Usage)
                              let usage = GetBlendUsage(p.Usage)
                              select new
                              {
                                  usage.Usage,
                                  usage.BlendChannel,
                                  p.Value
                              };

            material.AlphaMode = shaderProps.BlendMode switch
            {
                BlendMode.Additive => AlphaMode.Add,
                BlendMode.Multiply => AlphaMode.Multiply,
                BlendMode.AlphaBlend or BlendMode.AlphaBlendConstant or BlendMode.AlphaBlendMax or BlendMode.AlphaBlendAdditiveTransparent => AlphaMode.Blend,
                BlendMode.PreMultipliedAlpha => AlphaMode.PreMultiplied,
                _ => AlphaMode.Opaque
            };

            foreach (var texParam in textureParams)
            {
                if (texParam.Tag == null)
                    continue;

                var tagId = texParam.Tag.Id;
                if (!bitmapCache.TryGetValue(tagId, out var bitmap))
                    bitmapCache.Add(tagId, bitmap = texParam.Tag.ReadMetadata<bitmap>());

                var texture = new Texture
                {
                    Id = tagId,
                    ContentProvider = bitmap,
                    Gamma = ((IBitmap)bitmap).GetSubmapGamma(0)
                };

                texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, texParam.Tag.TagName);

                var texMap = new TextureMapping
                {
                    Usage = ShaderParameters.UsageLookup.GetValueOrDefault(texParam.Usage, TextureUsage.Other),
                    Tiling = new Vector2(texParam.TileData.X, texParam.TileData.Y),
                    BlendChannel = texParam.BlendChannel,
                    Texture = texture
                };

                if (texParam.Usage.StartsWith("control_map_"))
                    material.TextureMappings.AddRange(EnumerateControlChannelMappings(texParam.Usage, texMap));
                else
                    material.TextureMappings.Add(texMap);
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

            //TODO: check for specular-from-alpha?

            //add transparency mapping for alpha blending
            if (material.AlphaMode != AlphaMode.Opaque)
            {
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

            return true;
        }
    }
}
