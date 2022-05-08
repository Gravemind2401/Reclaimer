using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Blam.Common
{
    public static class XmlVertexField
    {
        public const string Type = "type";
        public const string Stream = "stream";
        public const string Offset = "offset";
        public const string Usage = "usage";
    }

    public static class XmlVertexUsage
    {
        public const string Position = "position";
        public const string TexCoords = "texcoords";
        public const string Normal = "normal";
        public const string Binormal = "binormal";
        public const string Tangent = "tangent";
        public const string BlendIndices = "blendindices";
        public const string BlendWeight = "blendweight";
        public const string Color = "color";
    }

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

            var origin = reader.BaseStream.Position;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment)
                    continue;

                var stream = int.Parse(child.Attributes[XmlVertexField.Stream].Value, CultureInfo.InvariantCulture);
                if (stream > 0)
                    throw new NotSupportedException();

                var offset = Convert.ToInt32(child.Attributes[XmlVertexField.Offset].Value, 16);
                var type = (VectorType)Enum.Parse(typeof(VectorType), child.Attributes[XmlVertexField.Type].Value, true);
                var usage = child.Attributes[XmlVertexField.Usage].Value;

                reader.BaseStream.Position = origin + offset;
                var value = ReadValue(type, reader);

                switch (usage)
                {
                    case XmlVertexUsage.Position:
                        positions.Add(value);
                        break;
                    case XmlVertexUsage.TexCoords:
                        texcoords.Add(value);
                        break;
                    case XmlVertexUsage.Normal:
                        normals.Add(value);
                        break;
                    case XmlVertexUsage.Binormal:
                        binormals.Add(value);
                        break;
                    case XmlVertexUsage.Tangent:
                        tangents.Add(value);
                        break;
                    case XmlVertexUsage.BlendIndices:
                        blendindices.Add(value);
                        break;
                    case XmlVertexUsage.BlendWeight:
                        blendweights.Add(value);
                        break;
                    case XmlVertexUsage.Color:
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
                case VectorType.Float16_2: return new RealVector2D(Half.ToHalf(reader.ReadUInt16()), Half.ToHalf(reader.ReadUInt16()));
                case VectorType.Float16_4: return new RealVector4D(Half.ToHalf(reader.ReadUInt16()), Half.ToHalf(reader.ReadUInt16()), Half.ToHalf(reader.ReadUInt16()), Half.ToHalf(reader.ReadUInt16()));
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
