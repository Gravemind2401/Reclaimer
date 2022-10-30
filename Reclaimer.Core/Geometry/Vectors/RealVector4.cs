using System;
using System.Numerics;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]    
    public record struct RealVector4(float X, float Y, float Z, float W)
    {
        public RealVector4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }
        
        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region Cast Operators

        public static explicit operator Vector4(RealVector4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator RealVector4(Vector4 value) => new RealVector4(value);

        #endregion
    }
}
