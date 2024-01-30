using Adjutant.Geometry;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Utilities;

namespace Reclaimer.Blam.Utilities
{
    [Obsolete("TODO: migrate model bitmap exports to IContentProvider<Scene>")]
    public interface IRenderGeometry : IContentProvider<Scene>
    {
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
        IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes);
    }
}
