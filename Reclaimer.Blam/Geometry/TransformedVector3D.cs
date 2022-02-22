using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public struct TransformedVector3D : IXMVector
    {
        private readonly Matrix4x4 transform;
        private readonly IXMVector source;
        private readonly bool isPoint;

        public TransformedVector3D(IXMVector source, Matrix4x4 transform, bool isPoint)
        {
            this.source = source;
            this.transform = transform;
            this.isPoint = isPoint;
        }

        public float X
        {
            get { return source.X * transform.M11 + source.Y * transform.M21 + source.Z * transform.M31 + (isPoint ? transform.M41 : 0); }
            set { throw new NotImplementedException(); }
        }

        public float Y
        {
            get { return source.X * transform.M12 + source.Y * transform.M22 + source.Z * transform.M32 + (isPoint ? transform.M42 : 0); }
            set { throw new NotImplementedException(); }
        }

        public float Z
        {
            get { return source.X * transform.M13 + source.Y * transform.M23 + source.Z * transform.M33 + (isPoint ? transform.M43 : 0); }
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
