using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public class render_model : ContentTagDefinition, IRenderGeometry
    {
        public render_model(IIndexItem item)
            : base(item)
        { }

        [Offset(20)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(28)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(36)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(72)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(88)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(96)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 6;

        public IGeometryModel ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);

            var model = new GeometryModel(Item.FileName) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo2Common.GetMaterials(Shaders));

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { SourceIndex = Regions.IndexOf(region), Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Select(p =>
                    new GeometryPermutation
                    {
                        SourceIndex = region.Permutations.IndexOf(p),
                        Name = p.Name,
                        MeshIndex = p.LodArray[lod],
                        MeshCount = 1
                    }));

                model.Regions.Add(gRegion);
            }

            foreach (var section in Sections)
            {
                var data = section.DataPointer.ReadData(section.DataSize);
                var baseAddress = section.DataSize - section.HeaderSize - 4;

                using (var ms = new MemoryStream(data))
                using (var reader = new EndianReader(ms, ByteOrder.LittleEndian))
                {
                    var sectionInfo = reader.ReadObject<MeshResourceDetailsBlock>();

                    var submeshResource = section.Resources[0];
                    var indexResource = section.Resources.FirstOrDefault(r => r.Type0 == 32);
                    var vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 0);
                    var uvResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 1);
                    var normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == 56 && r.Type1 == 2);
                    var nodeMapResource = section.Resources.FirstOrDefault(r => r.Type0 == 100);

                    reader.Seek(baseAddress + submeshResource.Offset, SeekOrigin.Begin);
                    var submeshes = reader.ReadArray<SubmeshDataBlock>(submeshResource.Size / 72);

                    var mesh = new GeometryMesh { BoundsIndex = 0 };

                    mesh.Submeshes.AddRange(
                        submeshes.Select(s => new GeometrySubmesh
                        {
                            MaterialIndex = s.ShaderIndex,
                            IndexStart = s.IndexStart,
                            IndexLength = s.IndexLength
                        })
                    );

                    var indexFormat = section.FaceCount * 3 == sectionInfo.IndexCount
                        ? IndexFormat.TriangleList
                        : IndexFormat.TriangleStrip;

                    reader.Seek(baseAddress + indexResource.Offset, SeekOrigin.Begin);
                    mesh.IndexBuffer = IndexBuffer.FromArray(reader.ReadArray<ushort>(sectionInfo.IndexCount), indexFormat);

                    #region Vertices
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
                        reader.Seek(baseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                        positionBuffer[i] = new UInt16N4((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), default);
                    }

                    for (var i = 0; i < section.VertexCount; i++)
                    {
                        reader.Seek(baseAddress + uvResource.Offset + i * 4, SeekOrigin.Begin);
                        texCoordsBuffer[i] = new UInt16N2((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue));
                    }

                    for (var i = 0; i < section.VertexCount; i++)
                    {
                        reader.Seek(baseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                        normalBuffer[i] = new HenDN3(reader.ReadUInt32());
                    }

                    var nodeMap = Array.Empty<byte>();
                    if (nodeMapResource != null)
                    {
                        reader.Seek(baseAddress + nodeMapResource.Offset, SeekOrigin.Begin);
                        nodeMap = reader.ReadBytes(sectionInfo.NodeMapCount);
                    }

                    PopulateBlendData(reader, section, mesh, nodeMap, baseAddress + vertexResource.Offset, vertexSize);
                    #endregion

                    model.Meshes.Add(mesh);
                }
            }

            return model;
        }

        private static void PopulateBlendData(EndianReader reader, SectionBlock section, GeometryMesh mesh, byte[] nodeMap, int vertexOffset, int vertexSize)
        {
            if (section.GeometryClassification == GeometryClassification.Rigid)
            {
                if (section.NodesPerVertex == 0)
                    mesh.NodeIndex = 0;
                else if (section.NodesPerVertex == 1 && nodeMap.Length > 0)
                    mesh.NodeIndex = nodeMap[0];
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

                reader.Seek(vertexOffset + i * vertexSize + 6, SeekOrigin.Begin);

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

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo2Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo2Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    public struct MeshResourceDetailsBlock
    {
        [Offset(40)]
        public ushort IndexCount { get; set; }

        [Offset(108)]
        public ushort NodeMapCount { get; set; }
    }

    [FixedSize(72)]
    public struct SubmeshDataBlock
    {
        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(6)]
        public ushort IndexStart { get; set; }

        [Offset(8)]
        public ushort IndexLength { get; set; }
    }

    [FixedSize(56)]
    public class BoundingBoxBlock : IRealBounds5D
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(24)]
        public RealBounds UBounds { get; set; }

        [Offset(32)]
        public RealBounds VBounds { get; set; }
    }

    [FixedSize(16)]
    public class RegionBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(8)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(16)]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short PotatoSectionIndex { get; set; }

        [Offset(6)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(8)]
        public short LowSectionIndex { get; set; }

        [Offset(10)]
        public short MediumSectionIndex { get; set; }

        [Offset(12)]
        public short HighSectionIndex { get; set; }

        [Offset(14)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex, PotatoSectionIndex };

        public override string ToString() => Name;
    }

    public enum GeometryClassification : short
    {
        Worldspace = 0,
        Rigid = 1,
        RigidBoned = 2,
        Skinned = 3
    }

    [FixedSize(92)]
    public class SectionBlock
    {
        [Offset(0)]
        public GeometryClassification GeometryClassification { get; set; }

        [Offset(4)]
        public ushort VertexCount { get; set; }

        [Offset(6)]
        public ushort FaceCount { get; set; }

        [Offset(20)]
        public byte NodesPerVertex { get; set; }

        [Offset(56)]
        public DataPointer DataPointer { get; set; }

        [Offset(60)]
        public int DataSize { get; set; }

        [Offset(68)]
        public int HeaderSize { get; set; }

        [Offset(72)]
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(96)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short ParentIndex { get; set; }

        [Offset(6)]
        public short FirstChildIndex { get; set; }

        [Offset(8)]
        public short NextSiblingIndex { get; set; }

        [Offset(12)]
        public RealVector3 Position { get; set; }

        [Offset(24)]
        public RealVector4 Rotation { get; set; }

        [Offset(40)]
        public float TransformScale { get; set; }

        [Offset(44)]
        public Matrix4x4 Transform { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IVector3 IGeometryNode.Position => Position;

        IVector4 IGeometryNode.Rotation => Rotation;

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

        #endregion
    }

    [FixedSize(12)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(36)]
    public class MarkerBlock : IGeometryMarker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        [Offset(4)]
        public RealVector3 Position { get; set; }

        [Offset(16)]
        public RealVector4 Rotation { get; set; }

        [Offset(32)]
        public float Scale { get; set; }

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        IVector3 IGeometryMarker.Position => Position;

        IVector4 IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(52, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(32, MinVersion = (int)CacheType.Halo2Xbox)]
    public class ShaderBlock
    {
        [Offset(16, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(8, MinVersion = (int)CacheType.Halo2Xbox)]
        public TagReference ShaderReference { get; set; }

        public override string ToString() => ShaderReference.Tag?.TagName;
    }
}
