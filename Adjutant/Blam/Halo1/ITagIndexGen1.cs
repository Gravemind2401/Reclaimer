using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public interface ITagIndexGen1
    {
        int Magic { get; }
        int VertexDataCount { get; }
        int VertexDataOffset { get; }
        int IndexDataCount { get; }
        int IndexDataOffset { get; }
    }
}
