using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public enum MetaValueType
    {
        Undefined,
        Revisions,
        Structure,
        TagReference,
        StringId,
        String,
        Bitmask8,
        Bitmask16,
        Bitmask32,
        Comment,
        Float32,
        SByte,
        Int16,
        Int32,
        Int64,
        Byte,
        UInt16,
        UInt32,
        UInt64,
        Enum8,
        Enum16,
        Enum32,
    }

    public struct FieldDefinition
    {
        private static readonly Dictionary<string, string> aliasLookup = new Dictionary<string, string>();
        private static readonly Dictionary<string, FieldDefinition> cache = new Dictionary<string, FieldDefinition>();

        private static readonly FieldDefinition UndefinedDefinition = new FieldDefinition("undefined", MetaValueType.Undefined, 4, 1, AxesDefinition.None);

        public static FieldDefinition GetDefinition(XmlNode node)
        {
            if (cache.Count == 0)
            {
                var doc = new XmlDocument();
                doc.LoadXml(Reclaimer.Properties.Resources.Halo3FieldDefinitions);

                foreach (XmlNode def in doc.DocumentElement.ChildNodes)
                {
                    var defName = def.Name.ToLowerInvariant();
                    cache.Add(defName, new FieldDefinition(def));

                    foreach(XmlNode child in def.ChildNodes)
                    {
                        if (child.Name.ToLowerInvariant() != "alias")
                            continue;

                        var alias = child.GetStringAttribute("name")?.ToLowerInvariant();
                        if (!string.IsNullOrEmpty(alias) && !aliasLookup.ContainsKey(alias))
                            aliasLookup.Add(alias, defName);
                    }
                }
            }

            var key = node.Name.ToLowerInvariant();
            if (aliasLookup.ContainsKey(key))
                key = aliasLookup[key];

            if (!cache.ContainsKey(key))
                return UndefinedDefinition;

            var result = cache[key];
            if (result.Size >= 0)
                return result;
            else if (result.Size == -1)
            {
                var totalSize = node.GetIntAttribute("length", "maxlength", "size") ?? 0;
                return new FieldDefinition(result, totalSize);
            }
            else if (result.Size == -2)
            {
                var totalSize = node.ChildNodes.OfType<XmlNode>()
                    .Sum(n => GetDefinition(n).Size);
                return new FieldDefinition(result, totalSize);
            }
            else
            {
                System.Diagnostics.Debugger.Break();
                return result;
            }
        }

        public string FieldType { get; }
        public MetaValueType ValueType { get; }
        public int Size { get; }
        public int Components { get; }
        public AxesDefinition Axes { get; }

        private FieldDefinition(string fieldType, MetaValueType valueType, int size, int components, AxesDefinition axes)
        {
            FieldType = fieldType;
            ValueType = valueType;
            Size = size;
            Components = components;
            Axes = AxesDefinition.None;
        }

        private FieldDefinition(FieldDefinition copyFrom, int newSize)
        {
            FieldType = copyFrom.FieldType;
            ValueType = copyFrom.ValueType;
            Size = newSize;
            Components = copyFrom.Components;
            Axes = copyFrom.Axes;
        }

        private FieldDefinition(XmlNode node)
        {
            FieldType = node.Name;
            ValueType = node.GetEnumAttribute<MetaValueType>("valueType") ?? MetaValueType.Undefined;

            var sizeStr = node.GetStringAttribute("size")?.ToLowerInvariant();
            if (sizeStr == "length")
                Size = -1;
            else if (sizeStr == "sum")
                Size = -2;
            else if (sizeStr == "?")
                Size = -3;
            else Size = node.GetIntAttribute("size") ?? 0;

            Components = node.GetIntAttribute("components") ?? 1;
            Axes = node.GetEnumAttribute<AxesDefinition>("axes") ?? AxesDefinition.None;
        }

        public override string ToString() => FieldType;
    }

    public enum AxesDefinition
    {
        None,
        Point,
        Vector,
        Bounds,
        Color
    }

    public class ShowInvisiblesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2 && values[0] is bool && values[1] is bool)
            {
                if ((bool)values[0] || (bool)values[1])
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FieldVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var meta = value as Halo3.MetaValue;
            int index;

            if (meta == null || !int.TryParse(parameter?.ToString(), out index))
                return Visibility.Collapsed;

            return index < meta.FieldDefinition.Components ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CommentVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MetaValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var meta = item as Halo3.MetaValue;

            if (element == null || meta == null)
                return base.SelectTemplate(item, container);

            if (meta is Halo3.CommentValue)
                return element.FindResource("CommentTemplate") as DataTemplate;
            else if (meta is Halo3.StructureValue)
                return element.FindResource("StructureTemplate") as DataTemplate;
            else if (meta is Halo3.MultiValue)
                return element.FindResource("MultiValueTemplate") as DataTemplate;
            else if (element.Tag as string == "content")
            {
                if (meta is Halo3.StringValue)
                    return element.FindResource("StringContent") as DataTemplate;
                else if (meta is Halo3.EnumValue)
                    return element.FindResource("EnumContent") as DataTemplate;
                else if (meta is Halo3.BitmaskValue)
                    return element.FindResource("BitmaskContent") as DataTemplate;
                else if (meta is Halo3.TagReferenceValue)
                    return element.FindResource("TagReferenceContent") as DataTemplate;
                else
                    return element.FindResource("DefaultContent") as DataTemplate;
            }
            else return element.FindResource("SingleValueTemplate") as DataTemplate;
        }
    }
}
