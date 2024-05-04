using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo1
{
    internal static class Halo1Common
    {
        public static void PopulateTextureMappings(Material material, TagReference shaderRefTagRef, DependencyReader reader)
        {
            if (shaderRefTagRef.Tag == null)
                return;

            if (shaderRefTagRef.Tag.ClassCode == "soso")
                PopulateShaderModel();
            else if (shaderRefTagRef.Tag.ClassCode == "senv")
                PopulateShaderEnvironment();
            else
                PopulateGeneric();

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

            return;

            void PopulateShaderModel()
            {
                var shader = shaderRefTagRef.Tag.ReadMetadata<shader_model>();

                if (shader.Flags.HasFlag(ShaderModelFlags.AlphaBlendedDecal))
                    material.AlphaMode = AlphaMode.Multiply;
                else if (shader.Flags.HasFlag(ShaderModelFlags.NotAlphaTested))
                    material.AlphaMode = AlphaMode.Opaque;
                else
                    material.AlphaMode = AlphaMode.Clip;

                var diffuse = AppendTextureMapping(shader.BaseMap.Tag, TextureUsage.Diffuse);
                if (diffuse != null)
                    diffuse.Tiling = CreateScale(shader.BaseMapUScale, shader.BaseMapVScale);
                
                var detail = AppendTextureMapping(shader.DetailMap.Tag, TextureUsage.DiffuseDetail);
                if (detail != null)
                    detail.Tiling = CreateScale(shader.DetailMapScale, shader.DetailMapScale) * CreateScale(1, shader.DetailMapVScale);

                AppendTextureMapping(shader.ReflectionCubeMap.Tag, TextureUsage.ReflectionCube);

                var multiTextue = CreateTexture(shader.MultipurposeMap.Tag);
                if (multiTextue == null)
                    return;

                //in order of RGBA
                var usages = shader.Flags.HasFlag(ShaderModelFlags.MultipurposeMapUsesOGXboxOrder)
                    ? new[] { TextureUsage.Specular, TextureUsage.SelfIllumination, TextureUsage.ColorChange, TextureUsage.Other }
                    : new[] { TextureUsage.Other, TextureUsage.SelfIllumination, TextureUsage.Specular, TextureUsage.ColorChange };

                for (var i = 0; i < 4; i++)
                {
                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = usages[i],
                        Tiling = Vector2.One,
                        Texture = multiTextue,
                        ChannelMask = (ChannelMask)(1 << i)
                    });
                }
            }

            void PopulateShaderEnvironment()
            {
                var shader = shaderRefTagRef.Tag.ReadMetadata<shader_environment>();

                material.AlphaMode = shader.Flags.HasFlag(ShaderEnvironmentFlags.AlphaTested)
                    ? AlphaMode.Clip
                    : AlphaMode.Opaque;

                AppendTextureMapping(shader.BaseMap.Tag, TextureUsage.Diffuse);

                var detail = AppendTextureMapping(shader.PrimaryDetailMap.Tag, TextureUsage.DiffuseDetail);
                if (detail != null)
                    detail.Tiling = CreateScale(shader.PrimaryDetailMapScale, shader.PrimaryDetailMapScale);

                detail = AppendTextureMapping(shader.SecondaryDetailMap.Tag, TextureUsage.DiffuseDetail);
                if (detail != null)
                    detail.Tiling = CreateScale(shader.SecondaryDetailMapScale, shader.SecondaryDetailMapScale);

                detail = AppendTextureMapping(shader.MicroDetailMap.Tag, TextureUsage.DiffuseDetail);
                if (detail != null)
                    detail.Tiling = CreateScale(shader.MicroDetailMapScale, shader.MicroDetailMapScale);

                var bump = AppendTextureMapping(shader.BumpMap.Tag, TextureUsage.Normal);
                if (bump != null)
                    bump.Tiling = CreateScale(shader.BumpMapScale, shader.BumpMapScale);

                AppendTextureMapping(shader.ReflectionCubeMap.Tag, TextureUsage.ReflectionCube);
            }

            void PopulateGeneric()
            {
                var offset = shaderRefTagRef.Tag.ClassCode switch
                {
                    "sgla" => 356,
                    "schi" => 228,
                    "scex" => 900,
                    "swat" or "smet" => 88,
                    _ => default
                };

                if (offset == default)
                    return;

                try
                {
                    reader.Seek(shaderRefTagRef.Tag.MetaPointer.Address + offset, SeekOrigin.Begin);

                    var bitmId = reader.ReadInt16();
                    if (bitmId == -1)
                        return;

                    var bitmTag = shaderRefTagRef.Tag.CacheFile.TagIndex[bitmId];
                    AppendTextureMapping(bitmTag, TextureUsage.Diffuse);
                    material.AlphaMode = AlphaMode.Multiply;
                }
                catch { }
            }

            static Texture CreateTexture(IIndexItem bitmapTag)
            {
                if (bitmapTag == null)
                    return null;

                var texture = new Texture
                {
                    Id = bitmapTag.Id,
                    ContentProvider = bitmapTag.ReadMetadata<bitmap>()
                };

                texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, bitmapTag.TagName);

                return texture;
            }

            TextureMapping AppendTextureMapping(IIndexItem bitmapTag, string usage)
            {
                var texture = CreateTexture(bitmapTag);
                if (texture == null)
                    return null;

                var tmap = new TextureMapping
                {
                    Usage = usage,
                    Tiling = Vector2.One,
                    Texture = texture
                };

                material.TextureMappings.Add(tmap);
                return tmap;
            }

            static Vector2 CreateScale(in float u, in float v)
            {
                return new Vector2(u == 0 ? 1 : u, v == 0 ? 1 : v);
            }
        }

        public static IEnumerable<Material> GetMaterials(IEnumerable<TagReference> shaderRefs, DependencyReader reader)
        {
            foreach (var shaderRef in shaderRefs)
            {
                if (shaderRef.Tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = shaderRef.Tag.Id,
                    Name = shaderRef.Tag.FileName
                };

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, shaderRef.Tag.TagName);
                PopulateTextureMappings(material, shaderRef, reader);

                yield return material;
            }
        }
    }
}
