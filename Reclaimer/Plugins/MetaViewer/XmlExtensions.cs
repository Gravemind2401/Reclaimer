using System.Globalization;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public static class XmlExtensions
    {
        public static string GetStringAttribute(this XmlNode node, params string[] possibleNames) => FindAttribute(node, possibleNames)?.Value;

        public static int? GetIntAttribute(this XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null)
                return null;

            var strVal = attr.Value;

            if (int.TryParse(strVal, out var intVal))
                return intVal;
            else if (int.TryParse(strVal[2..], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out intVal))
                return intVal;
            else
                return null;
        }

        public static bool? GetBoolAttribute(this XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null)
                return null;

            var strVal = attr.Value;
            return bool.TryParse(strVal, out var boolVal) ? boolVal : null;
        }

        public static TEnum? GetEnumAttribute<TEnum>(this XmlNode node, params string[] possibleNames) where TEnum : struct
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null)
                return null;

            var strVal = attr.Value;

            if (Enum.TryParse(strVal, true, out TEnum enumVal))
                return enumVal;
            else
                return null;
        }

        public static XmlAttribute FindAttribute(this XmlNode node, params string[] possibleNames)
        {
            return node.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => possibleNames.Any(s => s.ToUpper() == a.Name.ToUpper()));
        }
    }
}
