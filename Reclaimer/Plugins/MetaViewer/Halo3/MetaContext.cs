using Adjutant.Blam.Common;
using Adjutant.Blam.Common.Gen3;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class MetaContext : IDisposable
    {
        public XmlDocument Document { get; }
        public ICacheFile Cache { get; }
        public IIndexItem IndexItem { get; }
        public Stream DataSource { get; }

        private readonly Dictionary<XmlNode, MetaValue> valuesByNode = new Dictionary<XmlNode, MetaValue>();

        public MetaContext(XmlDocument xml, ICacheFile cache, IIndexItem indexItem)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (indexItem == null)
                throw new ArgumentNullException(nameof(indexItem));

            Document = xml;
            Cache = cache;
            IndexItem = indexItem;

            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            DataSource = new TransactionStream(fs);
        }

        public MetaContext(XmlDocument xml, ICacheFile cache, IIndexItem indexItem, Stream dataSource)
        {
            if (xml == null)
                throw new ArgumentNullException(nameof(xml));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (indexItem == null)
                throw new ArgumentNullException(nameof(indexItem));

            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            Document = xml;
            Cache = cache;
            IndexItem = indexItem;
            DataSource = dataSource;
        }

        public EndianReader CreateReader()
        {
            var reader = Cache.CreateReader(Cache.DefaultAddressTranslator, DataSource, true);
            var expander = (Cache as IMccCacheFile)?.PointerExpander;
            if (expander != null)
                reader.RegisterInstance(expander);

            return reader;
        }

        internal void AddValue(XmlNode node, MetaValue value)
        {
            if (valuesByNode.ContainsKey(node))
                valuesByNode[node] = value;
            else valuesByNode.Add(node, value);
        }

        public MetaValue GetValue(string xpath)
        {
            var node = Document.SelectSingleNode(xpath);
            if (node != null && valuesByNode.ContainsKey(node))
                return valuesByNode[node];
            else return null;
        }

        public void UpdateBlockIndices()
        {
            foreach (var bi in valuesByNode.Values.OfType<BlockIndexValue>())
                bi.ReadOptions();
        }

        public void Dispose()
        {
            DataSource.Dispose();
        }
    }
}
