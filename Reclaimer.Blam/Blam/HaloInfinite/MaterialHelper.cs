using Reclaimer.Blam.Common;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public static class ShaderParameters
    {
        public const string NormalMap = "2142563353";
        public const string Mask0Texture = "-1869712910";
        public const string Mask1Texture = "-1677269129";
        public const string ASGControl = "-447337164";
        public const string UITexture = "-339492952";
        public const string DecalMask = "-1898583093";
        public const string Conemap = "863316755";
        public const string DecalNormal = "-1654860010";
        public const string DecalControl = "-1180015629";
        public const string AnimatedUITexture = "-1724850299";
        public const string ScreenControl = "-695457752";
        public const string WeaponDecalNormal = "723636081";
        public const string WeaponDecalControl = "-699244700";

        public static readonly Dictionary<string, string> UsageLookup = new()
        {
            { NormalMap, TextureUsage.Normal },
            { Mask0Texture, TextureUsage.Other },
            { Mask1Texture, TextureUsage.Other },
            { ASGControl, TextureUsage.Diffuse },
            { UITexture, TextureUsage.Diffuse },
            { DecalMask, TextureUsage.Diffuse },
            { Conemap, TextureUsage.Other },
            { DecalNormal, TextureUsage.Normal },
            { DecalControl, TextureUsage.Other },
            { AnimatedUITexture, TextureUsage.Diffuse },
            { ScreenControl, TextureUsage.Diffuse },
            { WeaponDecalNormal, TextureUsage.Normal },
            { WeaponDecalControl, TextureUsage.Diffuse }
        };
    }

    internal static class MaterialHelper
    {
        public static bool PopulateTextureMappings(Dictionary<int, bitmap> bitmapCache, Material material, material shader)
        {
            if (!shader.MaterialParameters.Any())
                return false;

            var textureParams = shader.MaterialParameters
                .Where(p => p.ParameterType == ParameterType.Bitmap && p.Bitmap.Tag != null)
                .Select((p, i) =>
                {
                    return new
                    {
                        Usage = p.ParameterName.Value,
                        BlendChannel = ChannelMask.Default,
                        p.Bitmap.Tag,
                        TileData = new RealVector2(p.Real, p.Vector.X)
                    };
                });

            var shaderProps = shader.PostprocessDefinitions[0];

            material.AlphaMode = shaderProps.AlphaBlendMode switch
            {
                Halo5.AlphaBlendMode.Additive => AlphaMode.Add,
                Halo5.AlphaBlendMode.Multiply => AlphaMode.Multiply,
                Halo5.AlphaBlendMode.AlphaBlend or Halo5.AlphaBlendMode.AlphaBlendConstant or Halo5.AlphaBlendMode.AlphaBlendMax or Halo5.AlphaBlendMode.AlphaBlendAdditiveTransparent => AlphaMode.Blend,
                Halo5.AlphaBlendMode.PreMultipliedAlpha => AlphaMode.PreMultiplied,
                _ => AlphaMode.Opaque
            };

            foreach (var texParam in textureParams)
            {
                if (texParam.Tag == null)
                    continue;

                var tagId = texParam.Tag.GlobalTagId;
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

                material.TextureMappings.Add(texMap);
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
