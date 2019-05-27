using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Definitions
{
    public class XmlVertex : IVertex
    {
        private readonly List<IXMVector> positions = new List<IXMVector>();
        private readonly List<IXMVector> texcoords = new List<IXMVector>();
        private readonly List<IXMVector> normals = new List<IXMVector>();
        private readonly List<IXMVector> binormals = new List<IXMVector>();
        private readonly List<IXMVector> tangents = new List<IXMVector>();
        private readonly List<IXMVector> blendindices = new List<IXMVector>();
        private readonly List<IXMVector> blendweights = new List<IXMVector>();
        private readonly List<IXMVector> colors = new List<IXMVector>();

        public IReadOnlyList<IXMVector> Position => positions;
        public IReadOnlyList<IXMVector> TexCoords => texcoords;
        public IReadOnlyList<IXMVector> Normal => normals;
        public IReadOnlyList<IXMVector> Binormal => binormals;
        public IReadOnlyList<IXMVector> Tangent => tangents;
        public IReadOnlyList<IXMVector> BlendIndices => blendindices;
        public IReadOnlyList<IXMVector> BlendWeight => blendweights;
        public IReadOnlyList<IXMVector> Color => colors;

        public XmlVertex(EndianReader reader, XmlNode node)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (node == null)
                throw new ArgumentNullException(nameof(node));

            foreach (XmlNode child in node.ChildNodes)
            {
                var stream = int.Parse(child.Attributes["stream"].Value, CultureInfo.InvariantCulture);
                if (stream > 0) throw new NotSupportedException();

                var offset = Convert.ToInt32(child.Attributes["offset"].Value, 16);
                var type = (VectorType)Enum.Parse(typeof(VectorType), child.Attributes["type"].Value, true);
                var usage = child.Attributes["usage"].Value;

                var value = ReadValue(type, reader);
                switch (usage)
                {
                    case "position":
                        positions.Add(value);
                        break;
                    case "texcoords":
                        texcoords.Add(value);
                        break;
                    case "normal":
                        normals.Add(value);
                        break;
                    case "binormal":
                        binormals.Add(value);
                        break;
                    case "tangent":
                        tangents.Add(value);
                        break;
                    case "blendindices":
                        blendindices.Add(value);
                        break;
                    case "blendweight":
                        blendweights.Add(value);
                        break;
                    case "color":
                        colors.Add(value);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private static IXMVector ReadValue(VectorType type, EndianReader reader)
        {
            switch (type)
            {
                case VectorType.DecN4: return new DecN4(reader.ReadUInt32());
                case VectorType.DHenN3: return new DHenN3(reader.ReadUInt32());
                case VectorType.Float16_2: break;
                case VectorType.Float16_4: break;
                case VectorType.Float32_2: return new RealVector2D(reader.ReadSingle(), reader.ReadSingle());
                case VectorType.Float32_3: return new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case VectorType.Float32_4: return new RealVector4D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case VectorType.HenDN3: return new HenDN3(reader.ReadUInt32());
                case VectorType.Int16_N2: return new Int16N2(reader.ReadInt16(), reader.ReadInt16());
                case VectorType.Int16_N3: return new Int16N3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                case VectorType.Int16_N4: return new Int16N4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                case VectorType.Int8_N4: break;
                case VectorType.UDecN4: return new UDecN4(reader.ReadUInt32());
                case VectorType.UDHenN3: return new UDHenN3(reader.ReadUInt32());
                case VectorType.UHenDN3: return new UHenDN3(reader.ReadUInt32());
                case VectorType.UInt16_2: break;
                case VectorType.UInt16_4: break;
                case VectorType.UInt16_N2: return new UInt16N2(reader.ReadUInt16(), reader.ReadUInt16());
                case VectorType.UInt16_N4: return new UInt16N4(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
                case VectorType.UInt8_2: break;
                case VectorType.UInt8_3: break;
                case VectorType.UInt8_4: return new UByte4(reader.ReadUInt32(ByteOrder.LittleEndian));
                case VectorType.UInt8_N2: return new UByteN2(reader.ReadUInt16(ByteOrder.LittleEndian));
                case VectorType.UInt8_N3: break;
                case VectorType.UInt8_N4: return new UByteN4(reader.ReadUInt32(ByteOrder.LittleEndian));
            }

            throw new NotSupportedException();
        }
    }
}
