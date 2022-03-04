using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo1
{
    public class CompressedVertex : IVertex
    {
        public RealVector3D Position { get; set; }

        public HenDN3 Normal { get; set; }

        public HenDN3 Binormal { get; set; }

        public HenDN3 Tangent { get; set; }

        public RealVector2D TexCoords { get; set; }

        public short NodeIndex1 { get; set; }

        public short NodeIndex2 { get; set; }

        public RealVector2D NodeWeights { get; set; }

        public override string ToString() => Position.ToString();

        public CompressedVertex(DependencyReader reader)
        {
            Position = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Normal = new HenDN3(reader.ReadUInt32());
            Binormal = new HenDN3(reader.ReadUInt32());
            Tangent = new HenDN3(reader.ReadUInt32());
            TexCoords = new RealVector2D(reader.ReadInt16() / (float)short.MaxValue, reader.ReadInt16() / (float)short.MaxValue);
            NodeIndex1 = (short)(reader.ReadByte() / 3);
            NodeIndex2 = (short)(reader.ReadByte() / 3);

            var node0Weight = reader.ReadUInt16() / (float)short.MaxValue;
            NodeWeights = new RealVector2D(node0Weight, 1 - node0Weight);
        }

        #region IVertex

        IReadOnlyList<IXMVector> IVertex.Position => new IXMVector[] { Position };

        IReadOnlyList<IXMVector> IVertex.TexCoords => new IXMVector[] { TexCoords };

        IReadOnlyList<IXMVector> IVertex.Normal => new IXMVector[] { Normal };

        IReadOnlyList<IXMVector> IVertex.Binormal => new IXMVector[] { Binormal };

        IReadOnlyList<IXMVector> IVertex.Tangent => new IXMVector[] { Tangent };

        IReadOnlyList<IXMVector> IVertex.BlendIndices => new IXMVector[] { new RealVector2D(NodeIndex1, NodeIndex2) };

        IReadOnlyList<IXMVector> IVertex.BlendWeight => new IXMVector[] { NodeWeights };

        IReadOnlyList<IXMVector> IVertex.Color => new IXMVector[0];

        #endregion
    }
}
