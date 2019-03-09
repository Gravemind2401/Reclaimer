using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IBlockCollection<T> : IList<T>, IReadOnlyList<T>
    {
        Pointer Pointer { get; set; }
    }
}
