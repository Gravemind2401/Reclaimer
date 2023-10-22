using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Reclaimer.Utilities
{
    public static class Utils
    {
        /// <summary>
        /// Same as <see cref="Path.GetFileName"/> but allows invalid characters.
        /// </summary>
        public static string GetFileName(string path) => path?.Split('\\').Last();

        /// <summary>
        /// Same as <see cref="Path.GetFileNameWithoutExtension"/> but allows invalid characters.
        /// </summary>
        public static string GetFileNameWithoutExtension(string path)
        {
            if (path == null)
                return null;

            var fileName = path.Split('\\').Last();
            var index = fileName.LastIndexOf('.');

            return index < 0 ? fileName : fileName[..index];
        }

        /// <summary>
        /// Same as <see cref="Path.ChangeExtension(string, string)"/> but allows invalid characters.
        /// </summary>
        public static string ChangeExtension(string path, string extension)
        {
            var index = path.LastIndexOf('.');
            if (index >= 0)
                path = path[..index];

            if (string.IsNullOrEmpty(extension))
                return path;

            if (!extension.StartsWith('.'))
                extension = "." + extension;

            return path + extension;
        }

        /// <summary>
        /// Replaces any characters in <paramref name="fileName"/> that are not valid file name characters.
        /// </summary>
        /// <param name="fileName">The file name to convert.</param>
        public static string GetSafeFileName(string fileName) => string.Join("_", fileName.Split(Path.GetInvalidPathChars()));

        /// <summary>
        /// Wrapper for <see cref="Process.Start(string)"/> to work around permissions exception.
        /// </summary>
        /// <inheritdoc cref="Process.Start(string)"/>
        public static void StartProcess(string fileName)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true
            });
        }

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
