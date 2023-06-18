using System.Globalization;
using System.Windows.Data;

namespace Reclaimer.Utilities
{
    public class RadToDeg64Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rad = value as double?;
            if (rad == null)
                return null;

            var deg = Utils.RadToDeg(rad.Value);

            return parameter == null ? deg.ToString() : deg.ToString(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
