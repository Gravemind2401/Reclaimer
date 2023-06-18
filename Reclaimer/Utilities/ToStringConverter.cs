using System.Globalization;
using System.Windows.Data;

namespace Reclaimer.Utilities
{
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return null;
                else if (parameter == null)
                    return value.ToString();
                else
                    return ((dynamic)value).ToString(parameter.ToString());
            }
            catch
            {
                return value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
