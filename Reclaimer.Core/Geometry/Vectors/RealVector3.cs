using System;
using System.Numerics;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]    
    public record struct RealVector3(float X, float Y, float Z)
    {
        public RealVector3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }
        
        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region Cast Operators

        public static explicit operator Vector3(RealVector3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator RealVector3(Vector3 value) => new RealVector3(value);

        #endregion
    }
}
