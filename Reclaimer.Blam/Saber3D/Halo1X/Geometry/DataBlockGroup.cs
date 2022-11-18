using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public class DataBlockGroup : List<DataBlock>
    {
        protected TBlock GetUniqueChild<TBlock>() => this.OfType<TBlock>().Single();
        protected TBlock GetOptionalChild<TBlock>() => this.OfType<TBlock>().SingleOrDefault();
    }
}
