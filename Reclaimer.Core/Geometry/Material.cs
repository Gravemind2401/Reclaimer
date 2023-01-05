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
    }

    public class TextureMapping
    {
        public int Usage { get; set; }
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
        public int Usage { get; set; }
        public Color Color { get; set; }
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
