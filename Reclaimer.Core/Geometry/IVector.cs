using System.Runtime.CompilerServices;

namespace Reclaimer.Geometry
{
    public interface IVector
    {
        static virtual int Dimensions => throw new NotImplementedException();

        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float W { get; set; }

        internal void ThrowInvalidDimension(int dimensions, [CallerMemberName] string caller = null) => throw new NotSupportedException($"The {caller} property cannot be set on a {dimensions}-dimensional vector.");
    }

    public interface IVector2 : IVector
    {
        static int IVector.Dimensions => 2;

        float IVector.Z
        {
            get => default;
            set => ThrowInvalidDimension(2);
        }

        float IVector.W
        {
            get => default;
            set => ThrowInvalidDimension(2);
        }
    }

    public interface IVector3 : IVector
    {
        static int IVector.Dimensions => 3;

        float IVector.W
        {
            get => default;
            set => ThrowInvalidDimension(3);
        }
    }

    public interface IVector4 : IVector
    {
        static int IVector.Dimensions => 4;
    }
}
