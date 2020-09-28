using Adjutant.Audio;
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
        string SourceFile { get; }
        int Id { get; }
        string Name { get; }
        string Class { get; }
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }

    public interface IRenderGeometry
    {
        string SourceFile { get; }
        int Id { get; }
        string Name { get; }
        string Class { get; }
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
    }

    public interface ISoundContainer
    {
        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
