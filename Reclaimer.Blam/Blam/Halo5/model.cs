using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class model
    {
        [Offset(16)]
        public TagReference RenderModel { get; set; }

        [Offset(48)]
        public TagReference CollisionModel { get; set; }

        [Offset(80)]
        public TagReference Animation { get; set; }

        [Offset(112)]
        public TagReference PhysicsModel { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
