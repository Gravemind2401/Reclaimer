using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public static class Extensions
    {
        public static string FileName(this IIndexItem item)
        {
            return Utilities.Utils.GetFileName(item.FullPath);
        }
    }
}
