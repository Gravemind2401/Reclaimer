using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common
{
    public abstract class ContentTagDefinition : TagDefinition, IExtractable
    {
        protected ContentTagDefinition(IIndexItem item)
            : base(item)
        { }

        string IExtractable.SourceFile => Item.CacheFile.FileName;
        int IExtractable.Id => Item.Id;
        string IExtractable.Name => Item.TagName;
        string IExtractable.Class => Item.ClassName;
    }
}
