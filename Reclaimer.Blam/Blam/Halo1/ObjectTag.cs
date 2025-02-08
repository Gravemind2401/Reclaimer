using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    public abstract class ObjectTag
    {
        [Offset(0)]
        public short ObjectType { get; set; }

        [Offset(40)]
        public TagReference Model { get; set; }

        [Offset(112)]
        public TagReference CollisionModel { get; set; }

        public GbxModelTag ReadRenderModel() => Model.Tag?.ReadMetadata<GbxModelTag>();
    }
}
