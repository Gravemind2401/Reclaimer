using Adjutant.Geometry;
using Reclaimer.Audio;
using Reclaimer.Drawing;
using Reclaimer.Geometry;

namespace Reclaimer.Blam.Utilities
{
    public interface IContentProvider<TContent> : IExtractable
    {
        TContent GetContent();
    }

    public interface IExtractable
    {
        /// <summary>
        /// The full path of the file this content originates from.
        /// </summary>
        string SourceFile { get; }

        /// <summary>
        /// The ID associated with this content that is unique within the source file.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The name of the content.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The class or category that the content is associated with.
        /// </summary>
        string Class { get; }
    }

    public interface IBitmap : IExtractable
    {
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }

    public interface IRenderGeometry : IContentProvider<Model>
    {
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
        IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes);

        Model IContentProvider<Model>.GetContent() => ReadGeometry(0).ConvertToScene();
    }

    public interface ISoundContainer
    {
        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
