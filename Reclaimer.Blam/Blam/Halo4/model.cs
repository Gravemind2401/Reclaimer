using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class model
    {
        [Offset(0)]
        public TagReference RenderModel { get; set; }

        [Offset(16)]
        public TagReference CollisionModel { get; set; }

        [Offset(32)]
        public TagReference Animation { get; set; }

        [Offset(48)]
        public TagReference PhysicsModel { get; set; }

        //[Offset(180, MaxVersion = (int)CacheType.HaloReachRetail)]
        //[Offset(196, MinVersion = (int)CacheType.HaloReachRetail)]
        //public BlockCollection<ModelVariant> Variants { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
