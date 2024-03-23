using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    public class model
    {
        [Offset(0)]
        public TagReference RenderModel { get; set; }

        [Offset(8)]
        public TagReference CollisionModel { get; set; }

        [Offset(16)]
        public TagReference Animation { get; set; }

        [Offset(32)]
        public TagReference PhysicsModel { get; set; }

        //[Offset(80)]
        //public BlockCollection<ModelVariant> Variants { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
