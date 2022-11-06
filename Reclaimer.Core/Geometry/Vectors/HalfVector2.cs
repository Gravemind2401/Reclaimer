using System;
using System.Numerics;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]    
    public record struct HalfVector2(Half X, Half Y) : IVector2, IReadOnlyVector2
    {
        public HalfVector2(Vector2 value)
            : this((Half)value.X, (Half)value.Y)
        { }
        
        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region Cast Operators

        public static explicit operator Vector2(HalfVector2 value) => new Vector2((float)value.X, (float)value.Y);
        public static explicit operator HalfVector2(Vector2 value) => new HalfVector2(value);

        #endregion

        #region IVector2

        float IVector2.X
        {
            get => (float)X;
            set => X = (Half)value;
        }
        float IVector2.Y
        {
            get => (float)Y;
            set => Y = (Half)value;
        }

        #endregion

        #region IReadOnlyVector2

        float IReadOnlyVector2.X => (float)X;
        float IReadOnlyVector2.Y => (float)Y;

        #endregion
    }
}
