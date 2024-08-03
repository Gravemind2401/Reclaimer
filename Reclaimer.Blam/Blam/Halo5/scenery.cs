using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class scenery
    {
        [Offset(160)]
        public TagReference Model { get; set; }

        public render_model GetModel() => Model.Tag?.ReadMetadata<model>().ReadRenderModel();
    }
}
