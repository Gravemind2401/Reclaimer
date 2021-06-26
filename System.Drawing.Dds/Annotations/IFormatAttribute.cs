using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds.Annotations
{
    internal interface IFormatAttribute<T>
    {
        T Format { get; }
    }
}
