using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class ModelTag
    {
        [Offset(0)]
        public TagReference RenderModel { get; set; }

        [Offset(16)]
        public TagReference CollisionModel { get; set; }

        [Offset(32)]
        public TagReference Animation { get; set; }

        [Offset(48)]
        public TagReference PhysicsModel { get; set; }

        //[Offset(100)]
        //public BlockCollection<ModelVariant> Variants { get; set; }

        public RenderModelTag ReadRenderModel() => RenderModel.Tag?.ReadMetadata<RenderModelTag>();
    }
}
