using System.IO;
using System.Runtime.CompilerServices;

namespace Reclaimer.Utilities
{
    internal static class Exceptions
    {
        public static void ThrowIfNull(object argument, [CallerArgumentExpression(nameof(argument))] string paramName = null) => ArgumentNullException.ThrowIfNull(argument, paramName);
        public static void ThrowIfNullOrEmpty(string argument, [CallerArgumentExpression(nameof(argument))] string paramName = null) => ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
        public static void ThrowIfNullOrWhiteSpace(string argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            if (!string.IsNullOrWhiteSpace(argument))
                return;

            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            throw new ArgumentException($"'{paramName}' cannot be null or whitespace.", paramName);
        }

        public static void ThrowIfFileNotFound(string argument)
        {
            if (!File.Exists(argument))
                throw new FileNotFoundException("The file does not exist.", argument);
        }

        public static void ThrowIfOutOfRange<T>(T argument, T inclusiveMin, T exclusiveMax, [CallerArgumentExpression(nameof(argument))] string paramName = null) where T : IComparable<T>
        {
            if (argument.CompareTo(inclusiveMin) < 0 || argument.CompareTo(exclusiveMax) >= 0)
                throw new ArgumentOutOfRangeException(paramName);
        }

        public static void ThrowIfIndexOutOfRange(int argument, int count, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            if (argument < 0 || argument >= count)
                throw new ArgumentOutOfRangeException(paramName, "Index was out of range. Must be non-negative and less than the size of the collection.");
        }
    }
}
