using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class model
    {
        [Offset(16)]
        public TagReference RenderModel { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
