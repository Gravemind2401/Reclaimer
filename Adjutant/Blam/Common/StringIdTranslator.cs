using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Common
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
                return 0;

            var nsFirst = (ns.Id << indexBits) | (ns.Min);
            return index - ns.Start + nsFirst;
        }

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

            public override string ToString()
            {
                return $"Id={Id}, Min={Min}, Start={Start}";
            }
        }
    }
}
