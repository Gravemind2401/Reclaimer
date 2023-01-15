using Adjutant.Geometry;
using Reclaimer.Audio;
using Reclaimer.Drawing;

namespace Reclaimer.Blam.Utilities
{
    public interface IExtractable
    {
        string SourceFile { get; }
        int Id { get; }
        string Name { get; }
        string Class { get; }
    }

    public interface IBitmap : IExtractable
    {
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }

    public interface IRenderGeometry : IExtractable
    {
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
