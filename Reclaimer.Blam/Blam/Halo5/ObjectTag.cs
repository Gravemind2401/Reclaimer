using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public abstract class ObjectTag
    {
        [Offset(16)]
        public short ObjectType { get; set; }

        [Offset(156)]
        public StringIdGen5 DefaultVariant { get; set; }

        [Offset(160)]
        public TagReferenceGen5 Model { get; set; }

        [Offset(192)]
        public TagReferenceGen5 CrateObject { get; set; }

        public RenderModelTag ReadRenderModel() => Model.Tag?.ReadMetadata<ModelTag>().RenderModel.Tag?.ReadMetadata<RenderModelTag>();
    }
}
