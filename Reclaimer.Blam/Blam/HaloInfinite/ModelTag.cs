using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class ModelTag
    {
        [Offset(16)]
        public TagReferenceGen5 RenderModel { get; set; }

        public RenderModelTag ReadRenderModel() => RenderModel.Tag?.ReadMetadata<RenderModelTag>();
    }
}
