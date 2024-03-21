using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    public class sky
    {
        [Offset(0)]
        public TagReference Model { get; set; }

        public gbxmodel ReadRenderModel() => Model.Tag?.ReadMetadata<gbxmodel>();
    }
}
