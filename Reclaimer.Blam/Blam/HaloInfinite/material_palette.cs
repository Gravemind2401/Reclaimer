using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class material_palette
    {
        [Offset(16)]
        public BlockCollection<MaterialSwatchEntry> Swatches { get; set; }
    }

    [FixedSize(56)]
    public class MaterialSwatchEntry
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public TagReference Swatch { get; set; }
        [Offset(32)]
        public StringHash Color { get; set; }
        [Offset(36)]
        public RoughnessOverride RoughnessOverride { get; set; }
        [Offset(40)]
        public float EmissiveIntensity { get; set; }
        [Offset(44)]
        public float EmissiveAmount { get; set; }
    }

    public enum RoughnessOverride : byte
    {
        Negative100,
        Negative75,
        Negative50,
        Negative25,
        Default,
        Positive25,
        Positive50,
        Positive75,
        Positive100
    }
}