using System;
using System.Runtime.CompilerServices;

namespace Reclaimer.Geometry
{
    public interface IVector
    {
        static int Dimensions { get; } //TODO: abstract static in C# 11

        sealed int InstanceDimensions => Dimensions;

        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float W { get; set; }

        internal void ThrowInvalidDimension([CallerMemberName] string caller = null) => throw new NotSupportedException($"The {caller} property cannot be set on a {Dimensions}-dimensional vector.");
    }

    public interface IVector2 : IVector
    {
        static int Dimensions => 2;

        float IVector.Z
        {
            get => default;
            set => ThrowInvalidDimension();
        }

        float IVector.W
        {
            get => default;
            set => ThrowInvalidDimension();
        }
    }

    public interface IVector3 : IVector
    {
        static int Dimensions => 3;

        float IVector.W
        {
            get => default;
            set => ThrowInvalidDimension();
        }
    }

    public interface IVector4 : IVector
    {
        static int Dimensions => 4;
    }
}
