using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Like <see cref="System.IO.Path.GetFileName"/> but allows invalid characters.
        /// </summary>
        public static string GetFileName(string path) => path?.Split('\\').Last();

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
            else
                return fileName.Substring(0, index);
        }

        /// <summary>
        /// Replaces any characters in <paramref name="fileName"/> that are not valid file name characters.
        /// </summary>
        /// <param name="fileName">The file name to convert.</param>
        public static string GetSafeFileName(string fileName) => string.Join("_", fileName.Split(Path.GetInvalidPathChars()));

        /// <summary>
        /// Converts radian values to degree values.
        /// </summary>
        /// <param name="radians">The value in radians.</param>
        public static double RadToDeg(double radians) => radians * (180d / Math.PI);

        /// <summary>
        /// Converts degree values to radian values.
        /// </summary>
        /// <param name="degrees">The value in degrees.</param>
        public static double DegToRad(double degrees) => degrees * (Math.PI / 180d);

        /// <summary>
        /// Creates an instance of T and populates it's public properties based their <see cref="DefaultValueAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        public static T CreateDefaultInstance<T>() where T : new()
        {
            var instance = new T();

            foreach (var propInfo in typeof(T).GetProperties())
            {
                var attr = propInfo.GetCustomAttribute<DefaultValueAttribute>(true);
                if (attr?.Value == null)
                    continue;

                var setter = propInfo.GetSetMethod();
                if (setter == null)
                    continue;

                if (propInfo.PropertyType.IsAssignableFrom(attr.Value.GetType()))
                    propInfo.SetValue(instance, attr.Value);
            }

            return instance;
        }
    }
}
