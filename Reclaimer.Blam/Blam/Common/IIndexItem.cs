using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public interface IIndexItem
    {
        ICacheFile CacheFile { get; }

        int Id { get; }
        int ClassId { get; }
        Pointer MetaPointer { get; }
        string FullPath { get; }
        string ClassCode { get; }
        string ClassName { get; }

        T ReadMetadata<T>();
    }
}
