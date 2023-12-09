using Reclaimer.Drawing;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace Reclaimer.Geometry
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Material
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TextureMapping> TextureMappings { get; } = new();
        public List<MaterialTint> Tints { get; } = new();

        [Obsolete("legacy")]
        public int Flags { get; set; }
    }

    public class TextureMapping
    {
        public string Usage { get; set; }
        public Texture Texture { get; set; }
        public Vector2 Tiling { get; set; } = Vector2.One;
        public ChannelMask ChannelMask { get; set; }
    }

    public class Texture
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Func<DdsImage> GetDds { get; set; }
    }

    public class MaterialTint
    {
        public string Usage { get; set; }
        public Color Color { get; set; }
    }

    public static class MaterialUsage
    {
        public const string BlendMap = "blend";
        public const string Diffuse = "diffuse";
        public const string DiffuseDetail = "diffuse_detail";
        public const string ColorChange = "color_change";
        public const string Normal = "bump";
        public const string NormalDetail = "bump_detail";
        public const string SelfIllumination = "self_illum";
        public const string Specular = "specular";
    }

    public static class TintUsage
    {
        public const string Albedo = "albedo";
        public const string SelfIllumination = "self_illum";
        public const string Specular = "specular";
    }

    [Flags]
    public enum ChannelMask
    {
        Default = 0,

        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3,

        NonTransparent = Red | Green | Blue
    }
}
