using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public struct TransformedVector2D : IXMVector
    {
        private readonly Matrix4x4 transform;
        private readonly IXMVector source;
        private readonly bool isPoint;

        public TransformedVector2D(IXMVector source, Matrix4x4 transform, bool isPoint)
        {
            this.source = source;
            this.transform = transform;
            this.isPoint = isPoint;
        }

        public float X
        {
            get { return source.X * transform.M11 + source.Y * transform.M21 + (isPoint ? transform.M41 : 0); }
            set { throw new NotImplementedException(); }
        }

        public float Y
        {
            get { return source.X * transform.M12 + source.Y * transform.M22 + (isPoint ? transform.M42 : 0); }
            set { throw new NotImplementedException(); }
        }

        public float Z
        {
            get { return source.Z; }
            set { throw new NotImplementedException(); }
        }

        public float W
        {
            get { return source.W; }
            set { throw new NotImplementedException(); }
        }

        public float Length => source.Length;

        public VectorType VectorType => source.VectorType;
    }
}
