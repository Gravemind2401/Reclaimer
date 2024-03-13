using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class render_method_definition
    {
        [Offset(16)]
        public BlockCollection<ShaderOptionCategoryBlock> Categories { get; set; }
    }

    [FixedSize(24)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ShaderOptionCategoryBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<ShaderOptionBlock> Options { get; set; }
    }

    [FixedSize(28)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ShaderOptionBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }
    }
}
