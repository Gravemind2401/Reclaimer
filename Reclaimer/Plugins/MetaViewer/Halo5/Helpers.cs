using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public enum MetaValueType
    {
        _field_block_64,
        _field_struct,
        _field_array,

        _field_pageable_resource_64,
        _field_tag_reference_64,
        _field_data_64,

        _field_string_id,
        _field_string,
        _field_long_string,

        _field_explanation,
        _field_custom,
        _field_tag,
        _field_pad,
        _field_skip,

        _field_int64_integer,
        _field_qword_integer,
        _field_long_integer,
        _field_dword_integer,
        _field_word_integer,
        _field_short_integer,
        _field_byte_integer,
        _field_char_integer,
        _field_custom_long_block_index,
        _field_custom_short_block_index,
        _field_long_block_index,
        _field_short_block_index,

        _field_long_flags,
        _field_word_flags,
        _field_byte_flags,
        _field_long_block_flags,

        _field_long_enum,
        _field_short_enum,
        _field_char_enum,

        _field_rgb_color,
        
        _field_point_2d,

        _field_angle_bounds,
        _field_real_bounds,
        _field_fraction_bounds,
        _field_short_integer_bounds,
        _field_real_point_2d,
        _field_real_euler_angles_2d,
        _field_real_point_3d,
        _field_real_vector_3d,
        _field_real_quaternion,
        _field_real_plane_3d,
        _field_real_euler_angles_3d,
        _field_real_rgb_color,
        _field_real_argb_color,

        _field_real,
        _field_real_fraction,
        _field_angle,
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

            var isVisible = false;
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
