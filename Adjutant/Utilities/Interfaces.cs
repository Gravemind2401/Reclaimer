using Adjutant.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IBitmap
    {
        string Name { get; }
        string Class { get; }
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }

    public interface IRenderGeometry
    {
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
    }
}
