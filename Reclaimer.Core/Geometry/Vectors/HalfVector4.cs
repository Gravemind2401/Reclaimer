using System;
using System.Numerics;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]    
    public record struct HalfVector4(Half X, Half Y, Half Z, Half W) : IVector4, IReadOnlyVector4
    {
        public HalfVector4(Vector4 value)
            : this((Half)value.X, (Half)value.Y, (Half)value.Z, (Half)value.W)
        { }
        
        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region Cast Operators

        public static explicit operator Vector4(HalfVector4 value) => new Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
        public static explicit operator HalfVector4(Vector4 value) => new HalfVector4(value);

        #endregion

        #region IVector4

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
        float IVector3.Z
        {
            get => (float)Z;
            set => Z = (Half)value;
        }
        float IVector4.W
        {
            get => (float)W;
            set => W = (Half)value;
        }

        #endregion

        #region IReadOnlyVector4

        float IReadOnlyVector2.X => (float)X;
        float IReadOnlyVector2.Y => (float)Y;
        float IReadOnlyVector3.Z => (float)Z;
        float IReadOnlyVector4.W => (float)W;

        #endregion
    }
}
