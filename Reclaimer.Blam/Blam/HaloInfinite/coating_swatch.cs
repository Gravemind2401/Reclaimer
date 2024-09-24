using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class coating_swatch
    {
        [Offset(44)]
        public Vector2 ColorAndRoughnessTransform {  get; set; }
        [Offset(52)]
        public Vector2 NormalTextureTransform { get; set; }
        [Offset(60)]
        public TagReference ColorGradientMap { get; set; }
        [Offset(88)]
        public Vector3 GradientTopColor { get; set; }
        [Offset(100)]
        public Vector3 GradientMiddleColor { get; set; }
        [Offset(112)]
        public Vector3 GradientBottomColor { get; set; }
        [Offset(124)]
        public float RoughnessWhite {  get; set; }
        [Offset(128)]
        public float RoughnessBlack { get; set; }
        [Offset(132)]
        public TagReference NormalDetailMap { get; set; }
        [Offset(160)]
        public float Metallic {  get; set; }
        [Offset(172)]
        public Vector3 ScratchColor { get; set; }
        [Offset(184)]
        public float ScratchBrightness { get; set; }
        [Offset(188)]
        public float ScratchRoughness { get; set; }
        [Offset(192)]
        public float ScratchMetallic { get; set; }
        [Offset(204)]
        public float SubsurfaceIntensity { get; set; }
        [Offset(208)]
        public float EmissiveIntensity { get; set; }
        [Offset(212)]
        public float EmissiveAmount { get; set; }
    }
}
