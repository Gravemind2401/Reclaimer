using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    public class SkyTag
    {
        [Offset(0)]
        public TagReference Model { get; set; }

        public GbxModelTag ReadRenderModel() => Model.Tag?.ReadMetadata<GbxModelTag>();
    }
}
