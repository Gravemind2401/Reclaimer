using System.Numerics;

namespace Reclaimer.Geometry
{
    public record struct CoordinateSystem2(Vector3 ForwardVector, Vector3 RightVector, Vector3 UpVector, float UnitScale)
    {
        /// <summary>
        /// The default coordinate system. This is equivalent to using a world matrix of <see cref="Matrix4x4.Identity"/>.
        /// </summary>
        /// <remarks>
        /// The forward, right and up vectors are <see cref="Vector3.UnitX"/>, <see cref="Vector3.UnitY"/> and <see cref="Vector3.UnitZ"/> respectively.
        /// </remarks>
        public static readonly CoordinateSystem2 Default = new CoordinateSystem2(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, StandardUnits.Default);

        /// <summary>
        /// Returns a transform matrix that can be used to convert from one coordinate system to another.
        /// </summary>
        /// <param name="origin">The origin coordinate system.</param>
        /// <param name="destination">The destination coordinate system.</param>
        public static Matrix4x4 GetTransform(CoordinateSystem2 origin, CoordinateSystem2 destination)
        {
            if (origin == destination)
                return Matrix4x4.Identity;

            return Matrix4x4.Invert(origin.WorldMatrix, out var inverse)
                ? inverse * destination.WorldMatrix
                : throw new InvalidOperationException("No conversion exists between the given coordinate systems.");
        }

        /// <summary>
        /// Gets a transform representing the world matrix of the current coordinate system.
        /// </summary>
        public readonly Matrix4x4 WorldMatrix => new Matrix4x4(ForwardVector.X, ForwardVector.Y, ForwardVector.Z, 0, RightVector.X, RightVector.Y, RightVector.Z, 0, UpVector.X, UpVector.Y, UpVector.Z, 0, 0, 0, 0, 1);
    }
}
