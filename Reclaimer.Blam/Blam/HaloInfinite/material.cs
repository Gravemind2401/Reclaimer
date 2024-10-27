using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class material
    {
        [Offset(16)]
        public TagReferenceGen5 ShaderReference { get; set; }

        [Offset(44)]
        public BlockCollection<MaterialParameterBlock> MaterialParameters { get; set; }

        [Offset(64)]
        public BlockCollection<PostprocessDefinitionBlock> PostprocessDefinitions { get; set; }

        [Offset(116)]
        public BlockCollection<StyleBlock> Styles { get; set; }
    }

    [FixedSize(160)]
    public class PostprocessDefinitionBlock
    {
        [Offset(0)]
        public BlockCollection<TexturesBlock> Textures { get; set; }

        [Offset(64)]
        public Halo5.AlphaBlendMode AlphaBlendMode { get; set; }
    }

    [FixedSize(156)]
    public class MaterialParameterBlock
    {
        [Offset(0)]
        public StringHashGen5 ParameterName { get; set; }

        [Offset(4)]
        public ParameterType ParameterType { get; set; }

        [Offset(8)]
        public TagReferenceGen5 Bitmap { get; set; }

        [Offset(36)]
        public RealVector4 Color { get; set; }

        [Offset(52)]
        public float Real { get; set; }

        [Offset(56)]
        public RealVector3 Vector { get; set; }

        [Offset(68)]
        public int IntBool { get; set; }
    }

    [FixedSize(92)]
    public class StyleBlock
    {
        [Offset(0)]
        public TagReferenceGen5 MaterialStyle { get; set; }

        [Offset(28)]
        public TagReferenceGen5 MaterialStyleTag { get; set; }

        [Offset(56)]
        public StringHashGen5 RegionName { get; set; }

        [Offset(60)]
        public StringHashGen5 BaseIntention { get; set; }

        [Offset(64)]
        public StringHashGen5 Mask0RedIntention { get; set; }

        [Offset(68)]
        public StringHashGen5 Mask0GreenIntention { get; set; }

        [Offset(72)]
        public StringHashGen5 Mask0BlueIntention { get; set; }

        [Offset(76)]
        public StringHashGen5 Mask1RedIntention { get; set; }

        [Offset(80)]
        public StringHashGen5 Mask1GreenIntention { get; set; }

        [Offset(84)]
        public StringHashGen5 Mask1BlueIntention { get; set; }

        [Offset(88)]
        public SupportedLayer SupportedLayers { get; set; }

        [Offset(89)]
        public bool MaterialShaderRequiresDamage { get; set; }
    }

    [FixedSize(56)]
    [DebuggerDisplay($"{{{nameof(BitmapReference)},nq}}")]
    public class TexturesBlock
    {
        [Offset(0)]
        public TagReferenceGen5 BitmapReference { get; set; }
    }

    public enum ParameterType : int
    {
        Bitmap,
        Real,
        Int,
        Bool,
        Color,
        ScalarGPUProperty,
        ColorGPUProperty,
        String,
        Preset
    }

    public enum SupportedLayer : byte
    {
        Supports1Layer,
        Supports4Layers,
        Supports7Layers,
        LayeredShaderDisabled
    }
}