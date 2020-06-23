using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Halo5
{
    public class render_model : IRenderGeometry
    {
        private readonly Module module;
        private readonly ModuleItem item;

        public MetadataHeader Header { get; }

        public render_model(Module module, ModuleItem item, MetadataHeader header)
        {
            this.module = module;
            this.item = item;

            Header = header;
        }

        [Offset(32)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(60)]
        public int InstancedGeometrySectionIndex { get; set; }

        [Offset(64)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(96)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(152)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(180)]
        public BlockCollection<MaterialBlock> Materials { get; set; }

        [Offset(272)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(328)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(356)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        public override string ToString() => Utils.GetFileName(item.FullPath);

        #region IRenderGeometry

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => Sections.Max(s => s.SectionLods.Count);

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel(Utils.GetFileName(item.FullPath)) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);

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

            CreateInstanceMeshes(model, lod);

            return model;
        }

        private void CreateInstanceMeshes(GeometryModel model, int lod)
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
            var localLod = Math.Min(lod, section.SectionLods.Count - 1);
            for (int i = 0; i < GeometryInstances.Count; i++)
            {
                var subset = section.SectionLods[localLod].Subsets[i];
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

                var submesh = section.SectionLods[localLod].Submeshes[subset.SubmeshIndex];
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
            yield break;
        }

        #endregion
    }

    [FixedSize(80)]
    public struct VertexBufferInfo
    {
        [Offset(4)]
        public int VertexCount { get; set; }
    }

    [FixedSize(72)]
    public struct IndexBufferInfo
    {
        [Offset(4)]
        public int IndexCount { get; set; }
    }

    [FixedSize(32)]
    public class RegionBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(28)]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

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
        public StringHash Name { get; set; }

        [Offset(4)]
        public int NodeIndex { get; set; }

        [Offset(8)]
        public float TransformScale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(124)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        public StringHash Name { get; set; }

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
        public Matrix4x4 Transform { get; set; }

        [Offset(88)]
        public float TransformScale { get; set; }

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

    [FixedSize(32)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(56)]
    public class MarkerBlock : IGeometryMarker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(4)]
        public int PermutationIndex { get; set; }

        [Offset(8)]
        public byte NodeIndex { get; set; }

        [Offset(12)]
        public RealVector3D Position { get; set; }

        [Offset(24)]
        public RealVector4D Rotation { get; set; }

        [Offset(40)]
        public float Scale { get; set; }

        [Offset(44)]
        public RealVector3D Direction { get; set; }

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        byte IGeometryMarker.PermutationIndex => (byte)PermutationIndex;

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(32)]
    public class MaterialBlock
    {
        [Offset(0)]
        public TagReference MaterialReference { get; set; }

        public override string ToString() => MaterialReference.Tag?.FullPath;
    }

    [FixedSize(128)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SectionLodBlock> SectionLods { get; set; }

        [Offset(30)]
        public byte NodeIndex { get; set; }

        [Offset(31)]
        public byte VertexFormat { get; set; }

        [Offset(33)]
        [StoreType(typeof(byte))]
        public IndexFormat IndexFormat { get; set; }
    }

    [FixedSize(140)]
    public class SectionLodBlock
    {
        [Offset(56)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }

        [Offset(84)]
        public BlockCollection<SubsetBlock> Subsets { get; set; }

        [Offset(112)]
        public short VertexBufferIndex { get; set; }

        [Offset(134)]
        public short IndexBufferIndex { get; set; }
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
        //short flags, short padding

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

    [FixedSize(28)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }
}
