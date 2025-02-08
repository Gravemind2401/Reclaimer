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
        [Offset(48, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(52, MinVersion = (int)CacheType.Halo2Beta, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(40, MinVersion = (int)CacheType.Halo2Xbox)]
        public ushort IndexCount { get; set; }

        [Offset(168, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(108, MinVersion = (int)CacheType.Halo2Xbox)]
        public ushort NodeMapCount { get; set; }
    }

    [FixedSize(100, MaxVersion = (int)CacheType.Halo2Beta)]
    [FixedSize(72, MinVersion = (int)CacheType.Halo2Beta)]
    public struct SubmeshDataBlock
    {
        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(8, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(6, MinVersion = (int)CacheType.Halo2Beta)]
        public ushort IndexStart { get; set; }

        [Offset(10, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(8, MinVersion = (int)CacheType.Halo2Beta)]
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
        public static IEnumerable<Material> GetMaterials(Halo2GeometryArgs args)
        {
            var bitmapCache = new Dictionary<int, BitmapTag>();
            var materialCache = new Dictionary<int, Material>();

            for (var i = 0; i < args.Shaders.Count; i++)
            {
                var tag = args.Shaders[i].ShaderReference.Tag;
                if (tag == null)
                {
                    yield return null;
                    continue;
                }

                //this is cached because often the same material tag will be used in multiple material slots
                if (materialCache.TryGetValue(tag.Id, out var material))
                {
                    yield return material;
                    continue;
                }

                materialCache.Add(tag.Id, material = new Material
                {
                    Id = tag.Id,
                    Name = tag.FileName
                });

                material.CustomProperties.Add(BlamConstants.SourceTagPropertyName, tag.TagName);

                var shader = tag?.ReadMetadata<ShaderTag>();
                if (shader == null)
                {
                    yield return material;
                    continue;
                }

                if (args.Cache.CacheType >= CacheType.Halo2Xbox)
                {
                    MaterialHelper.PopulateTextureMappings(bitmapCache, material, shader);
                }
                else
                {
                    var bitmTag = shader.RuntimeProperties[0].DiffuseBitmapReference.Tag;
                    if (bitmTag == null)
                    {
                        yield return material;
                        continue;
                    }

                    var texture = new Texture
                    {
                        Id = bitmTag.Id,
                        ContentProvider = bitmTag.ReadMetadata<BitmapTag>()
                    };

                    texture.CustomProperties.Add(BlamConstants.SourceTagPropertyName, bitmTag.TagName);

                    material.TextureMappings.Add(new TextureMapping
                    {
                        Usage = TextureUsage.Diffuse,
                        Tiling = Vector2.One,
                        Texture = texture
                    });
                }

                yield return material;
            }
        }

        public static List<Mesh> GetMeshes(Halo2GeometryArgs args, out List<Material> materials)
        {
            var matLookup = materials = new List<Material>(args.Shaders.Count);
            matLookup.AddRange(GetMaterials(args));

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

            ResourceInfoBlock submeshResource, indexResource, vertexResource, uvResource, normalsResource, nodeMapResource;
            submeshResource = indexResource = vertexResource = uvResource = normalsResource = nodeMapResource = default;

            ResourceInfoBlock vertexResource2, vertexResource3;
            vertexResource2 = vertexResource3 = default;

            if (args.Cache.CacheType <= CacheType.Halo2E3)
            {
                submeshResource = section.Resources.FirstOrDefault(r => r.Type0 == 164);
                indexResource = section.Resources.FirstOrDefault(r => r.Type0 == 152);
                uvResource = section.Resources.FirstOrDefault(r => r.Type0 == 44);
                normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == 76);
                nodeMapResource = section.Resources.FirstOrDefault(r => r.Type0 == 328);
                vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == 12);
                vertexResource2 = section.Resources.FirstOrDefault(r => r.Type0 == 108);
                vertexResource3 = section.Resources.FirstOrDefault(r => r.Type0 == 140 && r.Type1 == 0);
            }
            else
            {
                var (indexType, vertexType, nodeMapType) = args.Cache.CacheType < CacheType.Halo2Xbox
                    ? (48, 92, 164)
                    : (32, 56, 100);

                if (args.Cache.CacheType >= CacheType.MccHalo2)
                    nodeMapType = 104;

                submeshResource = section.Resources[0];
                indexResource = section.Resources.FirstOrDefault(r => r.Type0 == indexType);
                vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 0);
                uvResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 1);
                normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == vertexType && r.Type1 == 2);
                nodeMapResource = section.Resources.FirstOrDefault(r => r.Type0 == nodeMapType);
            }

            if (vertexResource == null)
                return null;

            var submeshSize = (int)FixedSizeAttribute.ValueFor(typeof(SubmeshDataBlock), (int)args.Cache.CacheType);
            reader.Seek(section.BaseAddress + submeshResource.Offset, SeekOrigin.Begin);
            var submeshes = reader.ReadArray<SubmeshDataBlock>(submeshResource.Size / submeshSize, (int)args.Cache.CacheType);

            if (args.Cache.CacheType == CacheType.Halo2E3)
            {
                sectionInfo.IndexCount = (ushort)submeshes.Sum(s => s.IndexLength);
                sectionInfo.NodeMapCount = (ushort)(nodeMapResource?.Size ?? default);
            }

            var mesh = new Mesh();
            try
            {
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
                {
                    if (args.Cache.Metadata.Platform == CachePlatform.Xbox)
                    {
                        if (args.Cache.CacheType == CacheType.Halo2E3)
                            ReadE3RenderModelMeshData();
                        else
                            ReadXboxRenderModelMeshData();
                    }
                    else
                        ReadPcRenderModelMeshData();
                }
                else
                {
                    if (args.Cache.Metadata.Platform == CachePlatform.Xbox)
                        ReadXboxBspMeshData();
                    else
                        ReadPcBspMeshData();
                }

                return mesh;
            }
            catch
            {
                return null;
            }

            void ReadXboxBspMeshData()
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

            void ReadPcBspMeshData()
            {
                var positionBuffer = new VectorBuffer<RealVector3>(section.VertexCount);
                var texCoordsBuffer = new VectorBuffer<RealVector2>(section.VertexCount);
                var normalBuffer = new VectorBuffer<RealVector3>(section.VertexCount);

                mesh.VertexBuffer = new VertexBuffer();
                mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
                mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);
                mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

                var vertexSize = vertexResource.Size / section.VertexCount;

                if (vertexSize != 12)
                    System.Diagnostics.Debugger.Break();

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                    positionBuffer[i] = reader.ReadBufferable<RealVector3>();
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                    texCoordsBuffer[i] = reader.ReadBufferable<RealVector2>();
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    //this contains 3x the data needed for normals, maybe also contains binormal and tangent?
                    reader.Seek(section.BaseAddress + normalsResource.Offset + i * 36, SeekOrigin.Begin);
                    normalBuffer[i] = reader.ReadBufferable<RealVector3>();
                }
            }

            void ReadE3RenderModelMeshData()
            {
                //in the E3 build there can be two blocks of vertex data - it seems to be one block for opaque and one for transparent vertices
                //they both join to form a single vertex array though; there is only one set of triangle indices between them

                //either there is always 1 vertex worth of padding, or the initials zeros are used to indicate size
                reader.Seek(section.BaseAddress + vertexResource.Offset, SeekOrigin.Begin);
                var vertexSize = 0;
                while (reader.ReadByte() == 0)
                    vertexSize++;

                var primaryVertexCount = vertexResource.Size / vertexSize - 1;
                var secondaryVertexCount = section.VertexCount - primaryVertexCount;

                //pretend the padding was never there
                vertexResource = new ResourceInfoBlock
                {
                    Offset = vertexResource.Offset + vertexSize,
                    Size = vertexResource.Size - vertexSize
                };

                var positionBuffer = new VectorBuffer<UInt16N4>(section.VertexCount);
                var texCoordsBuffer = new VectorBuffer<UInt16N2>(section.VertexCount);

                mesh.VertexBuffer = new VertexBuffer();
                mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
                mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);

                for (var i = 0; i < primaryVertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                    positionBuffer[i] = new UInt16N4((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), default);
                }

                for (var i = 0; i < primaryVertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + uvResource.Offset + i * 4, SeekOrigin.Begin);
                    texCoordsBuffer[i] = new UInt16N2((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue));
                }

                //not correct?
                //if (vertexResource3 != null)
                //{
                //    for (var i = 0; i < secondaryVertexCount; i++)
                //    {
                //        reader.Seek(section.BaseAddress + vertexResource3.Offset + i * 16 + 12, SeekOrigin.Begin);
                //        texCoordsBuffer[primaryVertexCount + i] = new UInt16N2((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue));
                //    }
                //}

                if (normalsResource != null)
                {
                    var normalBuffer = new VectorBuffer<HenDN3>(section.VertexCount);
                    mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

                    for (var i = 0; i < primaryVertexCount; i++)
                    {
                        reader.Seek(section.BaseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                        normalBuffer[i] = new HenDN3(reader.ReadUInt32());
                    }

                    //not correct?
                    //if (vertexResource3 != null)
                    //{
                    //    for (var i = 0; i < secondaryVertexCount; i++)
                    //    {
                    //        reader.Seek(section.BaseAddress + vertexResource3.Offset + i * 16, SeekOrigin.Begin);
                    //        normalBuffer[primaryVertexCount + i] = new HenDN3(reader.ReadUInt32());
                    //    }
                    //}
                }

                if (vertexResource2 != null)
                {
                    var blockSize = vertexResource2.Size / secondaryVertexCount;
                    for (var i = 0; i < secondaryVertexCount; i++)
                    {
                        reader.Seek(section.BaseAddress + vertexResource2.Offset + i * blockSize, SeekOrigin.Begin);
                        positionBuffer[primaryVertexCount + i] = new UInt16N4((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), default);
                    }
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
                    else if (section.NodesPerVertex == 1)
                        mesh.BoneIndex = nodeMap.Length > 0 ? nodeMap[0] : (byte)0;
                    else
                        throw new NotSupportedException();

                    return;
                }

                var blendIndexBuffer = new VectorBuffer<UByte4>(section.VertexCount);
                var blendWeightBuffer = new VectorBuffer<RealVector4>(section.VertexCount);

                mesh.VertexBuffer.BlendIndexChannels.Add(blendIndexBuffer);
                mesh.VertexBuffer.BlendWeightChannels.Add(blendWeightBuffer);

                ReadBlendData(vertexResource, 0, primaryVertexCount);
                if (vertexResource2 != null)
                    ReadBlendData(vertexResource2, primaryVertexCount, secondaryVertexCount);

                void ReadBlendData(ResourceInfoBlock rsrc, int vertexOffset, int vertexCount)
                {
                    var blockSize = rsrc.Size / vertexCount;
                    for (var i = 0; i < vertexCount; i++)
                    {
                        UByte4 blendIndices = default;
                        RealVector4 blendWeights = default;

                        reader.Seek(section.BaseAddress + rsrc.Offset + i * blockSize + 6, SeekOrigin.Begin);

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

                        blendIndexBuffer[vertexOffset + i] = blendIndices;
                        blendWeightBuffer[vertexOffset + i] = blendWeights;
                    }
                }
            }

            void ReadXboxRenderModelMeshData()
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
                    else if (section.NodesPerVertex == 1)
                        mesh.BoneIndex = nodeMap.Length > 0 ? nodeMap[0] : (byte)0;
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

            void ReadPcRenderModelMeshData()
            {
                var positionBuffer = new VectorBuffer<RealVector3>(section.VertexCount);
                var texCoordsBuffer = new VectorBuffer<RealVector2>(section.VertexCount);
                var normalBuffer = new VectorBuffer<RealVector3>(section.VertexCount);

                mesh.VertexBuffer = new VertexBuffer();
                mesh.VertexBuffer.PositionChannels.Add(positionBuffer);
                mesh.VertexBuffer.TextureCoordinateChannels.Add(texCoordsBuffer);
                mesh.VertexBuffer.NormalChannels.Add(normalBuffer);

                var vertexSize = vertexResource.Size / section.VertexCount;

                if (vertexSize is not (12 or 16 or 20))
                    System.Diagnostics.Debugger.Break();

                for (var i = 0; i < section.VertexCount; i++)
                {
                    //blend weights and indicies can be included in this resource, depending on the geometry classification
                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                    positionBuffer[i] = reader.ReadBufferable<RealVector3>();
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    reader.Seek(section.BaseAddress + uvResource.Offset + i * 8, SeekOrigin.Begin);
                    texCoordsBuffer[i] = reader.ReadBufferable<RealVector2>();
                }

                for (var i = 0; i < section.VertexCount; i++)
                {
                    //this contains 3x the data needed for normals, maybe also contains binormal and tangent?
                    reader.Seek(section.BaseAddress + normalsResource.Offset + i * 36, SeekOrigin.Begin);
                    normalBuffer[i] = reader.ReadBufferable<RealVector3>();
                }

                var nodeMap = Array.Empty<byte>();
                if (nodeMapResource != null)
                {
                    reader.Seek(section.BaseAddress + nodeMapResource.Offset, SeekOrigin.Begin);
                    nodeMap = reader.ReadBytes(nodeMapResource.Size);
                }

                if (section.GeometryClassification == GeometryClassification.Rigid)
                {
                    if (section.NodesPerVertex == 0)
                        mesh.BoneIndex = 0;
                    else if (section.NodesPerVertex == 1)
                        mesh.BoneIndex = nodeMap.Length > 0 ? nodeMap[0] : (byte)0;
                    else
                        throw new NotSupportedException();

                    return;
                }

                var blendIndexBuffer = new VectorBuffer<UByte4>(section.VertexCount);
                var blendWeightBuffer = new VectorBuffer<UByteN4>(section.VertexCount);

                mesh.VertexBuffer.BlendIndexChannels.Add(blendIndexBuffer);
                mesh.VertexBuffer.BlendWeightChannels.Add(blendWeightBuffer);

                for (var i = 0; i < section.VertexCount; i++)
                {
                    UByte4 blendIndices = default;
                    UByteN4 blendWeights = default;

                    reader.Seek(section.BaseAddress + vertexResource.Offset + i * vertexSize + 12, SeekOrigin.Begin);

                    if (section.GeometryClassification == GeometryClassification.RigidBoned)
                    {
                        blendIndices = new UByte4(reader.ReadByte(), default, default, default);
                        blendWeights = new UByteN4(1f, default, default, default);
                        reader.ReadByte();
                    }
                    else if (section.GeometryClassification == GeometryClassification.Skinned)
                    {
                        //if (section.NodesPerVertex == 2 || section.NodesPerVertex == 4)
                        //    reader.ReadInt16();

                        //var nodes = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() : byte.MinValue).ToList();
                        //var weights = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() / (float)byte.MaxValue : 0).ToList();

                        //if (section.NodesPerVertex == 1 && weights.Sum() == 0)
                        //    weights[0] = 1;

                        //blendIndices = new UByte4(nodes[0], nodes[1], nodes[2], nodes[3]);
                        //blendWeights = new RealVector4(weights[0], weights[1], weights[2], weights[3]);

                        blendIndices = reader.ReadBufferable<UByte4>();
                        blendWeights = reader.ReadBufferable<UByteN4>();
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
