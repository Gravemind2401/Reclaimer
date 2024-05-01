using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo1
{
    public class shader_model
    {
        [Offset(40)]
        public ShaderModelFlags Flags { get; set; }

        [Offset(156)]
        public float BaseMapUScale { get; set; }
        
        [Offset(160)]
        public float BaseMapVScale { get; set; }

        [Offset(164)]
        public TagReference BaseMap { get; set; }

        /*

        PC:
        R: auxilary mask
        G: self illum
        B: specular reflection
        A: primary CC mask

        Xbox:
        R: specular reflection
        G: self illum
        B: primary CC mask
        A: auxilary mask

        */

        [Offset(188)]
        public TagReference MultipurposeMap { get; set; }

        [Offset(212)]
        public ShaderDetailFunction DetailFunction { get; set; }

        [Offset(214)]
        public ShaderModelDetailMask DetailMask { get; set; }

        [Offset(216)]
        public float DetailMapScale { get; set; }

        [Offset(220)]
        public TagReference DetailMap { get; set; }

        [Offset(236)]
        public float DetailMapVScale { get; set; }

        [Offset(356)]
        public TagReference ReflectionCubeMap { get; set; }
    }

    public enum ShaderDetailFunction : ushort
    {
        DoubleBiasedMultiply = 0,
        Multiply = 1,
        DoubleBiasedAdd = 2
    }

    [Flags]
    public enum ShaderModelFlags : ushort
    {
        None = 0,
        DetailAfterReflection = 1 << 0,
        TwoSided = 1 << 1,
        NotAlphaTested = 1 << 2,
        AlphaBlendedDecal = 1 << 3,
        TrueAtmosphericFog = 1 << 4,
        DisableTwoSidedCulling = 1 << 5,
        MultipurposeMapUsesOGXboxOrder = 1 << 6,
    }

    public enum ShaderModelDetailMask : ushort
    {
        None = 0,
        ReflectionMaskInverse = 1,
        ReflectionMask = 2,
        SelfIlluminationMaskInverse = 3,
        SelfIlluminationMask = 4,
        ChangeColorMaskInverse = 5,
        ChangeColorMask = 6,
        MultipurposeMapAlphaInverse = 7,
        MultipurposeMapAlpha = 8
    }
}
