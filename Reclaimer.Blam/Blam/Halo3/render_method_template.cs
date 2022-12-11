using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class render_method_template
    {
        [Offset(72)]
        public BlockCollection<StringId> Arguments { get; set; }

        [Offset(108)]
        public BlockCollection<StringId> Usages { get; set; }
    }
}
