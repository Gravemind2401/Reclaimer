using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo5
{
    internal class model
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
