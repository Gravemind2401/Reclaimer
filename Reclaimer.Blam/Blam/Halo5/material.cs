using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class material
    {
        [Offset(48)]
        public BlockCollection<MaterialParametersBlock> MaterialParameters { get; set; }

        [Offset(76)]
        public BlockCollection<PostprocessDefinitionBlock> PostprocessDefinitions { get; set; }
    }

    [FixedSize(232)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class MaterialParametersBlock
    {
        [Offset(0)]
        public StringHash ParameterName { get; set; }

        [Offset(4)]
        public ParameterType ParameterType { get; set; }

        [Offset(12)]
        public TagReferenceGen5 BitmapReference { get; set; }

        [Offset(48)]
        public RealVector4 ColorValue { get; set; }

        [Offset(64)]
        public float RealValue { get; set; }

        [Offset(68)]
        public RealVector3 VectorValue { get; set; }

        private string GetDebuggerDisplay() => $"[{ParameterType}] {ParameterName}";
    }

    [FixedSize(198)]
    public class PostprocessDefinitionBlock
    {
        [Offset(0)]
        public BlockCollection<TexturesBlock> Textures { get; set; }

        [Offset(116)]
        public AlphaBlendMode AlphaBlendMode { get; set; }
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
        Color
    }

    public enum AlphaBlendMode : byte
    {
        Opaque,
        Additive,
        Multiply,
        AlphaBlend,
        DoubleMultiply,
        PreMultipliedAlpha,
        Maximum,
        MultiplyAdd,
        AddSrcTimesDstAlpha,
        AddSrcTimesSrcAlpha,
        InvAlphaBlend,
        MotionBlurStatic,
        MotionBlurInhibit,
        AlphaBlendConstant,
        OverdrawApply,
        WetScreenEffect,
        Minimum,
        Revsubtract,
        ForgeLightmap,
        ForgeLightmapinv,
        ReplaceAllChannels,
        AlphaBlendMax,
        OpaqueAlphaBlend,
        AlphaBlendAdditiveTransparent,
        FloatingShadow,
        DecalAlphaBlend,
        DecalAddSrcTimesSrcAlpha,
        DecalMultiplyAdd,
        WpfBlendMode,
        WpfNoColorBlendMode,
        BlendForgeAnalyticRed,
        BlendFactorGreenOnly,
        BlendForgeAnalyticBlue,
        BlendFactorAlphaOnly,
        MultiplyRedGreenOnly,
        MultiplyBlueAlphaOnly,
        DecalOpaque,
        AccumulatePreMultipliedAlpha,
        AccumulateMultiplyAdd,
        AccumulateAlphaBlend,
        AccumulateInverseAlphaBlend,
        AccumulateAdditive,
        AccumulateAdditiveTransparent,
        AccumulateAddSrcTimesSrcAlpha,
        AccumulateMultiply,
        SsaoMultiply,
        SsaoAlphaChannelOnly,
        DepthPeelingBlendMode,
        DepthPeelingZPass,
        WpfAdditiveBlendMode,
        ForgeNormalize,
        DepthPeelingAccumulateAlpha,
        AlphaBlendForDisplayPlanes,
        ForgeLight,
        ForgeLightInverse,
        CompositeUI
    }
}
