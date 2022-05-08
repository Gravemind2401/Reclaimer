using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Reclaimer.Utilities
{
    public class GridLengthConverter : IValueConverter
    {
        public GridUnitType GridUnitType { get; set; }

        public GridLengthConverter()
        {
            GridUnitType = GridUnitType.Pixel;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new GridLength((double)value, GridUnitType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ((GridLength)value).Value;
    }
}
