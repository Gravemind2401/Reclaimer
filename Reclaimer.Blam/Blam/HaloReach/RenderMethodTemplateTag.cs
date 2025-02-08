using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class RenderMethodTemplateTag
    {
        [Offset(72)]
        public BlockCollection<StringId> Arguments { get; set; }

        [Offset(108)]
        public BlockCollection<StringId> Usages { get; set; }
    }
}
