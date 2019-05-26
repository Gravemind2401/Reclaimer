using Adjutant.Blam.Definitions;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Halo3
{
    public class render_model : IRenderGeometry
    {
        private readonly CacheFile cache;

        public render_model(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public ModelFlags Flags { get; set; }

        [Offset(12)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(28)]
        public int InstancedGeometrySectionIndex { get; set; }

        [Offset(32)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(48)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(60)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(72)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(104)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(116)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(176)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        [Offset(224)]
        public ResourceIdentifier ResourcePointer { get; set; }

        public override string ToString() => Name;

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);

            #region Shaders
            var shadersMeta = Shaders.Select(s => s.ShaderReference.Tag.ReadMetadata<shader>()).ToList();
            foreach (var shader in shadersMeta)
            {
                var template = shader.ShaderProperties[0].TemplateReference.Tag.ReadMetadata<render_method_template>();
                var stringId = template.Usages.FirstOrDefault(s => s.Value == "base_map");
                var diffuseIndex = stringId.Value == null ? 0 : template.Usages.IndexOf(stringId);

                var map = shader.ShaderProperties[0].ShaderMaps[diffuseIndex];
                var bitmTag = map.BitmapReference.Tag;
                if (bitmTag == null)
                {
                    model.Materials.Add(null);
                    continue;
                }

                var tile = map.TilingIndex == byte.MaxValue ? (RealVector4D?)null : shader.ShaderProperties[0].TilingData[map.TilingIndex];
                var mat = new GeometryMaterial
                {
                    Name = bitmTag.FileName,
                    Diffuse = bitmTag.ReadMetadata<bitmap>(),
                    Tiling = new RealVector2D(tile?.X ?? 1, tile?.Y ?? 1)
                };

                model.Materials.Add(mat);
            }
            #endregion

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Where(p => p.SectionIndex >= 0).Select(p =>
                    new GeometryPermutation
                    {
                        Name = p.Name,
                        NodeIndex = byte.MaxValue,
                        Transform = Matrix4x4.Identity,
                        TransformScale = 1,
                        BoundsIndex = 0,
                        MeshIndex = p.SectionIndex
                    }));

                if (gRegion.Permutations.Any())
                    model.Regions.Add(gRegion);
            }

            VertexBufferInfo[] vertexBufferInfo;
            IndexBufferInfo[] indexBufferInfo;

            var entry = cache.ResourceGestalt.ResourceEntries[ResourcePointer.ResourceIndex];
            using (var cacheReader = cache.CreateReader(cache.MetadataTranslator))
            using (var reader = cacheReader.CreateVirtualReader(cache.ResourceGestalt.FixupDataPointer.Address))
            {
                reader.Seek(entry.FixupOffset + (entry.FixupSize - 24), SeekOrigin.Begin);
                var vertexBufferCount = reader.ReadInt32();
                reader.Seek(8, SeekOrigin.Current);
                var indexBufferCount = reader.ReadInt32();

                reader.Seek(entry.FixupOffset, SeekOrigin.Begin);
                vertexBufferInfo = reader.ReadEnumerable<VertexBufferInfo>(vertexBufferCount).ToArray();
                reader.Seek(12 * vertexBufferCount, SeekOrigin.Current); //12 byte struct here for each vertex buffer
                indexBufferInfo = reader.ReadEnumerable<IndexBufferInfo>(indexBufferCount).ToArray();
                //12 byte struct here for each index buffer
                //4x 12 byte structs here
            }

            var meshes = new GeometryMesh[Sections.Count];

            using (var ms = new MemoryStream(ResourcePointer.ReadData()))
            using (var reader = new EndianReader(ms, ByteOrder.BigEndian))
            {
                var doc = new XmlDocument();
                doc.LoadXml(Adjutant.Properties.Resources.Halo3VertexBuffer);

                var lookup = doc.FirstChild.ChildNodes.Cast<XmlNode>()
                    .ToDictionary(n => Convert.ToInt32(n.Attributes["type"].Value, 16));

                foreach (var section in Sections)
                {
                    var sectionIndex = Sections.IndexOf(section);
                    foreach (var submesh in section.Submeshes)
                    {
                        var gSubmesh = new GeometrySubmesh
                        {
                            MaterialIndex = submesh.ShaderIndex,
                            IndexStart = submesh.IndexStart,
                            IndexLength = submesh.IndexLength
                        };

                        var permutations = model.Regions
                            .SelectMany(r => r.Permutations)
                            .Where(p => p.MeshIndex == sectionIndex);

                        foreach (var p in permutations)
                            ((List<IGeometrySubmesh>)p.Submeshes).Add(gSubmesh);
                    }

                    var node = lookup[section.VertexFormat];
                    var vInfo = vertexBufferInfo[section.VertexBufferIndex];
                    var iInfo = indexBufferInfo[section.IndexBufferIndex];

                    var mesh = meshes[sectionIndex] = new GeometryMesh
                    {
                        IndexFormat = iInfo.IndexFormat,
                        Vertices = new IVertex[vInfo.VertexCount]
                    };

                    var address = entry.ResourceFixups[section.VertexBufferIndex].Offset & 0x0FFFFFFF;
                    reader.Seek(address, SeekOrigin.Begin);
                    for (int i = 0; i < vInfo.VertexCount; i++)
                    {
                        var vert = new XmlVertex(reader, node);
                        mesh.Vertices[i] = vert;
                    }

                    var totalIndices = section.Submeshes.Sum(s => s.IndexLength);
                    address = entry.ResourceFixups[vertexBufferInfo.Length * 2 + section.IndexBufferIndex].Offset & 0x0FFFFFFF;
                    reader.Seek(address, SeekOrigin.Begin);
                    if (totalIndices > ushort.MaxValue)
                        mesh.Indicies = reader.ReadEnumerable<int>(totalIndices).ToArray();
                    else mesh.Indicies = reader.ReadEnumerable<ushort>(totalIndices).Select(i => (int)i).ToArray();
                }
            }

            model.Meshes.AddRange(meshes);

            CreateInstanceMeshes(model);

            return model;
        }

        private void CreateInstanceMeshes(GeometryModel model)
        {
            if (InstancedGeometrySectionIndex < 0)
                return;

            var gRegion = new GeometryRegion { Name = "Instances" };
            gRegion.Permutations.AddRange(GeometryInstances.Select(i =>
                new GeometryPermutation
                {
                    Name = i.Name,
                    NodeIndex = (byte)i.NodeIndex,
                    Transform = i.Transform,
                    TransformScale = i.TransformScale,
                    BoundsIndex = 0,
                    MeshIndex = InstancedGeometrySectionIndex + GeometryInstances.IndexOf(i)
                }));

            model.Regions.Add(gRegion);

            var sourceMesh = model.Meshes[InstancedGeometrySectionIndex];
            model.Meshes.Remove(sourceMesh);

            var section = Sections[InstancedGeometrySectionIndex];
            foreach (var subset in section.Subsets)
            {
                var mesh = new GeometryMesh
                {
                     IndexFormat = sourceMesh.IndexFormat,
                     VertexWeights = VertexWeights.Rigid
                };

                var strip = sourceMesh.Indicies.Skip(subset.IndexStart).Take(subset.IndexLength);

                var min = strip.Min();
                var max = strip.Max();
                var len = max - min + 1;

                mesh.Indicies = strip.Select(i => i - min).ToArray();
                mesh.Vertices = sourceMesh.Vertices.Skip(min).Take(len).ToArray();

                model.Meshes.Add(mesh);

                var sectionIndex = InstancedGeometrySectionIndex + section.Subsets.IndexOf(subset);
                var submesh = section.Submeshes[subset.SubmeshIndex];
                var gSubmesh = new GeometrySubmesh
                {
                    MaterialIndex = submesh.ShaderIndex,
                    IndexStart = 0,
                    IndexLength = mesh.Indicies.Length
                };

                var permutations = model.Regions
                    .SelectMany(r => r.Permutations)
                    .Where(p => p.MeshIndex == sectionIndex);

                foreach (var p in permutations)
                    ((List<IGeometrySubmesh>)p.Submeshes).Add(gSubmesh);
            }
        }

        #endregion

        [FixedSize(28)]
        private struct VertexBufferInfo
        {
            [Offset(0)]
            public int VertexCount { get; set; }

            [Offset(8)]
            public int DataLength { get; set; }
        }

        [FixedSize(24)]
        private struct IndexBufferInfo
        {
            [Offset(0)]
            public IndexFormat IndexFormat { get; set; }

            [Offset(4)]
            public int DataLength { get; set; }
        }
    }

    [Flags]
    public enum ModelFlags : int
    {
        UseLocalNodes = 262144
    }

    [FixedSize(16)]
    public class RegionBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(16, MaxVersion = (int)CacheType.Halo3ODST)]
    [FixedSize(24, MinVersion = (int)CacheType.Halo3ODST)]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short SectionIndex { get; set; }

        [Offset(6)]
        public short SectionCount { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(60)]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public int NodeIndex { get; set; }

        [Offset(8)]
        public float TransformScale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }

        public override string ToString() => Name;
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
        public RealVector3D Position { get; set; }

        [Offset(24)]
        public RealVector4D Rotation { get; set; }

        [Offset(40)]
        public float TransformScale { get; set; }

        [Offset(44)]
        public Matrix4x4 Transform { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation;

        #endregion
    }

    [FixedSize(16)]
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
        public RealVector3D Position { get; set; }

        [Offset(16)]
        public RealVector4D Rotation { get; set; }

        [Offset(32)]
        public float Scale { get; set; }

        #region IGeometryMarker

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(36)]
    public class ShaderBlock
    {
        [Offset(0)]
        public TagReference ShaderReference { get; set; }
    }

    [FixedSize(76)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }

        [Offset(12)]
        public BlockCollection<SubsetBlock> Subsets { get; set; }

        [Offset(24)]
        public short VertexBufferIndex { get; set; }

        [Offset(30)]
        public short UnknownIndex { get; set; }

        [Offset(40)]
        public short IndexBufferIndex { get; set; }

        [Offset(44)]
        public byte TransparentNodesPerVertex { get; set; }

        [Offset(45)]
        public byte NodeIndex { get; set; }

        [Offset(46)]
        public byte VertexFormat { get; set; }

        [Offset(47)]
        public byte OpaqueNodesPerVertex { get; set; }
    }

    [FixedSize(16)]
    public class SubmeshBlock
    {
        [Offset(0)]
        public short ShaderIndex { get; set; }

        [Offset(4)]
        [StoreType(typeof(ushort))]
        public int IndexStart { get; set; }

        [Offset(6)]
        [StoreType(typeof(ushort))]
        public int IndexLength { get; set; }

        [Offset(8)]
        [StoreType(typeof(ushort))]
        public int SubsetIndex { get; set; }

        [Offset(10)]
        [StoreType(typeof(ushort))]
        public int SubsetCount { get; set; }

        [Offset(14)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }
    }

    [FixedSize(8)]
    public class SubsetBlock
    {
        [Offset(0)]
        [StoreType(typeof(ushort))]
        public int IndexStart { get; set; }

        [Offset(2)]
        [StoreType(typeof(ushort))]
        public int IndexLength { get; set; }

        [Offset(4)]
        [StoreType(typeof(ushort))]
        public int SubmeshIndex { get; set; }

        [Offset(6)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }
    }

    [FixedSize(56)]
    public class BoundingBoxBlock : IRealBounds5D
    {
        [Offset(4)]
        public RealBounds XBounds { get; set; }

        [Offset(12)]
        public RealBounds YBounds { get; set; }

        [Offset(20)]
        public RealBounds ZBounds { get; set; }

        [Offset(28)]
        public RealBounds UBounds { get; set; }

        [Offset(36)]
        public RealBounds VBounds { get; set; }

        #region IRealBounds5D

        IRealBounds IRealBounds5D.XBounds => XBounds;

        IRealBounds IRealBounds5D.YBounds => YBounds;

        IRealBounds IRealBounds5D.ZBounds => ZBounds;

        IRealBounds IRealBounds5D.UBounds => UBounds;

        IRealBounds IRealBounds5D.VBounds => VBounds;

        #endregion
    }

    [FixedSize(12)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }
}
