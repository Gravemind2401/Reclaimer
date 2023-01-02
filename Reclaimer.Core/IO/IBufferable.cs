using System.Reflection;

namespace Reclaimer.IO
{
    public interface IBufferable
    {
        static abstract int PackSize { get; }
        static abstract int SizeOf { get; }
        void WriteToBuffer(Span<byte> buffer);

        public static int GetPackSize(Type type)
        {
            if (!typeof(IBufferable).IsAssignableFrom(type))
                throw new ArgumentException($"Type does not implement {nameof(IBufferable)}", nameof(type));

            return (int)GetPackSizeTMethod.MakeGenericMethod(type).Invoke(null, null);
        }

        public static int GetSizeOf(Type type)
        {
            if (!typeof(IBufferable).IsAssignableFrom(type))
                throw new ArgumentException($"Type does not implement {nameof(IBufferable)}", nameof(type));

            return (int)GetSizeOfTMethod.MakeGenericMethod(type).Invoke(null, null);
        }

        private static readonly MethodInfo GetPackSizeTMethod = typeof(IBufferable).GetMethod(nameof(GetSizeOfT), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo GetSizeOfTMethod = typeof(IBufferable).GetMethod(nameof(GetSizeOfT), BindingFlags.NonPublic | BindingFlags.Static);

        private static int GetPackSizeT<T>() where T : IBufferable => T.PackSize;
        private static int GetSizeOfT<T>() where T : IBufferable => T.SizeOf;
    }

    public interface IBufferable<out TBufferable> : IBufferable
    {
        static abstract TBufferable ReadFromBuffer(ReadOnlySpan<byte> buffer);
    }
}
