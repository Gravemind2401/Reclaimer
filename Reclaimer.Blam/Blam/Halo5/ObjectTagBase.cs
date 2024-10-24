using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public abstract class ObjectTagBase
    {
        [Offset(16)]
        public short ObjectType { get; set; }

        [Offset(156)]
        public StringIdGen5 DefaultVariant { get; set; }

        [Offset(160)]
        public TagReferenceGen5 Model { get; set; }

        [Offset(192)]
        public TagReferenceGen5 CrateObject { get; set; }

        public render_model ReadRenderModel() => Model.Tag?.ReadMetadata<model>().RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
