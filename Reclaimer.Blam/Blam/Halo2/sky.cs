using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    public class sky
    {
        [Offset(0)]
        public TagReference RenderModel { get; set; }

        [Offset(20)]
        public float RenderModelScale { get; set; }

        //the field description in the tags says 0 defaults to 0.03
        public float GetFinalScale() => RenderModelScale == default ? 0.03f : RenderModelScale;
        
        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
