using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo2Beta
{
    //note H2B ascii string fields are actually 32 bytes, but the last 4 are not part of the string
    public class render_model : IRenderGeometry
    {
        private readonly IIndexItem item;

        public render_model(IIndexItem item)
        {
            this.item = item;
        }

        [Offset(52)]
        public BlockCollection<Halo2.BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(64)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(76)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(124)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(148)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(160)]
        public BlockCollection<Halo2.ShaderBlock> Shaders { get; set; }

        #region IRenderGeometry

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 6;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel(item.FileName()) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo2.Halo2Common.GetMaterials(Shaders));

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
                var baseAddress = section.HeaderSize + 8;

                using (var ms = new MemoryStream(data))
                using (var reader = new EndianReader(ms, ByteOrder.LittleEndian))
                {
                    reader.ReadInt32(); //hklb
                    var sectionInfo = reader.ReadObject<MeshResourceDetailsBlock>();

                    var submeshResource = section.Resources[0];
                    var indexResource = section.Resources.FirstOrDefault(r => r.Type0 == 48);
                    var vertexResource = section.Resources.FirstOrDefault(r => r.Type0 == 92 && r.Type1 == 0);
                    var uvResource = section.Resources.FirstOrDefault(r => r.Type0 == 92 && r.Type1 == 1);
                    var normalsResource = section.Resources.FirstOrDefault(r => r.Type0 == 92 && r.Type1 == 2);
                    var nodeMapResource = section.Resources.FirstOrDefault(r => r.Type0 == 164);

                    reader.Seek(baseAddress + submeshResource.Offset, SeekOrigin.Begin);
                    var submeshes = reader.ReadEnumerable<Halo2.SubmeshDataBlock>(submeshResource.Size / 72).ToList();

                    var mesh = new GeometryMesh { BoundsIndex = 0, };

                    foreach (var submesh in submeshes)
                    {
                        mesh.Submeshes.Add(new GeometrySubmesh
                        {
                            MaterialIndex = submesh.ShaderIndex,
                            IndexStart = submesh.IndexStart,
                            IndexLength = submesh.IndexLength
                        });
                    }

                    if (section.FaceCount * 3 == sectionInfo.IndexCount)
                        mesh.IndexFormat = IndexFormat.TriangleList;
                    else
                        mesh.IndexFormat = IndexFormat.TriangleStrip;

                    reader.Seek(baseAddress + indexResource.Offset, SeekOrigin.Begin);
                    mesh.Indicies = reader.ReadEnumerable<ushort>(sectionInfo.IndexCount).Select(i => (int)i).ToArray();

                    var nodeMap = new byte[0];
                    if (nodeMapResource != null)
                    {
                        reader.Seek(baseAddress + nodeMapResource.Offset, SeekOrigin.Begin);
                        nodeMap = reader.ReadBytes(sectionInfo.NodeMapCount);
                    }

                    #region Vertices
                    mesh.Vertices = new IVertex[section.VertexCount];
                    var vertexSize = vertexResource.Size / section.VertexCount;
                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = new Halo2.Vertex();

                        reader.Seek(baseAddress + vertexResource.Offset + i * vertexSize, SeekOrigin.Begin);
                        vert.Position = new UInt16N4((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue), 0);
                        ReadBlendData(reader, section, mesh, vert, nodeMap);

                        mesh.Vertices[i] = vert;
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (Halo2.Vertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + uvResource.Offset + i * 4, SeekOrigin.Begin);
                        vert.TexCoords = new UInt16N2((ushort)(reader.ReadInt16() - short.MinValue), (ushort)(reader.ReadInt16() - short.MinValue));
                    }

                    for (int i = 0; i < section.VertexCount; i++)
                    {
                        var vert = (Halo2.Vertex)mesh.Vertices[i];

                        reader.Seek(baseAddress + normalsResource.Offset + i * 12, SeekOrigin.Begin);
                        vert.Normal = new HenDN3(reader.ReadUInt32());
                    }
                    #endregion

                    model.Meshes.Add(mesh);
                }
            }

            return model;
        }

        private void ReadBlendData(EndianReader reader, SectionBlock section, GeometryMesh mesh, Halo2.Vertex vert, byte[] nodeMap)
        {
            if (section.GeometryClassification == Halo2.GeometryClassification.Rigid)
            {
                mesh.VertexWeights = VertexWeights.Rigid;
                if (section.NodesPerVertex == 0)
                    mesh.NodeIndex = 0;
                else if (section.NodesPerVertex == 1 && nodeMap.Length > 0)
                    mesh.NodeIndex = nodeMap[0];
                else
                    throw new NotSupportedException();

                return;
            }
            else if (section.GeometryClassification == Halo2.GeometryClassification.RigidBoned)
            {
                mesh.VertexWeights = VertexWeights.Skinned;
                vert.BlendIndices = new RealVector4D(reader.ReadByte(), 0, 0, 0);
                vert.BlendWeight = new RealVector4D(1, 0, 0, 0);
                reader.ReadByte();
            }
            else if (section.GeometryClassification == Halo2.GeometryClassification.Skinned)
            {
                mesh.VertexWeights = VertexWeights.Skinned;
                if (section.NodesPerVertex == 2 || section.NodesPerVertex == 4)
                    reader.ReadInt16();

                var nodes = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() : 0).ToList();
                var weights = Enumerable.Range(0, 4).Select(i => section.NodesPerVertex > i ? reader.ReadByte() / (float)byte.MaxValue : 0).ToList();

                if (section.NodesPerVertex == 1 && weights.Sum() == 0)
                    weights[0] = 1;

                vert.BlendIndices = new RealVector4D(nodes[0], nodes[1], nodes[2], nodes[3]);
                vert.BlendWeight = new RealVector4D(weights[0], weights[1], weights[2], weights[3]);
            }

            if (nodeMap.Length > 0)
            {
                var temp = vert.BlendIndices;
                vert.BlendIndices = new RealVector4D
                {
                    X = section.NodesPerVertex > 0 ? nodeMap[(int)temp.X] : 0,
                    Y = section.NodesPerVertex > 1 ? nodeMap[(int)temp.Y] : 0,
                    Z = section.NodesPerVertex > 2 ? nodeMap[(int)temp.Z] : 0,
                    W = section.NodesPerVertex > 3 ? nodeMap[(int)temp.W] : 0,
                };
            }
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo2.Halo2Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo2.Halo2Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    public struct MeshResourceDetailsBlock
    {
        [Offset(52)]
        public ushort IndexCount { get; set; }

        [Offset(168)]
        public ushort NodeMapCount { get; set; }
    }

    [FixedSize(48)]
    public class RegionBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(36)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(44)]
    public class PermutationBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public short PotatoSectionIndex { get; set; }

        [Offset(34)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(36)]
        public short LowSectionIndex { get; set; }

        [Offset(38)]
        public short MediumSectionIndex { get; set; }

        [Offset(40)]
        public short HighSectionIndex { get; set; }

        [Offset(42)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex, PotatoSectionIndex };

        public override string ToString() => Name;
    }

    [FixedSize(104)]
    public class SectionBlock
    {
        [Offset(0)]
        public Halo2.GeometryClassification GeometryClassification { get; set; }

        [Offset(4)]
        public ushort VertexCount { get; set; }

        [Offset(6)]
        public ushort FaceCount { get; set; }

        [Offset(20)]
        public byte NodesPerVertex { get; set; }

        [Offset(64)]
        public Halo2.DataPointer DataPointer { get; set; }

        [Offset(68)]
        public int DataSize { get; set; }

        [Offset(72)]
        public int HeaderSize { get; set; }

        [Offset(76)]
        public int BodySize { get; set; }

        [Offset(80)]
        public BlockCollection<Halo2.ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(124)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public short ParentIndex { get; set; }

        [Offset(34)]
        public short FirstChildIndex { get; set; }

        [Offset(36)]
        public short NextSiblingIndex { get; set; }

        [Offset(38)]
        public short SomethingIndex { get; set; }

        [Offset(40)]
        public RealVector3D Position { get; set; }

        [Offset(52)]
        public RealVector4D Rotation { get; set; }

        [Offset(68)]
        public float TransformScale { get; set; }

        [Offset(72)]
        public Matrix4x4 Transform { get; set; }

        [Offset(120)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation;

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

        #endregion
    }

    [FixedSize(44)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public BlockCollection<Halo2.MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }
}
