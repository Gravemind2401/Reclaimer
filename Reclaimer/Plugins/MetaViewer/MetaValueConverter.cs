using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reclaimer.Plugins.MetaViewer
{
    public static class MetaValueConverter
    {
        private static readonly IValueConverter radToDegConverter = new Utilities.RadToDeg32Converter();

        public static readonly DependencyProperty MetaPropertyProperty =
            DependencyProperty.RegisterAttached("MetaProperty", typeof(string), typeof(Control), new PropertyMetadata((string)null));

        public static string GetMetaProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(MetaPropertyProperty);
        }

        public static void SetMetaProperty(DependencyObject obj, string value)
        {
            obj.SetValue(MetaPropertyProperty, value);

            var element = obj as Control;

            if (element?.DataContext is not MetaValueBase meta)
                return;

            var descriptor = DependencyPropertyDescriptor.FromName("Text", obj.GetType(), obj.GetType());
            var binding = BindingOperations.GetBinding(obj, descriptor.DependencyProperty);
            var expr = BindingOperations.GetBindingExpression(obj, descriptor.DependencyProperty);

            if (meta.FieldDefinition.ValueType == MetaValueType.Angle)
            {
                var newBinding = new Binding(binding.Path.Path);

                foreach (var prop in typeof(Binding).GetProperties())
                {
                    try
                    {
                        prop.GetSetMethod()?.Invoke(newBinding, new object[] { prop.GetValue(binding) });
                    }
                    catch { }
                }

                newBinding.Source = expr.DataItem;
                newBinding.Converter = radToDegConverter;

                foreach (var validationRule in binding.ValidationRules)
                    newBinding.ValidationRules.Add(validationRule);

                BindingOperations.SetBinding(obj, descriptor.DependencyProperty, newBinding);
            }
        }
    }
}
