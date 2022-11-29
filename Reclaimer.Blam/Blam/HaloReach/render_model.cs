using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.HaloReach
{
    public class render_model : IRenderGeometry
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public render_model(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
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

        [Offset(236, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(248, MinVersion = (int)CacheType.HaloReachRetail)]
        public ResourceIdentifier ResourcePointer { get; set; }

        public override string ToString() => Name;

        #region IRenderGeometry

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            if (Sections.All(s => s.IndexBufferIndex < 0))
                throw Exceptions.GeometryHasNoEdges();

            var model = new GeometryModel(Name) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(HaloReachCommon.GetMaterials(Shaders));

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { SourceIndex = Regions.IndexOf(region), Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Where(p => p.SectionIndex >= 0).Select(p =>
                    new GeometryPermutation
                    {
                        SourceIndex = region.Permutations.IndexOf(p),
                        Name = p.Name,
                        MeshIndex = p.SectionIndex,
                        MeshCount = 1
                    }));

                if (gRegion.Permutations.Any())
                    model.Regions.Add(gRegion);
            }

            Func<int, int, int> mapNodeFunc = null;
            if (Flags.HasFlag(ModelFlags.UseLocalNodes))
                mapNodeFunc = (si, i) => NodeMaps.ElementAtOrDefault(si)?.Indices.Cast<byte?>().ElementAtOrDefault(i) ?? i;

            model.Meshes.AddRange(HaloReachCommon.GetMeshes(cache, ResourcePointer, Sections, (s, m) => m.BoundsIndex = 0, mapNodeFunc));

            CreateInstanceMeshes(model);

            return model;
        }

        private void CreateInstanceMeshes(GeometryModel model)
        {
            if (InstancedGeometrySectionIndex < 0)
                return;

            /* 
             * The render_model geometry instances have all their mesh data
             * in the same section and each instance has its own subset.
             * This function separates the subsets into separate sections
             * to make things easier for the model rendering and exporting 
             */

            var gRegion = new GeometryRegion { Name = "Instances" };
            gRegion.Permutations.AddRange(GeometryInstances.Select(i =>
                new GeometryPermutation
                {
                    SourceIndex = GeometryInstances.IndexOf(i),
                    Name = i.Name,
                    Transform = i.Transform,
                    TransformScale = i.TransformScale,
                    MeshIndex = InstancedGeometrySectionIndex + GeometryInstances.IndexOf(i),
                    MeshCount = 1
                }));

            model.Regions.Add(gRegion);

            var sourceMesh = model.Meshes[InstancedGeometrySectionIndex];
            model.Meshes.Remove(sourceMesh);

            var section = Sections[InstancedGeometrySectionIndex];
            for (var i = 0; i < GeometryInstances.Count; i++)
            {
                var subset = section.Subsets[i];
                var mesh = new GeometryMesh
                {
                    IndexFormat = sourceMesh.IndexFormat,
                    VertexWeights = VertexWeights.Rigid,
                    NodeIndex = (byte)GeometryInstances[i].NodeIndex,
                    BoundsIndex = 0
                };

                var strip = sourceMesh.IndexBuffer.GetSubset(subset.IndexStart, subset.IndexLength);

                var min = strip.Min();
                var max = strip.Max();
                var len = max - min + 1;

                mesh.IndexBuffer = IndexBuffer.FromCollection(strip.Select(j => j - min));
                mesh.VertexBuffer = sourceMesh.VertexBuffer.GetSubset(min, len);

                var submesh = section.Submeshes[subset.SubmeshIndex];
                mesh.Submeshes.Add(new GeometrySubmesh
                {
                    MaterialIndex = submesh.ShaderIndex,
                    IndexStart = 0,
                    IndexLength = mesh.IndexBuffer.Count
                });

                model.Meshes.Add(mesh);
            }
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => HaloReachCommon.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => HaloReachCommon.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [Flags]
    public enum ModelFlags : short
    {
        UseLocalNodes = 4
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

    [FixedSize(24)]
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

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

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

    [FixedSize(48)]
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

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(44)]
    public class ShaderBlock
    {
        [Offset(0)]
        public TagReference ShaderReference { get; set; }

        public override string ToString() => ShaderReference.Tag?.FullPath;
    }

    [FixedSize(92)]
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

        [Offset(45)]
        public byte TransparentNodesPerVertex { get; set; }

        [Offset(46)]
        public byte NodeIndex { get; set; }

        [Offset(47)]
        public byte VertexFormat { get; set; }

        [Offset(48)]
        public byte OpaqueNodesPerVertex { get; set; }
    }

    [FixedSize(24)]
    public class SubmeshBlock
    {
        [Offset(0)]
        public short ShaderIndex { get; set; }

        [Offset(4)]
        public int IndexStart { get; set; }

        [Offset(8)]
        public int IndexLength { get; set; }

        [Offset(12)]
        public ushort SubsetIndex { get; set; }

        [Offset(14)]
        public ushort SubsetCount { get; set; }

        [Offset(20)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(16)]
    public class SubsetBlock
    {
        [Offset(0)]
        public int IndexStart { get; set; }

        [Offset(4)]
        public int IndexLength { get; set; }

        [Offset(8)]
        public ushort SubmeshIndex { get; set; }

        [Offset(10)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(52)]
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
