using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Reclaimer.Utilities
{
    public class RadToDeg32Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rad = value as float?;
            if (rad == null)
                return null;

            var deg = (float)Utils.RadToDeg(rad.Value);

            return parameter == null ? deg.ToString() : deg.ToString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return float.TryParse(value?.ToString(), out var deg)
                ? Utils.DegToRad(deg)
                : System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
