using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Reclaimer.Utils
{
    public class RadToDegConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rad = value as double?;
            if (rad == null)
                return null;

            var deg = (180 / Math.PI) * rad.Value;

            if (parameter == null)
                return deg.ToString();
            else return ((dynamic)deg).ToString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
