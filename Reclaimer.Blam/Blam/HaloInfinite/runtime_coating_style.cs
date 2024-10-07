using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class runtime_coating_style
    {
        [Offset(16)]
        public CoatingPaletteInfo GlobalDamageSwatch { get; set; }
        [Offset(144)]
        public CoatingPaletteInfo HeroDamageSwatch { get; set; }
        [Offset(272)]
        public CoatingPaletteInfo GlobalEmissiveSwatch { get; set; }
        [Offset(400)]
        public float EmissiveAmount { get; set; }
        [Offset(404)]
        public float EmissiveIntensity { get; set; }
        [Offset(408)]
        public float ScratchAmount { get; set; }
        [Offset(416)]
        public CoatingPaletteInfo GrimeSwatch { get; set; }
        [Offset(544)]
        public float GrimeAmount { get; set; }
        [Offset(552)]
        public BlockCollection<RuntimeCoatingRegion> Regions { get; set; }
    }

    [FixedSize(52)]
    public class RuntimeCoatingRegion
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public TagReference CoatingMaterialOverride { get; set; }
        [Offset(32)]
        public BlockCollection<RuntimeCoatingIntention> Intentions { get; set; }
    }

    [FixedSize(136)]
    public class RuntimeCoatingIntention
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(8)]
        public CoatingPaletteInfo Info { get; set; }
    }

    [FixedSize(128)]
    public class CoatingPaletteInfo
    {
        [Offset(0)]
        public long ElementID { get; set; }
        [Offset(8)]
        public StringHash Description { get; set; }
        [Offset(12)]
        public TagReference Swatch { get; set; }
        [Offset(40)]
        public bool UseSwatchColors { get; set; }
        [Offset(44)]
        public RealVector3 GradientTopColor { get; set; }
        [Offset(56)]
        public RealVector3 GradientMiddleColor { get; set; }
        [Offset(68)]
        public RealVector3 GradientBottomColor { get; set; }
        [Offset(80)]
        public float RoughnessOverride { get; set; }
        [Offset(84)]
        public bool UseScratchSwatchColors { get; set; }
        [Offset(88)]
        public RealVector3 ScratchColor { get; set; }
        [Offset(100)]
        public float ScratchRoughnessOffset { get; set; }
        [Offset(104)]
        public bool UseEmissive {  get; set; }
        [Offset(108)]
        public float EmissiveIntensity { get; set; }
        [Offset(112)]
        public float EmissiveAmount { get; set; }
        [Offset(116)]
        public bool UseAlpha { get; set; }
        [Offset(118)]
        public SubsurfaceUsage SubsurfaceUsage { get; set; }
        [Offset(120)]
        public bool HeroReveal { get; set; }
        [Offset(121)]
        public bool ColorBlend { get; set; }
        [Offset(122)]
        public bool NormalBlend { get; set; }
        [Offset(123)]
        public bool IgnoreTexelDensity { get; set; }
    }

    public enum SubsurfaceUsage : short
    {
        DoNotUseSubsurface,
        UseSubsurface,
        UseSubsurfaceGummy,
        UseSubsurfaceAlien,
        UseSubsurfaceBrute,
        UseSubsurfaceHumanPost,
        UseSubsurfaceHumanPre,
        UseSubsurfaceInquisitor,
        UseSubsurfaceMarble,
        UseSubsurfacePlastic,
        UseSubsurfacePreIntegrated,
        UseSubsurfaceSnow,
        UseSubsurfaceFlood
    }
}
