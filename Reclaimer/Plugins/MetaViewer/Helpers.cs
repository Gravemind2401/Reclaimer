using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reclaimer.Plugins.MetaViewer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class MetaValueTypeAliasAttribute : Attribute
    {
        public string Alias { get; }

        public MetaValueTypeAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public enum MetaValueType
    {
        [MetaValueTypeAlias("struct")]
        [MetaValueTypeAlias("reflexive")]
        Structure,

        StructureGroup,

        [MetaValueTypeAlias("tagreference")]
        TagRef,

        StringId,

        [MetaValueTypeAlias("ascii")]
        String,

        [MetaValueTypeAlias("bitfield8")]
        Bitmask8,

        [MetaValueTypeAlias("bitfield16")]
        Bitmask16,

        [MetaValueTypeAlias("bitfield32")]
        Bitmask32,

        Comment,

        DataRef,

        [MetaValueTypeAlias("float")]
        Float32,

        [MetaValueTypeAlias("sbyte")]
        Int8,

        [MetaValueTypeAlias("short")]
        Int16,

        [MetaValueTypeAlias("int")]
        Int32,

        [MetaValueTypeAlias("long")]
        Int64,

        [MetaValueTypeAlias("byte")]
        UInt8,

        [MetaValueTypeAlias("ushort")]
        UInt16,

        [MetaValueTypeAlias("uint")]
        UInt32,

        [MetaValueTypeAlias("ulong")]
        UInt64,

        RawID,

        Enum8,
        Enum16,
        Enum32,

        Undefined,

        //ShortBounds,

        [MetaValueTypeAlias("bounds")]
        RealBounds,

        //ShortPoint2D,

        [MetaValueTypeAlias("point2")]
        [MetaValueTypeAlias("point2d")]
        RealPoint2D,

        [MetaValueTypeAlias("point3")]
        [MetaValueTypeAlias("point3d")]
        RealPoint3D,

        [MetaValueTypeAlias("point4")]
        [MetaValueTypeAlias("point4d")]
        RealPoint4D,

        [MetaValueTypeAlias("vector2")]
        [MetaValueTypeAlias("vector2d")]
        RealVector2D,

        [MetaValueTypeAlias("vector3")]
        [MetaValueTypeAlias("vector3d")]
        RealVector3D,

        [MetaValueTypeAlias("vector4")]
        [MetaValueTypeAlias("vector4d")]
        RealVector4D,

        Colour32RGB,
        Colour32ARGB,
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
            var meta = value as MetaValue;
            int index;

            if (meta == null || !int.TryParse(parameter?.ToString(), out index))
                return Visibility.Collapsed;

            var isVisible = false;
            switch (meta.ValueType)
            {
                case MetaValueType.RealBounds:
                case MetaValueType.RealPoint2D:
                case MetaValueType.RealVector2D:
                    isVisible = index < 2;
                    break;

                case MetaValueType.RealPoint3D:
                case MetaValueType.RealVector3D:
                    isVisible = index < 3;
                    break;

                case MetaValueType.RealPoint4D:
                case MetaValueType.RealVector4D:
                    isVisible = index < 4;
                    break;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
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
                else if (meta is TagReferenceValue)
                    return element.FindResource("TagReferenceContent") as DataTemplate;
                else
                    return element.FindResource("DefaultContent") as DataTemplate;
            }
            else return element.FindResource("SingleValueTemplate") as DataTemplate;
        }
    }
}
