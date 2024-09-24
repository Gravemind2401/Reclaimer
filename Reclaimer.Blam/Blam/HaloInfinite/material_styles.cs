using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class materialstyles
    {
        [Offset(16)]
        public BlockCollection<StyleRegion> Regions { get; set; }
        [Offset(80)]
        public BlockCollection<MaterialStyle> Styles { get; set; }
        [Offset(116)]
        public TagReference VisorSwatch { get; set; }
    }

    [FixedSize(24)]
    public class StyleRegion
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public BlockCollection<StringHash> IntentionConversionList { get; set; }
    }

    [FixedSize(92)]
    public class MaterialStyle
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public TagReference Palette { get; set; }
        [Offset(32)]
        public StringHash GlobalDamage { get; set; }
        [Offset(36)]
        public StringHash HeroDamage { get; set; }
        [Offset(40)]
        public StringHash GlobalEmissive { get; set; }
        [Offset(44)]
        public float EmissiveAmount { get; set; }
        [Offset(48)]
        public float ScratchAmount { get; set; }
        [Offset(52)]
        public StringHash GrimeType { get; set; }
        [Offset(56)]
        public float GrimeAmount { get; set; }
        [Offset(60)]
        public BlockCollection<MaterialStyleRegion> Regions { get; set; }
    }

    [FixedSize(40)]
    public class MaterialStyleRegion
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(8)]
        public BlockCollection<MaterialStyleLayer> Layers { get; set; }
    }

    [FixedSize(72)]
    public class MaterialStyleLayer
    {
        [Offset(0)]
        public long ElementId { get; set; }
        [Offset(8)]
        public StringHash Name { get; set; }
        [Offset(12)]
        public bool HeroReveal { get; set; }
        [Offset(13)]
        public bool ColorBlend { get; set; }
        [Offset(14)]
        public bool NormalBlend { get; set; }
        [Offset(15)]
        public bool IgnoreTexelDensity { get; set; }
    }
}