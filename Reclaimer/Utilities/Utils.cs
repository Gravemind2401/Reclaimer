using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Like <see cref="System.IO.Path.GetFileName"/> but allows invalid characters.
        /// </summary>
        public static string GetFileName(string path)
        {
            return path?.Split('\\').Last();
        }

        /// <summary>
        /// Like <see cref="System.IO.Path.GetFileNameWithoutExtension"/> but allows invalid characters.
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            if (path == null)
                return null;

            var fileName = path.Split('\\').Last();
            var index = fileName.LastIndexOf('.');

            if (index < 0)
                return fileName;
            else return fileName.Substring(0, index);
        }
    }
}
