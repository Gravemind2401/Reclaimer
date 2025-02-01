using Reclaimer.Blam.Common;
using Reclaimer.Saber3D.Common;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Reclaimer.Blam.Utilities
{
    internal static class Exceptions
    {
        [DebuggerHidden]
        public static ArgumentException ParamMustBeNonZero(string paramName) => new ArgumentException(Utils.CurrentCulture($"{paramName} cannot be zero."), paramName);

        [DebuggerHidden]
        public static InvalidOperationException CoordSysNotConvertable() => new InvalidOperationException(Utils.CurrentCulture($"No conversion exists between the given coordinate systems."));

        [DebuggerHidden]
        public static ArgumentException NotAValidMapFile(string fileName) => new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It is not a valid map file or it may be compressed."));

        [DebuggerHidden]
        public static ArgumentException UnknownMapFile(string fileName) => new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It looks like a valid map file, but may not be a supported version."));

        [DebuggerHidden]
        public static ArgumentException NotAValidModuleFile(string fileName) => new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It is not a valid module file."));

        [DebuggerHidden]
        public static ArgumentException UnknownModuleFile(string fileName) => new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It looks like a valid module file, but may not be a supported version."));

        [DebuggerHidden]
        public static NotSupportedException BitmapFormatNotSupported(string formatName) => new NotSupportedException($"The BitmapFormat '{formatName}' is not supported.");

        [DebuggerHidden]
        public static ArgumentException NotASaberTextureItem(IPakItem item) => new ArgumentException($"'{item.Name}' is not a texture file.");

        [DebuggerHidden]
        public static NotSupportedException AmbiguousScenarioReference() => new NotSupportedException("Could not determine primary scenario tag.");

        [DebuggerHidden]
        public static InvalidOperationException GeometryHasNoEdges() => new InvalidOperationException("Geometry contains no edges.");

        [DebuggerHidden]
        public static NotSupportedException ResourceDataNotSupported(ICacheFile cache) => new NotSupportedException($"Cannot read resource data for {nameof(CacheType)}.{cache.CacheType}");

        [DebuggerHidden]
        public static void ThrowIfFileNotFound(string argument)
        {
            if (!File.Exists(argument))
                throw new FileNotFoundException("The file does not exist.", argument);
        }

        [DebuggerHidden]
        public static void ThrowIfNegative(int argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            if (argument < 0)
                throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must be non-negative.");
        }

        [DebuggerHidden]
        public static void ThrowIfNonPositive(int argument, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            if (argument <= 0)
                throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must be greater than zero.");
        }

        [DebuggerHidden]
        public static void ThrowIfOutOfRange<T>(T argument, T inclusiveMin, T exclusiveMax, [CallerArgumentExpression(nameof(argument))] string paramName = null) where T : IComparable<T>
        {
            if (argument.CompareTo(inclusiveMin) < 0 || argument.CompareTo(exclusiveMax) >= 0)
                throw new ArgumentOutOfRangeException(paramName, $"'{paramName}' must be greater than or equal to {inclusiveMin} and less than {exclusiveMax}.");
        }

        [DebuggerHidden]
        public static void ThrowIfIndexOutOfRange(int argument, int count, [CallerArgumentExpression(nameof(argument))] string paramName = null)
        {
            if (argument < 0 || argument >= count)
                throw new ArgumentOutOfRangeException(paramName, "Index was out of range. Must be non-negative and less than the size of the collection.");
        }
    }
}
