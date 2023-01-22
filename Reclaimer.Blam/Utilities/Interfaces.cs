using Adjutant.Geometry;
using Reclaimer.Audio;
using Reclaimer.Drawing;
using Reclaimer.Geometry;

namespace Reclaimer.Blam.Utilities
{
    public interface ICachedContentProvider<TContent> : IContentProvider<TContent>
    {
        TContent Content { get; }
        TContent IContentProvider<TContent>.GetContent() => Content;
    }

    public interface IContentProvider<TContent> : IExtractable
    {
        TContent GetContent();

        sealed bool IsCached => this is ICachedContentProvider<TContent>;
        sealed ICachedContentProvider<TContent> AsCached() => this as ICachedContentProvider<TContent> ?? new CachedContentProvider(this);

        //wrapper around an instance of IContentProvider<TContent> to ensure the content
        //is only loaded once, then the same TContent instance is returned each time after
        private sealed class CachedContentProvider : ICachedContentProvider<TContent>
        {
            private readonly IContentProvider<TContent> contentSource;
            private readonly TContent content;

            public CachedContentProvider(IContentProvider<TContent> provider)
            {
                contentSource = provider;
                content = provider.GetContent();
            }

            public string SourceFile => contentSource.SourceFile;
            public int Id => contentSource.Id;
            public string Name => contentSource.Name;
            public string Class => contentSource.Class;
            public TContent Content => content;
        }
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

    public interface IRenderGeometry : IContentProvider<Scene>
    {
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
        IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes);

        Scene IContentProvider<Scene>.GetContent() => ReadGeometry(0).ConvertToScene();
    }

    public interface ISoundContainer
    {
        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
