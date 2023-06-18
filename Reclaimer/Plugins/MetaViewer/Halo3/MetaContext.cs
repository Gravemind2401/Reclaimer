using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.IO;
using System.IO;
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
            Document = xml ?? throw new ArgumentNullException(nameof(xml));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            IndexItem = indexItem ?? throw new ArgumentNullException(nameof(indexItem));

            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            DataSource = new TransactionStream(fs);
        }

        public MetaContext(XmlDocument xml, ICacheFile cache, IIndexItem indexItem, Stream dataSource)
        {
            Document = xml ?? throw new ArgumentNullException(nameof(xml));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
            IndexItem = indexItem ?? throw new ArgumentNullException(nameof(indexItem));
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
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
            else
                valuesByNode.Add(node, value);
        }

        public MetaValue GetValue(string xpath)
        {
            var node = Document.SelectSingleNode(xpath);
            return node != null && valuesByNode.ContainsKey(node) ? valuesByNode[node] : null;
        }

        public void UpdateBlockIndices()
        {
            foreach (var bi in valuesByNode.Values.OfType<BlockIndexValue>())
                bi.ReadOptions();
        }

        public void Dispose() => DataSource.Dispose();
    }
}
