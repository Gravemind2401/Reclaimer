﻿using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct HalfVector3(Half X, Half Y, Half Z) : IVector3, IReadOnlyVector3, IBufferableVector<HalfVector3>
    {
        private const int packSize = 2;
        private const int structureSize = 6;

        public HalfVector3(Vector3 value)
            : this((Half)value.X, (Half)value.Y, (Half)value.Z)
        { }

        private HalfVector3(ReadOnlySpan<Half> values)
            : this(values[0], values[1], values[2])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static HalfVector3 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new HalfVector3(MemoryMarshal.Cast<byte, Half>(buffer));
        void IBufferable<HalfVector3>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<Half, byte>(new[] { X, Y, Z }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(HalfVector3 value) => new Vector3((float)value.X, (float)value.Y, (float)value.Z);
        public static explicit operator HalfVector3(Vector3 value) => new HalfVector3(value);

        #endregion

        #region IVector3

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

        #endregion

        #region IReadOnlyVector3

        float IReadOnlyVector2.X => (float)X;
        float IReadOnlyVector2.Y => (float)Y;
        float IReadOnlyVector3.Z => (float)Z;

        #endregion
    }
}