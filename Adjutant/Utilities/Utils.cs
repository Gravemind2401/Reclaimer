using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    internal static class Utils
    {
        internal static string CurrentCulture(FormattableString formattable)
        {
            if (formattable == null)
                throw new ArgumentNullException(nameof(formattable));

            return formattable.ToString(CultureInfo.CurrentCulture);
        }

        internal static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(min, value), max);
        }
    }
}
