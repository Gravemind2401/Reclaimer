using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class model
    {
        [Offset(16)]
        public TagReferenceGen5 RenderModel { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
