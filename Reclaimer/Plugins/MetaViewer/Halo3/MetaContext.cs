using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public sealed class MetaContext : IDisposable
    {
        public XmlDocument Document { get; }
        public ICacheFile Cache { get; }
        public IIndexItem IndexItem { get; }
        public IAddressTranslator AddressTranslator { get; }
        public Stream DataSource { get; }

        private readonly Dictionary<XmlNode, MetaValue> valuesByNode = new Dictionary<XmlNode, MetaValue>();

        public MetaContext(XmlDocument xml, IIndexItem indexItem)
        {
            Document = xml ?? throw new ArgumentNullException(nameof(xml));
            IndexItem = indexItem ?? throw new ArgumentNullException(nameof(indexItem));

            Cache = indexItem.CacheFile;
            AddressTranslator = indexItem.GetAddressTranslator();

            DataSource = new TransactionStream(Cache.CreateStream());
        }

        public MetaContext(XmlDocument xml, IIndexItem indexItem, Stream dataSource)
        {
            Document = xml ?? throw new ArgumentNullException(nameof(xml));
            IndexItem = indexItem ?? throw new ArgumentNullException(nameof(indexItem));
            DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

            Cache = indexItem.CacheFile;
            AddressTranslator = indexItem.GetAddressTranslator();
        }

        public EndianReader CreateReader()
        {
            var reader = Cache.CreateReader(IndexItem.GetAddressTranslator(), DataSource, true);
            var expander = (Cache as IMccCacheFile)?.PointerExpander;
            if (expander != null)
                reader.RegisterInstance(expander);

            return reader;
        }

        internal void AddValue(XmlNode node, MetaValue value)
        {
            valuesByNode[node] = value;
        }

        public MetaValue GetValue(string xpath)
        {
            var node = Document.SelectSingleNode(xpath);
            return node != null && valuesByNode.TryGetValue(node, out var metaValue) ? metaValue : null;
        }

        public void UpdateBlockIndices()
        {
            foreach (var bi in valuesByNode.Values.OfType<BlockIndexValue>())
                bi.ReadOptions();
        }

        public void Dispose() => DataSource.Dispose();
    }
}
