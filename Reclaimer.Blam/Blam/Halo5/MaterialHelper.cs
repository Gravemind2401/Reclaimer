using Reclaimer.Blam.Common;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public static class ShaderParameters
    {
        public const string LayerMaskMap = "layer_mask_map";
        public const string BaseColorMap = "base_color_map";
        public const string BaseNormalMap = "base_normal_map";
        public const string BaseNormalDetailMap = "base_normal_detail_map";
        public const string SurfaceColorMap = "surface_color_map";
        public const string SurfaceColorMapWrinkle = "surface_color_map_wrinkle";
        public const string DiffuseMap = "diffuse_map";
        public const string ColorMap = "color_map";
        public const string ColorDetailMap = "color_detail_map";
        public const string NormalMap = "normal_map";
        public const string NormalMapWrinkle = "normal_map_wrinkle";
        public const string NormalDetailMap = "normal_detail_map";
        public const string NormalInnerMap = "normal_inner_map";
        public const string NormalOuterMap = "normal_outer_map";
        public const string MacroNormalMap = "macro_normal_map";
        public const string EyeNormalMap = "eye_normal_map";
        public const string CorneaNormalMap = "cornea_normal_map";
        public const string SelfIllumMap = "self_illum_map";
        public const string SelfIllumMap2 = "selfillum_map";
        public const string SpecularMap = "specular_map";
        public const string SpecDetailMap = "spec_detail_map";
        public const string SpecDetailMap2 = "specdetail_map";
        public const string SmoothnessMap = "smoothness_map";
        public const string RoughnessMap = "roughness_map";
        public const string ReflectivityMap = "reflectivity_map";
        public const string AlphaMap = "alpha_map";
        public const string ReflectionMap = "reflection_map";

        public const string BaseControlMap = "base_control_map";
        public const string BuildupControlMap = "buildup_control_map";
        public const string DecayControlMap = "decay_control_map";
        public const string EyeControlMap = "eye_control_map";
        public const string HairControlMap = "hair_control_map";

        public const string ControlMap = "control_map";
        public const string ControlMap_SpGlFm = "control_map_spglfm";
        public const string ControlMap_SpGlRf = "control_map_spglrf";
        public const string ControlMap_SpGlSc = "control_map_spglsc";
        public const string ControlMap_SpGlSi = "control_map_spglsi";
        public const string ControlMap_SpGlTr = "control_map_spgltr";

        public const string AlbedoTint = "albedo_tint";
        public const string ColorTint = "color_tint";
        public const string DiffuseColor = "diffuse_color";
        public const string SurfaceColor = "surface_color";
        public const string SurfaceColorTint = "surface_color_tint";
        public const string TintColor = "tint_color";
        public const string BaseColor = "base_color";
        public const string SpecularColor = "specular_color";
        public const string SpecularTint = "specular_tint";
        public const string ReflectionColor = "reflection_color";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { LayerMaskMap, TextureUsage.BlendMap },
            { BaseColorMap, TextureUsage.Diffuse },
            { DiffuseMap, TextureUsage.Diffuse },
            { SurfaceColorMap, TextureUsage.Diffuse },
            { ColorMap, TextureUsage.Diffuse },
            { ColorDetailMap, TextureUsage.DiffuseDetail },
            { NormalMap, TextureUsage.Normal },
            { NormalOuterMap, TextureUsage.Normal },
            { MacroNormalMap, TextureUsage.Normal },
            { EyeNormalMap, TextureUsage.Normal },
            { CorneaNormalMap, TextureUsage.Normal },
            { BaseNormalDetailMap, TextureUsage.NormalDetail },
            { NormalDetailMap, TextureUsage.NormalDetail },
            { NormalMapWrinkle, TextureUsage.NormalDetail },
            { NormalInnerMap, TextureUsage.NormalDetail },
            { SelfIllumMap, TextureUsage.SelfIllumination },
            { SelfIllumMap2, TextureUsage.SelfIllumination },
            { SpecularMap, TextureUsage.Specular },
            { AlphaMap, TextureUsage.Transparency },
            { ReflectionMap, TextureUsage.ReflectionCube }
        };

        public static readonly Dictionary<string, string> TintLookup = new()
        {
            { AlbedoTint, TintUsage.Albedo },
            { ColorTint, TintUsage.Albedo },
            { DiffuseColor, TintUsage.Albedo },
            { SurfaceColor, TintUsage.Albedo },
            { SurfaceColorTint, TintUsage.Albedo },
            { TintColor, TintUsage.Albedo },
            { BaseColor, TintUsage.Albedo },
            { SpecularColor, TintUsage.Specular },
            { SpecularTint, TintUsage.Specular }
        };
    }

    internal static class MaterialHelper
    {
        private static (string Usage, ChannelMask BlendChannel) GetBlendUsage(string usage)
        {
            //TODO: terrain blend support
            return (usage, ChannelMask.Default);
        }

        private static IEnumerable<TextureMapping> EnumerateControlChannelMappings(string usage, TextureMapping src)
        {
            if (!usage.StartsWith("control_map_"))
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

        public static bool PopulateTextureMappings(Dictionary<int, BitmapTag> bitmapCache, Material material, MaterialTag shader)
        {
            if (!shader.MaterialParameters.Any())
                return false;

            var textureParams = shader.MaterialParameters
                .Where(p => p.ParameterType == ParameterType.Bitmap && p.BitmapReference.Tag != null)
                .Select((p, i) =>
                {
                    var (usage, channelMask) = GetBlendUsage(p.ParameterName.Value);

                    return new
                    {
                        Usage = usage,
                        BlendChannel = channelMask,
                        p.BitmapReference.Tag,
                        TileData = new RealVector2(p.RealValue, p.VectorValue.X) //are these aligned correctly?
                    };
                });

            var floatParams = from p in shader.MaterialParameters
                              where p.ParameterType == ParameterType.Color
                              let u = GetBlendUsage(p.ParameterName.Value)
                              where ShaderParameters.TintLookup.ContainsKey(u.Usage)
                              select new
                              {
                                  u.Usage,
                                  u.BlendChannel,
                                  Value = new RealVector4(p.ColorValue.Y, p.ColorValue.Z, p.ColorValue.W, p.ColorValue.X) //argb -> rgba
                              };

            var shaderProps = shader.PostprocessDefinitions[0];

            material.AlphaMode = shaderProps.AlphaBlendMode switch
            {
                AlphaBlendMode.Additive => AlphaMode.Add,
                AlphaBlendMode.Multiply => AlphaMode.Multiply,
                AlphaBlendMode.AlphaBlend or AlphaBlendMode.AlphaBlendConstant or AlphaBlendMode.AlphaBlendMax or AlphaBlendMode.AlphaBlendAdditiveTransparent => AlphaMode.Blend,
                AlphaBlendMode.PreMultipliedAlpha => AlphaMode.PreMultiplied,
                _ => AlphaMode.Opaque
            };

            foreach (var texParam in textureParams)
            {
                if (texParam.Tag == null)
                    continue;

                var tagId = texParam.Tag.GlobalTagId;
                if (!bitmapCache.TryGetValue(tagId, out var bitmap))
                    bitmapCache.Add(tagId, bitmap = texParam.Tag.ReadMetadata<BitmapTag>());

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
