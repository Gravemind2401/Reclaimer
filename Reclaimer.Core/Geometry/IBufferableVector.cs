using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public interface IBufferableVector<out TSelf> : IVector, IBufferable<TSelf>
    { }
}
