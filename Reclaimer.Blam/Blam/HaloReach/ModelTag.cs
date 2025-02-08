using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
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

        //[Offset(124, MaxVersion = (int)CacheType.HaloReachRetail)]
        //[Offset(132, MinVersion = (int)CacheType.HaloReachRetail)]
        //public BlockCollection<ModelVariant> Variants { get; set; }

        public RenderModelTag ReadRenderModel() => RenderModel.Tag?.ReadMetadata<RenderModelTag>();
    }
}
