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
        int BitmapCount { get; }
        DdsImage ToDds(int index);
    }

    public interface IRenderGeometry
    {
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
    }
}
