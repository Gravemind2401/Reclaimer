using Adjutant.Geometry;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reclaimer.Controls.DirectX
{
    public partial class ModelViewer
    {
        private sealed class TextureLoader
        {
            private static readonly Material ErrorMaterial = DiffuseMaterials.Gold;

            private readonly Dictionary<int, Material> materials = new();
            private readonly Dictionary<int, TextureModel> textureLookup = new();

            public Material this[int matIndex] => materials.GetValueOrDefault(matIndex, ErrorMaterial);

            public TextureLoader(IGeometryModel model)
            {
                var indexes = model.Meshes.SelectMany(m => m.Submeshes)
                    .Select(s => s.MaterialIndex).Distinct();

                foreach (var i in indexes)
                {
                    var mat = model.Materials.ElementAtOrDefault(i);
                    if (mat == null)
                        continue;

                    try
                    {
                        var diffuse = mat.Submaterials.FirstOrDefault(m => m.Usage == MaterialUsage.Diffuse);
                        if (diffuse == null)
                            continue;

                        var material = new DiffuseMaterial
                        {
                            DiffuseMap = GetTextureForBitmap(diffuse.Bitmap)
                        };

                        material.Freeze();
                        materials.Add(i, material);
                    }
                    catch { }
                }
            }

            private TextureModel GetTextureForBitmap(IBitmap bitmap)
            {
                if (bitmap == null)
                    return null;

                if (!textureLookup.TryGetValue(bitmap.Id, out var textureModel))
                {
                    var args = new DdsOutputArgs(DecompressOptions.Bgr24);
                    var stream = new MemoryStream();
                    bitmap.ToDds(0).WriteToStream(stream, System.Drawing.Imaging.ImageFormat.Png, args);
                    textureLookup.Add(bitmap.Id, textureModel = new TextureModel(stream));
                }

                return textureModel;
            }
        }
    }
}
