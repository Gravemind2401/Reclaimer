using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    /// <summary>
    /// Contains matrices representing various coordinate systems.
    /// </summary>
    public static class CoordinateSystem
    {
        #region Static Coordinate Systems

        /// <summary>
        /// Returns the standard Z-up coordinate system with no scaling.
        /// </summary>
        public static Matrix4x4 Default => @default;

        /// <summary>
        /// Returns the coordinate system of Halo CE Anniversary.
        /// </summary>
        public static Matrix4x4 HaloCEX => haloCEX;

        /// <summary>
        /// Returns the coordinate system of Halo Wars.
        /// </summary>
        public static Matrix4x4 HaloWars => haloWars;

        private static readonly Matrix4x4 @default = Matrix4x4.Identity;

        private static readonly Matrix4x4 haloCEX =
            new Matrix4x4(
                1,  0,  0, 0, //forward
                0,  0, -1, 0, //right
                0,  1,  0, 0, //up
                0,  0,  0, 1);

        private static readonly Matrix4x4 haloWars =
            new Matrix4x4(
                0,  0,  1, 0, //forward
               -1,  0,  0, 0, //right
                0,  1,  0, 0, //up
                0,  0,  0, 1);

        #endregion

        /// <summary>
        /// Creates a transform matrix that can be used to convert coordinates from one system to another.
        /// </summary>
        /// <param name="origin">The origin coordinate system.</param>
        /// <param name="destination">The destination coordinate system.</param>
        /// <returns></returns>
        public static Matrix4x4 GetTransform(Matrix4x4 origin, Matrix4x4 destination)
        {
            if (!Matrix4x4.Invert(origin, out Matrix4x4 inverse))
                throw Exceptions.CoordSysNotConvertable();

            return inverse * destination;
        }

        //public static T TransformVector<T>(this Matrix4x4 matrix, T vector) where T : struct, IXMVector
        //{
        //    var vec4 = new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        //    vec4 = Vector4.Transform(vec4, matrix);

        //    var result = default(T);
        //    result.X = vec4.X;
        //    result.Y = vec4.Y;
        //    result.Z = vec4.Z;
        //    result.W = vec4.W;

        //    return result;
        //}
    }
}
