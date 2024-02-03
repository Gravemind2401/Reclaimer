using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public abstract class ObjectTagBase
    {
        [Offset(0)]
        public short ObjectType { get; set; }

        [Offset(76, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(96, MinVersion = (int)CacheType.HaloReachRetail)]
        public StringId DefaultVariant { get; set; }

        [Offset(80, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(100, MinVersion = (int)CacheType.HaloReachRetail)]
        public TagReference Model { get; set; }

        [Offset(96, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(116, MinVersion = (int)CacheType.HaloReachRetail)]
        public TagReference CrateObject { get; set; }

        [Offset(112, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(132, MinVersion = (int)CacheType.HaloReachRetail)]
        public TagReference CollisionDamage { get; set; }

        public render_model ReadRenderModel() => Model.Tag?.ReadMetadata<model>().RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
