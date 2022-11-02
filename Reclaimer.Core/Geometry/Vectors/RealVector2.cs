using System;
using System.Numerics;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]    
    public record struct RealVector2(float X, float Y) : IVector2, IReadOnlyVector2
    {
        public RealVector2(Vector2 value)
            : this(value.X, value.Y)
        { }
        
        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region Cast Operators

        public static explicit operator Vector2(RealVector2 value) => new Vector2(value.X, value.Y);
        public static explicit operator RealVector2(Vector2 value) => new RealVector2(value);

        #endregion
    }
}
