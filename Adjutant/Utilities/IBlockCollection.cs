using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IBlockCollection<T> : IList<T>
    {
        Pointer Pointer { get; set; }
    }
}
