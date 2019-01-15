using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Definitions
{
    public interface IIndexItem
    {
        int Id { get; }
        Pointer MetaPointer { get; }
        string FileName { get; }
        string ClassCode { get; }
    }
}
