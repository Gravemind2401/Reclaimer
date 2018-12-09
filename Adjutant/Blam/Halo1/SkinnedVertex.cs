using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    [FixedSize(66)]
    public struct SkinnedVertex
    {
        [Offset(0)]
        public RealVector3D Position { get; set; }

        [Offset(12)]
        public RealVector3D Normal { get; set; }

        [Offset(24)]
        public RealVector3D Binormal { get; set; }

        [Offset(36)]
        public RealVector3D Tangent { get; set; }

        [Offset(48)]
        public RealVector2D TexCoords { get; set; }

        [Offset(56)]
        public byte NodeIndex1 { get; set; }

        [Offset(57)]
        public byte NodeIndex2 { get; set; }

        [Offset(58)]
        public RealVector2D NodeWeights { get; set; }

        public override string ToString() => Position.ToString();

        #region Equality Operators

        public static bool operator ==(SkinnedVertex vertex1, SkinnedVertex vertex2)
        {
            return vertex1.Position == vertex2.Position
                && vertex1.Normal == vertex2.Normal
                && vertex1.Binormal == vertex2.Binormal
                && vertex1.Tangent == vertex2.Tangent
                && vertex1.TexCoords == vertex2.TexCoords
                && vertex1.NodeIndex1 == vertex2.NodeIndex1
                && vertex1.NodeIndex2 == vertex2.NodeIndex2
                && vertex1.NodeWeights == vertex2.NodeWeights;
        }

        public static bool operator !=(SkinnedVertex vertex1, SkinnedVertex vertex2)
        {
            return !(vertex1 == vertex2);
        }

        public static bool Equals(SkinnedVertex vertex1, SkinnedVertex vertex2)
        {
            return vertex1.Position.Equals(vertex2.Position)
                && vertex1.Normal.Equals(vertex2.Normal)
                && vertex1.Binormal.Equals(vertex2.Binormal)
                && vertex1.Tangent.Equals(vertex2.Tangent)
                && vertex1.TexCoords.Equals(vertex2.TexCoords)
                && vertex1.NodeIndex1.Equals(vertex2.NodeIndex1)
                && vertex1.NodeIndex2.Equals(vertex2.NodeIndex2)
                && vertex1.NodeWeights.Equals(vertex2.NodeWeights);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is SkinnedVertex))
                return false;

            return SkinnedVertex.Equals(this, (SkinnedVertex)obj);
        }

        public bool Equals(SkinnedVertex value)
        {
            return SkinnedVertex.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode()
                ^ Normal.GetHashCode()
                ^ Binormal.GetHashCode()
                ^ Tangent.GetHashCode()
                ^ TexCoords.GetHashCode()
                ^ NodeIndex1.GetHashCode()
                ^ NodeIndex2.GetHashCode()
                ^ NodeWeights.GetHashCode();
        }

        #endregion
    }
}
