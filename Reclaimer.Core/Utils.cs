using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer
{
    internal static class Utils
    {
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            else if (value.CompareTo(max) > 0)
                return max;
            else
                return value;
        }
    }
}
