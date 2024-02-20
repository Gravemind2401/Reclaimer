using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo5
{
    internal class scenery
    {
        [Offset(160)]
        public TagReference hlmt { get; set; }
        public render_model GetModel() => hlmt.Tag?.ReadMetadata<model>().ReadRenderModel();
    }
}
