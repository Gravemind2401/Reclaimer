using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        ShortBounds,
        RealBounds,

        ShortPoint2D,
        RealPoint2D,
        RealPoint3D,
        RealPoint4D,

        RealVector2D,
        RealVector3D,
        RealVector4D,

        Colour32RGB,
        Colour32ARGB,
    }

    public class MetaValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var meta = item as MetaValue;

            if (element == null || meta == null)
                return base.SelectTemplate(item, container);

            switch (meta.ValueType)
            {
                case MetaValueType.Structure:
                    return element.FindResource("StructureTemplate") as DataTemplate;

                default:
                    return element.FindResource("DefaultTemplate") as DataTemplate;
            }
        }
    }
}
