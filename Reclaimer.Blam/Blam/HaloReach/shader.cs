using Reclaimer.Blam.Common;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class shader
    {
        [Offset(0)]
        public TagReference RenderMethodDefinitionReference { get; set; }

        [Offset(32)]
        public BlockCollection<ShaderOptionIndexBlock> ShaderOptions { get; set; }

        [Offset(56)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(2)]
    public class ShaderOptionIndexBlock
    {
        [Offset(0)]
        public short OptionIndex { get; set; }
    }

    [FixedSize(172)]
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
