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

            if (parameter == null)
                return deg.ToString();
            else return deg.ToString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (float.TryParse(value?.ToString(), out var deg))
                return (float)Utils.DegToRad(deg);
            else return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}
