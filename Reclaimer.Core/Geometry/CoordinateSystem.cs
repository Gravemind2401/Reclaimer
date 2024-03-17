namespace Reclaimer.Geometry
{
    /// <summary>
    /// A structure representing the orientation and scale of a 3d scene.
    /// </summary>
    /// <param name="ForwardVector">A unit vector pointing in the forward direction.</param>
    /// <param name="RightVector">A unit vector pointing in the right-hand direction.</param>
    /// <param name="UpVector">A unit vector pointing in the up direction.</param>
    /// <param name="UnitScale">The number of millimeters per world unit.</param>
    public record struct CoordinateSystem(Vector3 ForwardVector, Vector3 RightVector, Vector3 UpVector, float UnitScale)
    {
        /// <summary>
        /// The default coordinate system. This is equivalent to using a world matrix of <see cref="Matrix4x4.Identity"/>.
        /// </summary>
        /// <remarks>
        /// The forward, right and up vectors are <see cref="Vector3.UnitX"/>, <see cref="Vector3.UnitY"/> and <see cref="Vector3.UnitZ"/> respectively.
        /// </remarks>
        public static readonly CoordinateSystem Default = new CoordinateSystem(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, StandardUnits.Default);

        /// <summary>
        /// The coordinate system used by Saber3D geometry in Halo CE Anniversary.
        /// </summary>
        /// <remarks>
        /// The forward, right and up vectors are <see cref="Vector3.UnitX"/>, -<see cref="Vector3.UnitZ"/> and <see cref="Vector3.UnitY"/> respectively.
        /// </remarks>
        public static readonly CoordinateSystem HaloCEX = new CoordinateSystem(Vector3.UnitX, -Vector3.UnitZ, Vector3.UnitY, StandardUnits.Default);

        /// <summary>
        /// The coordinate system used by Halo Wars geometry.
        /// </summary>
        /// <remarks>
        /// The forward, right and up vectors are <see cref="Vector3.UnitZ"/>, -<see cref="Vector3.UnitX"/> and <see cref="Vector3.UnitY"/> respectively.
        /// </remarks>
        public static readonly CoordinateSystem HaloWars = new CoordinateSystem(Vector3.UnitZ, -Vector3.UnitX, Vector3.UnitY, StandardUnits.Default);

        /// <summary>
        /// Returns a transform matrix that can be used to convert from one coordinate system to another.
        /// </summary>
        /// <param name="origin">The origin coordinate system.</param>
        /// <param name="destination">The destination coordinate system.</param>
        /// <param name="scaled">If <see langword="true"/>, the <see cref="UnitScale"/> transform will be included in the resulting matrix.</param>
        public static Matrix4x4 GetTransform(CoordinateSystem origin, CoordinateSystem destination, bool scaled)
        {
            var (origMatrix, destMatrix) = scaled ? (origin.ScaledWorldMatrix, destination.ScaledWorldMatrix) : (origin.WorldMatrix, destination.WorldMatrix);
            return GetTransform(origMatrix, destMatrix);
        }

        /// <inheritdoc cref="GetTransform(CoordinateSystem, CoordinateSystem, bool)"/>
        public static Matrix4x4 GetTransform(Matrix4x4 origin, Matrix4x4 destination)
        {
            if (origin == destination)
                return Matrix4x4.Identity;

            return Matrix4x4.Invert(origin, out var inverse)
                ? destination * inverse
                : throw new InvalidOperationException("No conversion exists between the given coordinate systems.");
        }

        /// <summary>
        /// Gets a copy of the current coordinate system with the <see cref="UnitScale"/> property set to a new value.
        /// </summary>
        /// <param name="unitScale">The new scale value.</param>
        public readonly CoordinateSystem WithScale(float unitScale) => this with { UnitScale = unitScale };

        /// <summary>
        /// Gets a transform representing the world matrix of the current coordinate system.
        /// </summary>
        /// <remarks>
        /// The scale component is expressed in world units (1 world unit = <see cref="UnitScale"/>).
        /// </remarks>
        public readonly Matrix4x4 WorldMatrix => new Matrix4x4(ForwardVector.X, ForwardVector.Y, ForwardVector.Z, 0, RightVector.X, RightVector.Y, RightVector.Z, 0, UpVector.X, UpVector.Y, UpVector.Z, 0, 0, 0, 0, 1);

        /// <summary>
        /// Gets a transform representing the world matrix of the current coordinate system.
        /// </summary>
        /// <remarks>
        /// The scale component is expressed in millimeters (1 world unit = 1 millimeter).
        /// </remarks>
        public readonly Matrix4x4 ScaledWorldMatrix => Matrix4x4.CreateScale(UnitScale) * WorldMatrix;
    }
}
