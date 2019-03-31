using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adjutant.Utilities;
using Reclaimer.Utils;

namespace Reclaimer.Entities
{
    public partial class IndexItem : IIndexItem
    {
        private CacheFile Cache => TagIndex.CacheFile;

        private Pointer? metaPointer;

        Pointer IIndexItem.MetaPointer
        {
            get
            {
                if (!metaPointer.HasValue)
                    metaPointer = new Pointer(MetaPointer, Cache.TagAddressTranslator);

                return metaPointer.Value;
            }
        }

        int IIndexItem.Id => (int)TagId;

        public string FileName => Path.Value;

        public T ReadMetadata<T>()
        {
            throw new NotImplementedException();
        }
    }
}
