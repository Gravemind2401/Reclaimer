using Adjutant.Geometry;
using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public interface IBitmap
    {
        int Id { get; }
        string Name { get; }
        string Class { get; }
        string SourceFile { get; }
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
        IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes);
    }

    public interface ISoundContainer
    {
        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
