using Adjutant.Geometry;
using Adjutant.Spatial;
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

        public List<IGeometryMarkerGroup> MarkerGroups { get; }
        public List<IGeometryRegion> Regions { get; }

        public MaskedGeometryModel(IGeometryModel source, IEnumerable<IGeometryPermutation> permutations)
        {
            this.source = source;
            permutations = (permutations as IList<IGeometryPermutation>) ?? permutations.ToList();

            var regions = new List<GeometryRegion>();

            foreach (var reg in source.Regions)
            {
                var perms = reg.Permutations
                    .Where(p => permutations.Contains(p))
                    .ToList();

                if (!perms.Any())
                    continue;

                regions.Add(new GeometryRegion
                {
                    Name = reg.Name,
                    Permutations = perms
                });
            }

            MarkerGroups = new List<IGeometryMarkerGroup>();
            Regions = regions.OfType<IGeometryRegion>().ToList();
        }

        #region IGeometryModel

        Matrix4x4 IGeometryModel.CoordinateSystem => source.CoordinateSystem;

        string IGeometryModel.Name => source.Name;

        IReadOnlyList<IGeometryNode> IGeometryModel.Nodes => source.Nodes;

        IReadOnlyList<IGeometryMarkerGroup> IGeometryModel.MarkerGroups => MarkerGroups;

        IReadOnlyList<IGeometryRegion> IGeometryModel.Regions => Regions;

        IReadOnlyList<IGeometryMaterial> IGeometryModel.Materials => source.Materials;

        IReadOnlyList<IRealBounds5D> IGeometryModel.Bounds => source.Bounds;

        IReadOnlyList<IGeometryMesh> IGeometryModel.Meshes => source.Meshes;

        #endregion

        public void Dispose()
        {

        }
    }
}
