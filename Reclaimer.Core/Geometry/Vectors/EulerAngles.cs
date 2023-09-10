using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with single-precision floating-point values
    /// that represents rotation angles, in radians, about the X, Y and Z axes.
    /// </summary>
    /// <param name="Yaw">The yaw angle, in radians, around the up/down axis.</param>
    /// <param name="Pitch">The pitch angle, in radians, around the left/right axis.</param>
    /// <param name="Roll">The roll angle, in radians, around the front/back axis.</param>
    public record struct EulerAngles(float Yaw, float Pitch, float Roll) : IVector3, IBufferableVector<EulerAngles>
    {
        private const int packSize = 4;
        private const int structureSize = 12;

        public EulerAngles(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        private EulerAngles(ReadOnlySpan<float> values)
            : this(values[0], values[1], values[2])
        { }

        public override readonly string ToString() => $"[{Yaw:F6}, {Pitch:F6}, {Roll:F6}]";

        #region IVector3

        float IVector.X
        {
            readonly get => Yaw;
            set => Yaw = value;
        }

        float IVector.Y
        {
            readonly get => Pitch;
            set => Pitch = value;
        }

        float IVector.Z
        {
            readonly get => Roll;
            set => Roll = value;
        }

        #endregion

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static EulerAngles IBufferable<EulerAngles>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new EulerAngles(MemoryMarshal.Cast<byte, float>(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { Yaw, Pitch, Roll }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(EulerAngles value) => new Vector3(value.Yaw, value.Pitch, value.Roll);
        public static explicit operator EulerAngles(Vector3 value) => new EulerAngles(value);
        public static implicit operator EulerAngles((float yaw, float pitch, float roll) value) => new EulerAngles(value.yaw, value.pitch, value.roll);

        public static explicit operator RealVector3(EulerAngles value) => new RealVector3(value.Yaw, value.Pitch, value.Roll);
        public static explicit operator EulerAngles(RealVector3 value) => new EulerAngles(value.X, value.Y, value.Z);
        
        public static explicit operator Quaternion(EulerAngles value)
        {
            //TODO: ToQuaternion() function with CoordSys parameter
            //currently assuming DX coordinate system
            return Quaternion.CreateFromAxisAngle(Vector3.UnitX, value.Roll)
                * Quaternion.CreateFromAxisAngle(-Vector3.UnitY, value.Pitch)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, value.Yaw);
        }

        #endregion
    }
}
