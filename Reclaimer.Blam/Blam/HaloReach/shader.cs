using Reclaimer.Blam.Common;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class shader
    {
        [Offset(0)]
        public TagReference BaseShaderReference { get; set; }

        [Offset(56)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(176)]
    public class ShaderPropertiesBlock
    {
        [Offset(0)]
        public TagReference TemplateReference { get; set; }

        [Offset(16)]
        public BlockCollection<ShaderMapBlock> ShaderMaps { get; set; }

        [Offset(28)]
        public BlockCollection<RealVector4> TilingData { get; set; }
    }

    [FixedSize(24)]
    [DebuggerDisplay($"{{{nameof(BitmapReference)},nq}}")]
    public class ShaderMapBlock
    {
        [Offset(0)]
        public TagReference BitmapReference { get; set; }

        [Offset(21)]
        public byte TilingIndex { get; set; }
    }
}
