using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Reflection;
using System.Xml;

namespace Reclaimer.Blam.Common
{
    public class XmlVertexBuilder
    {
        private readonly Dictionary<int, XmlVertexLayout> vertexLayouts;

        public XmlVertexBuilder(string xmlDefinitions)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlDefinitions);

            vertexLayouts = doc.SelectNodes("//*[local-name()='vertex']").OfType<XmlNode>()
                .Select(vertexNode => new XmlVertexLayout
                {
                    TypeId = ParseInt(vertexNode.Attributes["type"].Value),
                    Name = vertexNode.Attributes["name"].Value,
                    Size = vertexNode.Attributes["size"] == null ? null : ParseInt(vertexNode.Attributes["size"].Value),
                    Fields = vertexNode.SelectNodes("./*[local-name()='value']").OfType<XmlNode>()
                        .Select(valueNode => new XmlVertexValue
                        {
                            Stream = ParseInt(valueNode.Attributes["stream"].Value),
                            Offset = ParseInt(valueNode.Attributes["offset"].Value),
                            DataType = (VectorType)Enum.Parse(typeof(VectorType), valueNode.Attributes[XmlVertexField.Type].Value, true),
                            Usage = valueNode.Attributes["usage"].Value,
                            UsageIndex = ParseInt(valueNode.Attributes["usageIndex"].Value)
                        }).ToList()
                }).ToDictionary(v => v.TypeId);

            static int ParseInt(string value) => int.TryParse(value, out var result) ? result : Convert.ToInt32(value, 16);
        }

        public VertexBuffer CreateVertexBuffer(int typeId, int count, byte[] data)
        {
            var layout = vertexLayouts[typeId];
            var vertexBuffer = new VertexBuffer();

            var lastField = layout.Fields.Where(f => f.Stream == 0).OrderBy(f => f.Offset).Last();
            var fieldType = GetFieldType(lastField.DataType);
            var vertexSize = layout.Size ?? (lastField.Offset + IBufferable.GetSizeOf(fieldType));

            foreach (var field in layout.Fields)
            {
                var vectorType = GetFieldType(field.DataType);
                if (vectorType == null)
                    continue;

                var method = GetType().GetMethod(nameof(CreateBuffer), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(vectorType);
                var buffer = (IVectorBuffer)method.Invoke(this, new object[] { data, count, 0, vertexSize, field.Offset });
                GetVectorChannel(field.Usage, vertexBuffer).Add(buffer);
            }

            return vertexBuffer;
        }

        private static Type GetFieldType(VectorType vectorType)
        {
            return vectorType switch
            {
                VectorType.Float16_2 => typeof(HalfVector2),
                //VectorType.Float16_3 => typeof(HalfVector3),
                VectorType.Float16_4 => typeof(HalfVector4),
                VectorType.Float32_2 => typeof(RealVector2),
                VectorType.Float32_3 => typeof(RealVector3),
                VectorType.Float32_4 => typeof(RealVector4),
                VectorType.DecN4 => typeof(DecN4),
                VectorType.HenDN3 => typeof(HenDN3),
                VectorType.DHenN3 => typeof(DHenN3),
                VectorType.UDecN4 => typeof(UDecN4),
                VectorType.UHenDN3 => typeof(UHenDN3),
                VectorType.UDHenN3 => typeof(UDHenN3),
                VectorType.UInt8_4 => typeof(UByte4),
                //VectorType.Int8_N2 => typeof(ByteN2),
                VectorType.Int8_N4 => typeof(ByteN4),
                VectorType.UInt8_N2 => typeof(UByteN2),
                VectorType.UInt8_N4 => typeof(UByteN4),
                VectorType.Int16_N2 => typeof(Int16N2),
                //VectorType.Int16_N3 => typeof(Int16N3),
                VectorType.Int16_N4 => typeof(Int16N4),
                VectorType.UInt16_N2 => typeof(UInt16N2),
                //VectorType.UInt16_N3 => typeof(UInt16N3),
                VectorType.UInt16_N4 => typeof(UInt16N4),
                VectorType.NxAAA2 => typeof(NxAAA2),
                VectorType.UxAAC0 => typeof(UxAAC0),
                _ => null
            };
        }

        private static IList<IReadOnlyList<IVector>> GetVectorChannel(string usage, VertexBuffer vertexBuffer)
        {
            return usage switch
            {
                XmlVertexUsage.Position => vertexBuffer.PositionChannels,
                XmlVertexUsage.TexCoords => vertexBuffer.TextureCoordinateChannels,
                XmlVertexUsage.Normal => vertexBuffer.NormalChannels,
                XmlVertexUsage.Binormal => vertexBuffer.BinormalChannels,
                XmlVertexUsage.Tangent => vertexBuffer.TangentChannels,
                XmlVertexUsage.BlendWeight => vertexBuffer.BlendWeightChannels,
                XmlVertexUsage.BlendIndices => vertexBuffer.BlendIndexChannels,
                XmlVertexUsage.Color => vertexBuffer.ColorChannels,
                _ => null
            };
        }

        private static IVectorBuffer CreateBuffer<T>(byte[] buffer, int count, int start, int stride, int offset)
            where T : struct, IBufferableVector<T>
        {
            return new VectorBuffer<T>(buffer, count, start, stride, offset);
        }

        #region Nested Types
        private enum VectorType
        {
            Float16_2,
            Float16_4,

            Float32_2,
            Float32_3,
            Float32_4,

            DHenN3,
            UDHenN3,

            HenDN3,
            UHenDN3,

            DecN4,
            UDecN4,

            Int8_N4,
            UInt8_2,
            UInt8_3,
            UInt8_4,
            UInt8_N2,
            UInt8_N3,
            UInt8_N4,

            Int16_N2,
            Int16_N3,
            Int16_N4,
            UInt16_2,
            UInt16_4,
            UInt16_N2,
            UInt16_N4,

            D3DColour,

            NxAAA2,
            UxAAC0
        }

        private static class XmlVertexField
        {
            public const string Type = "type";
            public const string Stream = "stream";
            public const string Offset = "offset";
            public const string Usage = "usage";
        }

        private static class XmlVertexUsage
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

        [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
        private sealed class XmlVertexLayout
        {
            public int TypeId { get; init; }
            public string Name { get; init; }
            public int? Size { get; init; }
            public List<XmlVertexValue> Fields { get; init; }
        }

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private sealed class XmlVertexValue
        {
            public int Stream { get; init; }
            public int Offset { get; init; }
            public VectorType DataType { get; init; }
            public string Usage { get; init; }
            public int UsageIndex { get; init; }

            private string GetDebuggerDisplay() => $"[{DataType}] {Usage}";
        }
        #endregion
    }
}
