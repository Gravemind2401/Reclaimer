using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class runtime_coating_styles
    {
        [Offset(16)]
        public BlockCollection<RuntimeCoatingStyleRef> Styles { get; set; }
        [Offset(36)]
        public TagReference VisorSwatch { get; set; }
    }

    [FixedSize(36)]
    public class RuntimeCoatingStyleRef
    {
        [Offset(0)]
        public StringHash Name { get; set; }
        [Offset(4)]
        public StringHash VariantName { get; set; }
        [Offset(8)]
        public TagReference StyleReference { get; set; }
 
    }

}
