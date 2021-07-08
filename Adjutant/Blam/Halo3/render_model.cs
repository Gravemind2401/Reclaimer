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

namespace Adjutant.Blam.Halo3
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

        [Offset(224)]
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

            var model = new GeometryModel(Name) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo3Common.GetMaterials(Shaders));

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
                mapNodeFunc = (si, i) => NodeMaps[si].Indices[i];

            model.Meshes.AddRange(Halo3Common.GetMeshes(cache, ResourcePointer, Sections, (s, m) => m.BoundsIndex = 0, mapNodeFunc));

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

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo3Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo3Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [Flags]
    public enum ModelFlags : int
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

        public override string ToString() => Position.ToString();

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

        public override string ToString() => ShaderReference.Tag?.FullPath;
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
        public ushort IndexStart { get; set; }

        [Offset(6)]
        public ushort IndexLength { get; set; }

        [Offset(8)]
        public ushort SubsetIndex { get; set; }

        [Offset(10)]
        public ushort SubsetCount { get; set; }

        [Offset(14)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(8)]
    public class SubsetBlock
    {
        [Offset(0)]
        public ushort IndexStart { get; set; }

        [Offset(2)]
        public ushort IndexLength { get; set; }

        [Offset(4)]
        public ushort SubmeshIndex { get; set; }

        [Offset(6)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(56)]
    public class BoundingBoxBlock : BspBoundingBoxBlock
    {

    }

    [FixedSize(12)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }
}
