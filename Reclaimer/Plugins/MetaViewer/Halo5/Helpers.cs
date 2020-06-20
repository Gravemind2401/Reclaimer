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

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public enum MetaValueType
    {
        Undefined,
        TagReference,
        Structure,
        Array,
        StringId,
        String,
        Comment,
        Padding,
        Int64,
        Int32,
        Int16,
        Byte,
        Bitmask32,
        Bitmask16,
        Bitmask8,
        Enum32,
        Enum16,
        Enum8,
        Float32

        // DataReference, Angle, Euler Angle, Color, maybe block index
    }

    public struct FieldDefinition
    {
        private static readonly Dictionary<string, FieldDefinition> cache = new Dictionary<string, FieldDefinition>();

        public static FieldDefinition GetDefinition(XmlNode node)
        {
            if (cache.Count == 0)
            {
                var doc = new XmlDocument();
                doc.LoadXml(Reclaimer.Properties.Resources.Halo5FieldDefinitions);

                foreach (XmlNode def in doc.DocumentElement.ChildNodes)
                    cache.Add(def.Name.ToLowerInvariant(), new FieldDefinition(def));
            }

            var key = node.Name.ToLowerInvariant();
            if (!cache.ContainsKey(key))
                throw new NotSupportedException();

            var result = cache[key];
            if (result.Size >= 0)
                return result;
            else if (result.Size == -1)
            {
                var totalSize = node.GetIntAttribute("length") ?? 0;
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
            ValueType = node.GetEnumAttribute<MetaValueType>("meta-type") ?? MetaValueType.Undefined;

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
            return Visibility.Visible;
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
            var meta = value as MetaValue;
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
            var meta = item as MetaValue;

            if (element == null || meta == null)
                return base.SelectTemplate(item, container);

            if (meta is CommentValue)
                return element.FindResource("CommentTemplate") as DataTemplate;
            else if (meta is StructureValue)
                return element.FindResource("StructureTemplate") as DataTemplate;
            else if (meta is MultiValue)
                return element.FindResource("MultiValueTemplate") as DataTemplate;
            else if (element.Tag as string == "content")
            {
                if (meta is StringValue)
                    return element.FindResource("StringContent") as DataTemplate;
                else if (meta is EnumValue)
                    return element.FindResource("EnumContent") as DataTemplate;
                else if (meta is BitmaskValue)
                    return element.FindResource("BitmaskContent") as DataTemplate;
                else
                    return element.FindResource("DefaultContent") as DataTemplate;
            }
            else return element.FindResource("SingleValueTemplate") as DataTemplate;
        }
    }
}
