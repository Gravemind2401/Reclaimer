using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
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

        [Offset(312, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(104, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(336, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(128, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(396, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(188, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        [Offset(444, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(248, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer { get; set; }

        public override string ToString() => Name;

        #region IRenderGeometry

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel(Name) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo4Common.GetMaterials(Shaders));

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Where(p => p.SectionIndex >= 0).Select(p =>
                    new GeometryPermutation
                    {
                        Name = p.Name,
                        MeshIndex = p.SectionIndex,
                        MeshCount = p.SectionCount
                    }));

                if (gRegion.Permutations.Any())
                    model.Regions.Add(gRegion);
            }

            model.Meshes.AddRange(Halo4Common.GetMeshes(cache, ResourcePointer, Sections, s => 0));

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
            for (int i = 0; i < GeometryInstances.Count; i++)
            {
                var subset = section.Subsets[i];
                var mesh = new GeometryMesh
                {
                    IndexFormat = sourceMesh.IndexFormat,
                    VertexWeights = VertexWeights.Rigid,
                    NodeIndex = (byte)GeometryInstances[i].NodeIndex,
                    BoundsIndex = 0
                };

                var strip = sourceMesh.Indicies.Skip(subset.IndexStart).Take(subset.IndexLength);

                var min = strip.Min();
                var max = strip.Max();
                var len = max - min + 1;

                mesh.Indicies = strip.Select(j => j - min).ToArray();
                mesh.Vertices = sourceMesh.Vertices.Skip(min).Take(len).ToArray();

                var submesh = section.Submeshes[subset.SubmeshIndex];
                mesh.Submeshes.Add(new GeometrySubmesh
                {
                    MaterialIndex = submesh.ShaderIndex,
                    IndexStart = 0,
                    IndexLength = mesh.Indicies.Length
                });

                model.Meshes.Add(mesh);
            }
        }

        public IEnumerable<IBitmap> GetAllBitmaps()
        {
            var complete = new List<int>();

            foreach (var s in Shaders)
            {
                var rmsh = s.MaterialReference.Tag?.ReadMetadata<material>();
                if (rmsh == null) continue;

                foreach (var map in rmsh.ShaderProperties.SelectMany(p => p.ShaderMaps))
                {
                    if (map.BitmapReference.Tag == null || complete.Contains(map.BitmapReference.TagId))
                        continue;

                    complete.Add(map.BitmapReference.TagId);
                    yield return map.BitmapReference.Tag.ReadMetadata<bitmap>();
                }
            }
        }

        #endregion
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

    [FixedSize(28)]
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

    [FixedSize(48)]
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
        public TagReference MaterialReference { get; set; }

        public override string ToString() => MaterialReference.Tag?.FullPath;
    }

    [FixedSize(112)]
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

        [Offset(42)]
        public short IndexBufferIndex { get; set; }

        [Offset(47)]
        public byte TransparentNodesPerVertex { get; set; }

        [Offset(48)]
        public byte NodeIndex { get; set; }

        [Offset(49)]
        public byte VertexFormat { get; set; }

        [Offset(50)]
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
        [StoreType(typeof(ushort))]
        public int SubsetIndex { get; set; }

        [Offset(14)]
        [StoreType(typeof(ushort))]
        public int SubsetCount { get; set; }

        [Offset(20)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }
    }

    [FixedSize(16)]
    public class SubsetBlock
    {
        [Offset(0)]
        public int IndexStart { get; set; }

        [Offset(4)]
        public int IndexLength { get; set; }

        [Offset(8)]
        [StoreType(typeof(ushort))]
        public int SubmeshIndex { get; set; }

        [Offset(10)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }
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
