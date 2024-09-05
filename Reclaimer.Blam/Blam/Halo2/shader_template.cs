using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    public class shader_template
    {
        [Offset(88)]
        public BlockCollection<ShaderTemplatePropertiesBlock> ShaderTemplateProperties { get; set; }
    }

    [FixedSize(40)]
    public class ShaderTemplatePropertiesBlock
    {
        [Offset(16)]
        public BlockCollection<ShaderPassBlock> ShaderPasses { get; set; }

        [Offset(24)]
        public BlockCollection<ImplementationBlock> Implementations { get; set; }

        [Offset(32)]
        public BlockCollection<RemappingBlock> Remappings { get; set; }
    }

    [FixedSize(10)]
    [DebuggerDisplay($"{{{nameof(ShaderPassReference)},nq}}")]
    public class ShaderPassBlock
    {
        [Offset(0)]
        public TagReference ShaderPassReference { get; set; }

        [Offset(8)]
        public BlockRange ImplementationRange { get; set; }
    }

    [FixedSize(6)]
    public class ImplementationBlock
    {
        [Offset(0)]
        public BlockRange BitmapRemappingRange { get; set; }

        [Offset(2)]
        public BlockRange PixelConstantsRemappingRange { get; set; }

        [Offset(4)]
        public BlockRange VertexConstantsRemappingRange { get; set; }
    }

    [FixedSize(4)]
    public class RemappingBlock
    {
        [Offset(3)]
        public byte BlockIndex { get; set; }
    }
}
