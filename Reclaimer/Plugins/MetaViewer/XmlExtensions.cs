using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public static class XmlExtensions
    {
        public static string GetStringAttribute(this XmlNode node, params string[] possibleNames)
        {
            return FindAttribute(node, possibleNames)?.Value;
        }

        public static int? GetIntAttribute(this XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null) return null;

            int intVal;
            var strVal = attr.Value;

            if (int.TryParse(strVal, out intVal))
                return intVal;
            else if (int.TryParse(strVal.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out intVal))
                return intVal;
            else return null;
        }

        public static bool? GetBoolAttribute(this XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null) return null;

            bool boolVal;
            var strVal = attr.Value;

            if (bool.TryParse(strVal, out boolVal))
                return boolVal;
            else return null;
        }

        public static XmlAttribute FindAttribute(this XmlNode node, params string[] possibleNames)
        {
            return node.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => possibleNames.Any(s => s.ToUpper() == a.Name.ToUpper()));
        }
    }
}
