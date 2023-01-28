using Reclaimer.Blam.Common.Gen3;
using System.Diagnostics;
using System.Xml;

namespace Reclaimer.Blam.Common
{
    internal class StringIdTranslator
    {
        private readonly int indexBits;
        private readonly int namespaceBits;
        private readonly int lengthBits;

        private readonly Dictionary<int, Namespace> namespaces;

        private static IEnumerable<XmlNode> GetNodes(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc.DocumentElement.ChildNodes.OfType<XmlNode>();
        }

        #region Xml-Based
        public StringIdTranslator(string xml)
            : this(GetNodes(xml).First())
        {

        }

        public StringIdTranslator(string xml, string collectionId)
            : this(GetNodes(xml).First(n => n.Attributes["id"].Value == collectionId))
        {

        }

        private StringIdTranslator(XmlNode node)
        {
            indexBits = int.Parse(node.Attributes["indexBits"].Value);
            namespaceBits = int.Parse(node.Attributes["namespaceBits"].Value);
            lengthBits = int.Parse(node.Attributes["lengthBits"].Value);

            namespaces = new Dictionary<int, Namespace>();
            foreach (XmlNode child in node.ChildNodes)
            {
                var id = Convert.ToInt32(child.Attributes["id"].Value, 16);
                var min = child.Attributes["min"] == null ? 0 : Convert.ToInt32(child.Attributes["min"].Value, 16);
                var start = Convert.ToInt32(child.Attributes["startIndex"].Value, 16);

                namespaces.Add(id, new Namespace(id, min, start));
            }
        }
        #endregion

        #region Header-Based
        public StringIdTranslator(IMccCacheFile cache, string xml)
            : this(cache, GetNodes(xml).First())
        {

        }

        public StringIdTranslator(IMccCacheFile cache, string xml, string collectionId)
            : this(cache, GetNodes(xml).First(n => n.Attributes["id"].Value == collectionId))
        {

        }

        private StringIdTranslator(IMccCacheFile cache, XmlNode node)
        {
            if (cache.Header is not IMccGen3Header header)
                throw new NotSupportedException($"'{nameof(cache)}' parameter must be a Gen3 cache file or later.");

            indexBits = int.Parse(node.Attributes["indexBits"].Value);
            namespaceBits = int.Parse(node.Attributes["namespaceBits"].Value);
            lengthBits = int.Parse(node.Attributes["lengthBits"].Value);

            namespaces = new Dictionary<int, Namespace>();

            if (header.StringNamespaceCount <= 1)
                return;

            var reader = cache.CreateReader(cache.DefaultAddressTranslator);
            reader.Seek(header.StringNamespaceTablePointer.Address, System.IO.SeekOrigin.Begin);
            var nsArray = reader.ReadArray<int>(header.StringNamespaceCount);

            //namespace 0 always starts at the end of the rest
            int mask = (1 << indexBits) - 1, start = nsArray[0] & mask;
            for (var i = 1; i < header.StringNamespaceCount; i++)
            {
                namespaces.Add(i, new Namespace(i, 0, start));
                start += nsArray[i] & mask;
            }
            namespaces.Add(0, new Namespace(0, nsArray[0] & mask, start));
        }
        #endregion

        public int GetStringIndex(int stringId)
        {
            var index = stringId & ((1 << indexBits) - 1);
            var id = (stringId >> indexBits) & ((1 << namespaceBits) - 1);

            while (!namespaces.ContainsKey(id) && id > 0)
                id--;

            var ns = namespaces[id];
            return index < ns.Min ? index : index - ns.Min + ns.Start;
        }

        public int GetStringId(int index)
        {
            var ns = namespaces.Values.OrderByDescending(n => n.Start)
                .FirstOrDefault(n => n.Start <= index);

            if (ns == null)
                return index;

            var nsFirst = (ns.Id << indexBits) | (ns.Min);
            return index - ns.Start + nsFirst;
        }

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private class Namespace
        {
            public int Id { get; }
            public int Min { get; }
            public int Start { get; }

            public Namespace(int id, int min, int start)
            {
                Id = id;
                Min = min;
                Start = start;
            }

            private string GetDebuggerDisplay() => new { Id, Min, Start }.ToString();
        }
    }
}
