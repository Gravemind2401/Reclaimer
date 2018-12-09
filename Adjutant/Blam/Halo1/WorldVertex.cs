using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    [FixedSize(56)]
    public struct WorldVertex
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

        public override string ToString() => Position.ToString();

        #region Equality Operators

        public static bool operator ==(WorldVertex vertex1, WorldVertex vertex2)
        {
            return vertex1.Position == vertex2.Position
                && vertex1.Normal == vertex2.Normal
                && vertex1.Binormal == vertex2.Binormal
                && vertex1.Tangent == vertex2.Tangent
                && vertex1.TexCoords == vertex2.TexCoords;
        }

        public static bool operator !=(WorldVertex vertex1, WorldVertex vertex2)
        {
            return !(vertex1 == vertex2);
        }

        public static bool Equals(WorldVertex vertex1, WorldVertex vertex2)
        {
            return vertex1.Position.Equals(vertex2.Position)
                && vertex1.Normal.Equals(vertex2.Normal)
                && vertex1.Binormal.Equals(vertex2.Binormal)
                && vertex1.Tangent.Equals(vertex2.Tangent)
                && vertex1.TexCoords.Equals(vertex2.TexCoords);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is WorldVertex))
                return false;

            return WorldVertex.Equals(this, (WorldVertex)obj);
        }

        public bool Equals(WorldVertex value)
        {
            return WorldVertex.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode()
                ^ Normal.GetHashCode()
                ^ Binormal.GetHashCode()
                ^ Tangent.GetHashCode()
                ^ TexCoords.GetHashCode();
        }

        #endregion
    }
}
