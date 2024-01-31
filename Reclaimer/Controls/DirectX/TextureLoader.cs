using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using System.IO;

using Material = HelixToolkit.Wpf.SharpDX.Material;

namespace Reclaimer.Controls.DirectX
{
    public partial class ModelViewer
    {
        private sealed class TextureLoader
        {
            private static readonly Material ErrorMaterial = DiffuseMaterials.Gold;

            private readonly Dictionary<int, Material> materials = new();
            private readonly Dictionary<int, TextureModel> textureLookup = new();

            public Material this[int? id] => id.HasValue ? materials.GetValueOrDefault(id.Value, ErrorMaterial) : ErrorMaterial;

            public TextureLoader(Scene scene)
            {
                foreach (var mat in scene.EnumerateMaterials())
                {
                    try
                    {
                        var diffuse = mat.TextureMappings.FirstOrDefault(m => m.Usage == TextureUsage.Diffuse);
                        if (diffuse == null)
                            continue;

                        var material = new DiffuseMaterial
                        {
                            DiffuseMap = GetTextureForBitmap(diffuse.Texture)
                        };

                        material.Freeze();
                        materials.Add(mat.Id, material);
                    }
                    catch { }
                }
            }

            private TextureModel GetTextureForBitmap(Texture bitmap)
            {
                if (bitmap == null)
                    return null;

                if (!textureLookup.TryGetValue(bitmap.Id, out var textureModel))
                {
                    var args = new DdsOutputArgs(DecompressOptions.Bgr24);
                    var stream = new MemoryStream();
                    bitmap.GetDds().WriteToStream(stream, System.Drawing.Imaging.ImageFormat.Png, args);
                    textureLookup.Add(bitmap.Id, textureModel = new TextureModel(stream));
                }

                return textureModel;
            }
        }
    }
}
