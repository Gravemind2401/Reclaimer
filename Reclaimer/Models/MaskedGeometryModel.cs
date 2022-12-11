using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Models
{
    public class MaskedGeometryModel : IGeometryModel
    {
        private readonly IGeometryModel source;

        private readonly List<MaskedRegion> regions;
        private readonly List<GeometryMarkerGroup> markerGroups;

        public MaskedGeometryModel(IGeometryModel source, IEnumerable<IGeometryPermutation> filter)
        {
            this.source = source;
            filter = (filter as IList<IGeometryPermutation>) ?? filter.ToList();

            regions = source.Regions.Select((r, i) => new MaskedRegion(i, r)).ToList();
            foreach (var region in regions)
                region.Permutations.RemoveAll(p => !filter.Contains(p.SourcePermutation));
            regions.RemoveAll(r => r.Permutations.Count == 0);

            markerGroups = new List<GeometryMarkerGroup>();
            foreach (var group in source.MarkerGroups)
            {
                var markers = group.Markers
                    .Select(m => new MaskedMarker(m, regions))
                    .Where(m => m.IsValid)
                    .OfType<IGeometryMarker>()
                    .ToList();

                if (markers.Count == 0)
                    continue;

                markerGroups.Add(new GeometryMarkerGroup
                {
                    Name = group.Name,
                    Markers = markers
                });
            }
        }

        #region IGeometryModel

        Matrix4x4 IGeometryModel.CoordinateSystem => source.CoordinateSystem;

        string IGeometryModel.Name => source.Name;

        IReadOnlyList<IGeometryNode> IGeometryModel.Nodes => source.Nodes;

        IReadOnlyList<IGeometryMarkerGroup> IGeometryModel.MarkerGroups => markerGroups;

        IReadOnlyList<IGeometryRegion> IGeometryModel.Regions => regions;

        IReadOnlyList<IGeometryMaterial> IGeometryModel.Materials => source.Materials;

        IReadOnlyList<IRealBounds5D> IGeometryModel.Bounds => source.Bounds;

        IReadOnlyList<IGeometryMesh> IGeometryModel.Meshes => source.Meshes;

        public void Dispose()
        {

        }

        #endregion

        private class MaskedRegion : IGeometryRegion
        {
            public int SourceIndex { get; }
            public IGeometryRegion SourceRegion { get; }
            public List<MaskedPermutation> Permutations { get; }

            public MaskedRegion(int sourceIndex, IGeometryRegion sourceRegion)
            {
                SourceIndex = sourceIndex;
                SourceRegion = sourceRegion;
                Permutations = sourceRegion.Permutations.Select((p, i) => new MaskedPermutation(i, p)).ToList();
            }

            string IGeometryRegion.Name => SourceRegion.Name;
            IReadOnlyList<IGeometryPermutation> IGeometryRegion.Permutations => Permutations;
        }

        private class MaskedPermutation : IGeometryPermutation
        {
            public int SourceIndex { get; }
            public IGeometryPermutation SourcePermutation { get; }

            public MaskedPermutation(int sourceIndex, IGeometryPermutation sourcePermutation)
            {
                SourceIndex = sourceIndex;
                SourcePermutation = sourcePermutation;
            }

            string IGeometryPermutation.Name => SourcePermutation.Name;
            int IGeometryPermutation.MeshIndex => SourcePermutation.MeshIndex;
            int IGeometryPermutation.MeshCount => SourcePermutation.MeshCount;
            float IGeometryPermutation.TransformScale => SourcePermutation.TransformScale;
            Matrix4x4 IGeometryPermutation.Transform => SourcePermutation.Transform;
        }

        private class MaskedMarker : IGeometryMarker
        {
            private readonly IGeometryMarker sourceMarker;
            private readonly byte regionIndex;
            private readonly byte permutationIndex;

            public MaskedMarker(IGeometryMarker sourceMarker, List<MaskedRegion> regions)
            {
                this.sourceMarker = sourceMarker;

                if (sourceMarker.RegionIndex == byte.MaxValue)
                {
                    regionIndex = permutationIndex = byte.MaxValue;
                    return;
                }

                var index = regions.FindIndex(r => r.SourceIndex == sourceMarker.RegionIndex);
                regionIndex = index < 0 ? byte.MaxValue : (byte)index;

                var region = regions.ElementAtOrDefault(regionIndex);
                index = region?.Permutations.FindIndex(p => p.SourceIndex == sourceMarker.PermutationIndex) ?? -1;
                permutationIndex = index < 0 ? byte.MaxValue : (byte)index;
            }

            public bool IsValid => sourceMarker.RegionIndex == byte.MaxValue || regionIndex != byte.MaxValue;

            byte IGeometryMarker.RegionIndex => regionIndex;
            byte IGeometryMarker.PermutationIndex => permutationIndex;
            byte IGeometryMarker.NodeIndex => sourceMarker.NodeIndex;
            IVector3 IGeometryMarker.Position => sourceMarker.Position;
            IVector4 IGeometryMarker.Rotation => sourceMarker.Rotation;
        }
    }
}
