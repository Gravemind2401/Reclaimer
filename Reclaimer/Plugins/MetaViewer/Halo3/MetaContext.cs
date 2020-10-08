using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class MetaContext
    {
        public ICacheFile Cache { get; }
        public IIndexItem IndexItem { get; }
        public Stream DataSource { get; }

        public MetaContext(ICacheFile cache, IIndexItem indexItem)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (indexItem == null)
                throw new ArgumentNullException(nameof(indexItem));

            Cache = cache;
            IndexItem = indexItem;

            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            DataSource = new TransactionStream(fs);
        }

        public MetaContext(ICacheFile cache, IIndexItem indexItem, Stream dataSource)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (indexItem == null)
                throw new ArgumentNullException(nameof(indexItem));

            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            Cache = cache;
            IndexItem = indexItem;
            DataSource = dataSource;
        }
    }
}
