using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public abstract class ObjectTagBase
    {
        [Offset(16)]
        public short ObjectType { get; set; }

        [Offset(156)]
        public StringId DefaultVariant { get; set; }

        [Offset(160)]
        public TagReference Model { get; set; }

        [Offset(192)]
        public TagReference CrateObject { get; set; }

        public render_model ReadRenderModel() => Model.Tag?.ReadMetadata<model>().RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
