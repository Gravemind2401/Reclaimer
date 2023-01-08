using Reclaimer.IO;
using System;
using System.Runtime.InteropServices;

namespace Reclaimer.Geometry
{
    public record struct RealBounds(float Min, float Max) : IBufferable<RealBounds>
    {
        private const int packSize = sizeof(float);
        private const int structureSize = sizeof(float) * 2;

        public float Length => Max - Min;
        public float Midpoint => (Min + Max) / 2;

        private RealBounds(ReadOnlySpan<float> values)
            : this(values[0], values[1])
        { }

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;

        static RealBounds IBufferable<RealBounds>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new RealBounds(MemoryMarshal.Cast<byte, float>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { Min, Max }).CopyTo(buffer);
        
        #endregion
    }
}
