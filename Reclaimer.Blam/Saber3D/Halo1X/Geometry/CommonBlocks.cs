using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DataBlock(0x0100, ExpectedSize = 0)]
    public class EmptyBlock : DataBlock
    {
        protected override object GetDebugProperties() => null;
    }

    [DataBlock(0xBA01)]
    public class StringBlock0xBA01 : StringBlock
    {

    }

    [DataBlock(0x1501)]
    public class StringBlock0x1501 : StringBlock
    {

    }
}
