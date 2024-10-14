﻿using Reclaimer.Utilities;
using System.Globalization;
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
        Array,
        TagReference,
        StringId,
        String,
        Comment,
        Padding,
        Int64,
        Int32,
        Int16,
        SByte,
        UInt64,
        UInt32,
        UInt16,
        Byte,
        Bitmask32,
        Bitmask16,
        Bitmask8,
        Enum32,
        Enum16,
        Enum8,
        Float32,
        BlockIndex32,
        BlockIndex16,
        BlockIndex8,
        Angle

        // DataReference, Color
    }

    public readonly struct FieldDefinition
    {
        private static readonly Dictionary<string, string> h3aliasLookup = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> h5aliasLookup = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> hialiasLookup = new Dictionary<string, string>();
        private static readonly Dictionary<string, FieldDefinition> h3cache = new Dictionary<string, FieldDefinition>();
        private static readonly Dictionary<string, FieldDefinition> h5cache = new Dictionary<string, FieldDefinition>();
        private static readonly Dictionary<string, FieldDefinition> hicache = new Dictionary<string, FieldDefinition>();

        private static readonly FieldDefinition UndefinedDefinition = new FieldDefinition("undefined", MetaValueType.Undefined, 4, 1, AxesDefinition.None, true);

        public static FieldDefinition GetHalo3Definition(XmlNode node) => GetDefinition(node, Properties.Resources.Halo3FieldDefinitions, h3aliasLookup, h3cache);

        public static FieldDefinition GetHalo5Definition(XmlNode node) => GetDefinition(node, Properties.Resources.Halo5FieldDefinitions, h5aliasLookup, h5cache);

        public static FieldDefinition GetHaloInfiniteDefinition(XmlNode node) => GetDefinition(node, Properties.Resources.HaloInfiniteFieldDefinitions, hialiasLookup, hicache);

        private static FieldDefinition GetDefinition(XmlNode node, string definitionXml, Dictionary<string, string> aliasLookup, Dictionary<string, FieldDefinition> cache)
        {
            if (cache.Count == 0)
            {
                var doc = new XmlDocument();
                doc.LoadXml(definitionXml);

                foreach (var def in doc.DocumentElement.GetChildElements())
                {
                    var defName = def.Name.ToLowerInvariant();
                    cache.Add(defName, new FieldDefinition(def));

                    foreach (var child in def.GetChildElements())
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
            if (aliasLookup.TryGetValue(key, out var value))
                key = value;

            if (!cache.TryGetValue(key, out var result))
                return UndefinedDefinition;

            if (result.Size >= 0)
                return result;
            else if (result.Size == -1) //length
            {
                var totalSize = node.GetIntAttribute("length", "maxlength", "size") ?? 0;
                return new FieldDefinition(result, totalSize);
            }
            else if (result.Size == -2) //sum
            {
                var totalSize = node.GetChildElements()
                    .Sum(n => GetDefinition(n, definitionXml, aliasLookup, cache).Size);
                return new FieldDefinition(result, totalSize);
            }
            else //unknown
            {
                System.Diagnostics.Debugger.Break();
                return new FieldDefinition(result, -1);
            }
        }

        public string FieldTypeName { get; }
        public MetaValueType ValueType { get; }
        public int Size { get; }
        public int Components { get; }
        public AxesDefinition Axes { get; }
        public bool Hidden { get; }

        private FieldDefinition(string fieldType, MetaValueType valueType, int size, int components, AxesDefinition axes, bool hidden)
        {
            FieldTypeName = fieldType;
            ValueType = valueType;
            Size = size;
            Components = components;
            Axes = AxesDefinition.None;
            Hidden = hidden;
        }

        private FieldDefinition(FieldDefinition copyFrom, int newSize)
        {
            FieldTypeName = copyFrom.FieldTypeName;
            ValueType = copyFrom.ValueType;
            Size = newSize;
            Components = copyFrom.Components;
            Axes = copyFrom.Axes;
            Hidden = copyFrom.Hidden;
        }

        private FieldDefinition(XmlNode node)
        {
            FieldTypeName = node.Name;
            ValueType = node.GetEnumAttribute<MetaValueType>("valueType") ?? MetaValueType.Undefined;

            var sizeStr = node.GetStringAttribute("size")?.ToLowerInvariant();
            Size = sizeStr switch
            {
                "length" => -1,
                "sum" => -2,
                "?" or null => -3,
                _ => node.GetIntAttribute("size").GetValueOrDefault()
            };

            Components = node.GetIntAttribute("components") ?? 1;
            Axes = node.GetEnumAttribute<AxesDefinition>("axes") ?? AxesDefinition.None;
            Hidden = node.GetBoolAttribute("hidden") ?? false;
        }

        public override string ToString() => FieldTypeName;
    }

    public enum AxesDefinition
    {
        None,
        Point,
        Vector,
        Bounds,
        Color,
        Angle,
        Plane
    }

    public interface IExpandable
    {
        bool IsExpanded { get; set; }
        IEnumerable<MetaValueBase> Children { get; }
    }

    public class ComboBoxItem
    {
        public string Label { get; }
        public bool IsVisible { get; }

        public ComboBoxItem(string label)
            : this(label, true)
        {

        }

        public ComboBoxItem(string label, bool isVisible)
        {
            Label = label;
            IsVisible = isVisible;
        }

        public override string ToString() => Label;
    }

    public class ComboBoxItem<T> : ComboBoxItem
    {
        public T Context { get; }

        public ComboBoxItem(string label, T context)
            : this(label, context, true)
        {

        }

        public ComboBoxItem(string label, T context, bool isVisible)
            : base(label, isVisible)
        {
            Context = context;
        }
    }

    public class ShowInvisiblesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2 && values[0] is bool b0 && values[1] is bool b1 && (b0 || b1))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class FieldVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MetaValueBase meta || !int.TryParse(parameter?.ToString(), out var index))
                return Visibility.Collapsed;

            return index < meta.FieldDefinition.Components ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CommentVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class MetaValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is not FrameworkElement element || item is not MetaValueBase meta)
                return base.SelectTemplate(item, container);

            if (meta.FieldDefinition.ValueType == MetaValueType.Comment)
                return element.FindResource("CommentTemplate") as DataTemplate;
            else if (meta.FieldDefinition.ValueType == MetaValueType.Structure)
                return element.FindResource("StructureTemplate") as DataTemplate;
            else if (meta.FieldDefinition.ValueType == MetaValueType.Array)
                return element.FindResource("ArrayTemplate") as DataTemplate;
            else if (meta.FieldDefinition.Components > 1)
                return element.FindResource("MultiValueTemplate") as DataTemplate;
            else if (element.Tag as string == "content")
            {
                switch (meta.FieldDefinition.ValueType)
                {
                    case MetaValueType.String:
                        return element.FindResource("StringContent") as DataTemplate;

                    case MetaValueType.StringId:
                        return element.FindResource("StringIdContent") as DataTemplate;

                    case MetaValueType.Enum8:
                    case MetaValueType.Enum16:
                    case MetaValueType.Enum32:
                    case MetaValueType.BlockIndex8:
                    case MetaValueType.BlockIndex16:
                    case MetaValueType.BlockIndex32:
                        return element.FindResource("EnumContent") as DataTemplate;

                    case MetaValueType.Bitmask8:
                    case MetaValueType.Bitmask16:
                    case MetaValueType.Bitmask32:
                        return element.FindResource("BitmaskContent") as DataTemplate;

                    case MetaValueType.TagReference:
                        return element.FindResource("TagReferenceContent") as DataTemplate;

                    default:
                        return element.FindResource("DefaultContent") as DataTemplate;
                }
            }
            else
                return element.FindResource("SingleValueTemplate") as DataTemplate;
        }
    }
}
