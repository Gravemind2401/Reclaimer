using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class material_swatch
    {
        [Offset(16)]
        public Vector2 ColorAndRoughnessTransform {  get; set; }
        [Offset(24)]
        public Vector2 NormalTextureTransform { get; set; }
        [Offset(32)]
        public TagReference ColorGradientMap { get; set; }
        [Offset(60)]
        public float RoughnessWhite {  get; set; }
        [Offset(64)]
        public float RoughnessBlack { get; set; }
        [Offset(68)]
        public TagReference NormalDetailMap { get; set; }
        [Offset(96)]
        public float Metallic {  get; set; }
        [Offset(108)]
        public float SubsurfaceStrength { get; set; }
        [Offset(112)]
        public Vector3 ScratchColor { get; set; }
        [Offset(124)]
        public float ScratchBrightness { get; set; }
        [Offset(128)]
        public float ScratchRoughness { get; set; }
        [Offset(132)]
        public float ScratchMetallic { get; set; }
        [Offset(144)]
        public float SubsurfaceIntensity { get; set; }
        [Offset(148)]
        public BlockCollection<MaterialColorVariant> ColorVariants { get; set; }
    }

    [FixedSize(48)]
    public class MaterialColorVariant
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public Vector3 GradientTopColor { get; set; }
        [Offset(16)]
        public Vector3 GradientMiddleColor { get; set; }
        [Offset(28)]
        public Vector3 GradientBottomColor { get; set; }
    }
}
