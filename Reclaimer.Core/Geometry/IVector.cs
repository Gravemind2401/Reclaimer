using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public interface IVector
    {
        static int Dimensions { get; } //TODO: abstract static in C# 11
        static bool IsReadOnly { get; } //TODO: abstract static in C# 11

        float X
        {
            get => throw new ArgumentOutOfRangeException();
            set => throw new ArgumentOutOfRangeException();
        }

        float Y
        {
            get => throw new ArgumentOutOfRangeException();
            set => throw new ArgumentOutOfRangeException();
        }

        float Z
        {
            get => throw new ArgumentOutOfRangeException();
            set => throw new ArgumentOutOfRangeException();
        }

        float W
        {
            get => throw new ArgumentOutOfRangeException();
            set => throw new ArgumentOutOfRangeException();
        }
    }

    public interface IVector2 : IVector
    {
        new float X { get; set; }
        new float Y { get; set; }

        float IVector.X
        {
            get => X;
            set => X = value;
        }

        float IVector.Y
        {
            get => Y;
            set => Y = value;
        }
    }

    public interface IVector3 : IVector2
    {
        new float Z { get; set; }

        float IVector.Z
        {
            get => Z;
            set => Z = value;
        }
    }

    public interface IVector4 : IVector3
    {
        new float W { get; set; }

        float IVector.W
        {
            get => W;
            set => W = value;
        }
    }
}
