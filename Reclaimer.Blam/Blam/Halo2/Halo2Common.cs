using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    [FixedSize(16)]
    public class ResourceInfoBlock
    {
        [Offset(4)]
        public short Type0 { get; set; }

        [Offset(6)]
        public short Type1 { get; set; }

        [Offset(8)]
        public int Size { get; set; }

        [Offset(12)]
        public int Offset { get; set; }
    }

    public struct MeshResourceDetailsBlock
    {
        [Offset(52, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(40, MinVersion = (int)CacheType.Halo2Xbox)]
        public ushort IndexCount { get; set; }

        [Offset(168, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(108, MinVersion = (int)CacheType.Halo2Xbox)]
        public ushort NodeMapCount { get; set; }
    }

    [FixedSize(SizeOf)]
    public struct SubmeshDataBlock
    {
        public const int SizeOf = 72;

        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(6)]
        public ushort IndexStart { get; set; }

        [Offset(8)]
        public ushort IndexLength { get; set; }
    }

    public class SectionArgs
    {
        public GeometryClassification GeometryClassification { get; init; }
        public ushort VertexCount { get; init; }
        public ushort FaceCount { get; init; }
        public byte NodesPerVertex { get; init; }
        public DataPointer DataPointer { get; init; }
        public int DataSize { get; init; }
        public int BaseAddress { get; init; }
        public IReadOnlyList<ResourceInfoBlock> Resources { get; init; }
    }

    public class Halo2GeometryArgs
    {
        public ICacheFile Cache { get; init; }
        public IReadOnlyList<ShaderBlock> Shaders { get; init; }
        public IReadOnlyList<SectionArgs> Sections { get; init; }
        public bool IsRenderModel { get; init; }
    }

    internal static class Halo2Common
    {
        public static IEnumerable<Material> GetMaterials(IReadOnlyList<ShaderBlock> shaders)
        {
            for (var i = 0; i < shaders.Count; i++)
            {
                var tag = shaders[i].ShaderReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                var material = new Material
                {
                    Id = tag.Id,
                    Name = tag.FileName
                };

                var shader = tag?.ReadMetadata<shader>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                var bitmTag = shader.ShaderMaps[0].DiffuseBitmapReference.Tag;
                if (bitmTag == null)
                {
                    yield return material;
                    continue;
                }

                material.TextureMappings.Add(new TextureMapping
                {
                    Usage = TextureUsage.Diffuse,
                    Tiling = Vector2.One,
                    Texture = new Texture
                    {
                        Id = bitmTag.Id,
                        ContentProvider = bitmTag.ReadMetadata<bitmap>()
                    }
                });

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo2GeometryArgs args, out List<Material> materials)
        {
            var matLookup = materials = new List<Material>(args.Shaders.Count);
            matLookup.AddRange(GetMaterials(args.Shaders));

            var meshList = new List<Mesh>(args.Sections.Count);

            meshList.AddRange(args.Sections.Select((section, sectionIndex) =>
            {
                if (section.VertexCount == 0)
                    return null;

                var data = section.DataPointer.ReadData(section.DataSize);

                using (var ms = new MemoryStream(data))
                using (var reader = new EndianReader(ms, ByteOrder.LittleEndian))
                    return ReadMesh(args, reader, section, matLookup);
            }));

            return meshList;
        }

        private static Mesh ReadMesh(Halo2GeometryArgs args, EndianReader reader, SectionArgs section, List<Material> materials)
        {
            if (args.Cache.Metadata.CacheType < CacheType.Halo2Xbox)
                reader.ReadInt32(); //hklb

            var sectionInfo = reader.ReadObject<MeshResourceDetailsBlock>((int)args.Cache.Metadata.CacheType);

            var (indexType, vertexType, nodeMapType) = args.Cache.Metadata.CacheType < CacheType.Halo2Xbox
                ? (48, 92, 164)
                : (32, 56, 100);

            var submeshResource = section.Resources[0];
            var indexResource = section.Resources.FirstOrDefault(r => r.Type0 == indexType);
            var vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 0);
            var uvResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 1);
            var normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 2);
            var nodeMapResource = section.Resources.FirstOrDefault(r => r.Type0 == nodeMapType);

            reader.Seek(section.BaseAddress + submeshResource.Offset, SeekOrigin.Begin);
            var submeshes = reader.ReadArray<SubmeshDataBlock>(submeshResource.Size / SubmeshDataBlock.SizeOf);

            var mesh = new Mesh();
            mesh.Segments.AddRange(
                submeshes.Select(s => new MeshSegment
                {
                    Material = materials.ElementAtOrDefault(s.ShaderIndex),
                    IndexStart = s.IndexStart,
                    IndexLength = s.IndexLength
                })
            );

            var indexFormat = section.FaceCount * 3 == sectionInfo.IndexCount
                ? IndexFormat.TriangleList
                : IndexFormat.TriangleStrip;

            reader.Seek(section.BaseAddress + indexResource.Offset, SeekOrigin.Begin);
            mesh.IndexBuffer = IndexBuffer.FromArray(reader.ReadArray<ushort>(sectionInfo.IndexCount), indexFormat);

            if (args.IsRenderModel)
                ReadRenderModelMeshData();
            else
                ReadBspMeshData();

            return mesh;

            void ReadBspMeshData()
            {
                var positionBuffer = new VectorBuffer<RealVector3>(section.VertexCount);
                var texCoordsBuffer = new VectorBuffer<RealVector2>(section.VertexCount);
                var normalBuffer = new VectorBuffer<HenDN3>(section.VertexCount);

                mesh.VertexBuffer = new VertexBuffer();
                mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
                mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);
                mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

                var vertexSize = vertexResource.Size / section.VertexCount;
                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                    positionBuffer[i] = new RealVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                    texCoordsBuffer[i] = new RealVector2(reader.ReadSingle(), reader.ReadSingle());
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                    normalBuffer[i] = new HenDN3(reader.ReadUInt32());
                }
            }

            void ReadRenderModelMeshData()
            {
                var positionBuffer = new VectorBuffer<UInt16N4>(section.VertexCount);
                var texCoordsBuffer = new VectorBuffer<UInt16N2>(section.VertexCount);
                var normalBuffer = new VectorBuffer<HenDN3>(section.VertexCount);

                mesh.VertexBuffer = new VertexBuffer();
                mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
                mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);
                mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

                var vertexSize = vertexResource.Size / section.VertexCount;
                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                    positionBuffer[i] = new UInt16N4((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), default);
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + uvResource.Offset + i * 4, SeekOrigin.Begin);
                    texCoordsBuffer[i] = new UInt16N2((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue));
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                    normalBuffer[i] = new HenDN3(reader.ReadUInt32());
                }

                var nodeMap = Array.Empty<byte>();
                if (nodeMapResource != null)
                {
                    reader.Seek(section.BaseAddress + nodeMapResource.Offset, SeekOrigin.Begin);
                    nodeMap = reader.ReadBytes(sectionInfo.NodeMapCount);
                }

                if (section.GeometryClassification == GeometryClassification.Rigid)
                {
                    if (section.NodesPerVertex == 0)
                        mesh.BoneIndex = 0;
                    else if (section.NodesPerVertex == 1 && nodeMap.Length > 0)
                        mesh.BoneIndex = nodeMap[0];
                    else
                        throw new NotSupportedException();

                    return;
                }

                var blendIndexBuffer = new VectorBuffer<UByte4>(section.VertexCount);
                var blendWeightBuffer = new VectorBuffer<RealVector4>(section.VertexCount);

                mesh.VertexBuffer.BlendIndexChannels.Add(blendIndexBuffer);
                mesh.VertexBuffer.BlendWeightChannels.Add(blendWeightBuffer);

                for (var i = 0; i < section.VertexCount; i++)
                {
                    UByte4 blendIndices = default;
                    RealVector4 blendWeights = default;

                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize + 6, SeekOrigin.Begin);

                    if (section.GeometryClassification == GeometryClassification.RigidBoned)
                    {
                        blendIndices = new UByte4(reader.ReadByte(), default, default, default);
                        blendWeights = new RealVector4(1f, default, default, default);
                        reader.ReadByte();
                    }
                    else if (section.GeometryClassification == GeometryClassification.Skinned)
                    {
                        if (section.NodesPerVertex == 2 || section.NodesPerVertex == 4)
                            reader.ReadInt16();

                        var nodes = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() : byte.MinValue).ToList();
                        var weights = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() / (float)byte.MaxValue : 0).ToList();

                        if (section.NodesPerVertex == 1 && weights.Sum() == 0)
                            weights[0] = 1;

                        blendIndices = new UByte4(nodes[0], nodes[1], nodes[2], nodes[3]);
                        blendWeights = new RealVector4(weights[0], weights[1], weights[2], weights[3]);
                    }

                    if (nodeMap.Length > 0)
                    {
                        var temp = blendIndices;
                        blendIndices = new UByte4
                        {
                            X = section.NodesPerVertex > 0 ? nodeMap[temp.X] : byte.MinValue,
                            Y = section.NodesPerVertex > 1 ? nodeMap[temp.Y] : byte.MinValue,
                            Z = section.NodesPerVertex > 2 ? nodeMap[temp.Z] : byte.MinValue,
                            W = section.NodesPerVertex > 3 ? nodeMap[temp.W] : byte.MinValue,
                        };
                    }

                    blendIndexBuffer[i] = blendIndices;
                    blendWeightBuffer[i] = blendWeights;
                }
            }
        }
    }
}
