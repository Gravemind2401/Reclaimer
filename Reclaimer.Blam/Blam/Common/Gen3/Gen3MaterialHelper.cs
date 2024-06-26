﻿using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Compatibility;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Utilities;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Common.Gen3
{
    internal static partial class Gen3MaterialHelper
    {
        [GeneratedRegex("^(\\w+?)(?:_m_(\\d))?$")]
        private static partial Regex UsageRegex();

        public static void PopulateTextureMappings<TBitmap>(Dictionary<int, TBitmap> bitmapCache, Material material, string tagClassCode, Dictionary<string, string> shaderOptions, BlockCollection<StringId> usages, BlockCollection<StringId> arguments, BlockCollection<RealVector4> tilingData, Func<int, IIndexItem> tagFunc)
            where TBitmap : IContentProvider<IBitmap>, IBitmap
        {
            var textureParams = from index in Enumerable.Range(0, usages.Count)
                                let usage = usages[index]
                                let tileIndex = arguments.IndexOf(usage)
                                let match = UsageRegex().Match(usage)
                                select new
                                {
                                    Usage = match.Groups[1].Value,
                                    BlendChannel = match.Groups[2].Success ? (ChannelMask)(1 << int.Parse(match.Groups[2].Value)) : default,
                                    Tag = tagFunc(index),
                                    TileData = tileIndex >= 0 ? tilingData[tileIndex] : new RealVector4(1, 1, 1, 1),
                                };

            var floatParams = from index in Enumerable.Range(0, arguments.Count)
                              let usage = arguments[index]
                              where !usages.Contains(usage)
                              let match = UsageRegex().Match(usage)
                              where ShaderParameters.TintLookup.ContainsKey(match.Groups[1].Value)
                              select new
                              {
                                  Usage = match.Groups[1].Value,
                                  BlendChannel = match.Groups[2].Success ? (ChannelMask)(1 << int.Parse(match.Groups[2].Value)) : default,
                                  Value = tilingData[index]
                              };

            if (shaderOptions.TryGetValue(ShaderOptionCategories.BlendMode, out var blendMode))
            {
                material.AlphaMode = blendMode switch
                {
                    ShaderOptions.BlendMode.Additive => AlphaMode.Add,
                    ShaderOptions.BlendMode.Multiply => AlphaMode.Multiply,
                    ShaderOptions.BlendMode.AlphaBlend => AlphaMode.Blend,
                    ShaderOptions.BlendMode.PreMultipliedAlpha => AlphaMode.PreMultiplied,
                    _ => AlphaMode.Opaque
                };
            }

            if (shaderOptions.TryGetValue(ShaderOptionCategories.AlphaTest, out var alphaTest) && alphaTest != ShaderOptions.AlphaTest.None)
                material.AlphaMode = AlphaMode.Clip;

            if (string.IsNullOrEmpty(material.AlphaMode))
                material.AlphaMode = AlphaMode.Opaque;

            foreach (var texParam in textureParams)
            {
                if (texParam.Tag == null)
                    continue;

                var tagId = texParam.Tag.Id;
                if (!bitmapCache.TryGetValue(tagId, out var bitmap))
                    bitmapCache.Add(tagId, bitmap = texParam.Tag.ReadMetadata<TBitmap>());

                var texture = new Texture
                {
                    Id = tagId,
                    ContentProvider = bitmap,
                    Gamma = bitmap.GetSubmapGamma(0)
                };

                texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, texParam.Tag.TagName);

                material.TextureMappings.Add(new TextureMapping
                {
                    Usage = ShaderParameters.UsageLookup.GetValueOrDefault(texParam.Usage, TextureUsage.Other),
                    Tiling = new Vector2(texParam.TileData.X, texParam.TileData.Y),
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

            //check for specular-from-alpha on regular materials
            if (shaderOptions.GetValueOrDefault(ShaderOptionCategories.SpecularMask) == ShaderOptions.SpecularMask.SpecularMaskFromDiffuse)
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

            //check for specular-from-alpha on terrain diffuse materials
            for (var i = 0; i < 4; i++)
            {
                if (shaderOptions.GetValueOrDefault($"material_{i}") != TerrainShaderOptions.MaterialN.DiffusePlusSpecular)
                    continue;

                var channel = (ChannelMask)(1 << i);
                var diffuse = material.TextureMappings.FirstOrDefault(t => t.Usage == TextureUsage.Diffuse && t.BlendChannel == channel);
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
                shaderOptions.TryGetValue(ShaderOptionCategories.Albedo, out var albedoOption);
                var targetUsage = albedoOption switch
                {
                    ShaderOptions.Albedo.ChangeColor or ShaderOptions.Albedo.EmblemChangeColor => TextureUsage.ColorChange,
                    _ => TextureUsage.Diffuse
                };

                //should only ever be one diffuse if alpha blending is being used (terrain shaders have no blend mode parameter)
                var albedo = material.TextureMappings.FirstOrDefault(t => t.Usage == targetUsage);
                if (albedo != null)
                {
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Transparency,
                        Tiling = albedo.Tiling,
                        BlendChannel = albedo.BlendChannel,
                        ChannelMask = ChannelMask.Alpha,
                        Texture = albedo.Texture
                    });
                }
            }

            PopulateLegacyFlags(material, tagClassCode);
        }

        [Obsolete("backwards compatibility for AMF")]
        public static void PopulateLegacyFlags(Material material, string tagClassCode)
        {
            if (tagClassCode == "rmtr")
                material.LegacyFlags |= MaterialFlagsCompat.TerrainBlend;
            else if (tagClassCode != "rmsh")
                material.LegacyFlags |= MaterialFlagsCompat.Transparent;

            if (material.TextureMappings.Any(m => m.Usage == TextureUsage.ColorChange) && !material.TextureMappings.Any(m => m.Usage == TextureUsage.Diffuse))
                material.LegacyFlags |= MaterialFlagsCompat.ColourChange;
        }
    }
}
