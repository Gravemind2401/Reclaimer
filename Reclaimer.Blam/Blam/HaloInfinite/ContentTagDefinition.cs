using Reclaimer.Utilities;

namespace Reclaimer.Blam.HaloInfinite
{
    public abstract class ContentTagDefinition : TagDefinition, IExtractable
    {
        protected ContentTagDefinition(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        string IExtractable.SourceFile => Item.Module.FileName;
        int IExtractable.Id => Item.GlobalTagId;
        string IExtractable.Name => Item.TagName;
        string IExtractable.Class => Item.ClassName;
    }

    public abstract class ContentTagDefinition<TContent> : ContentTagDefinition, IContentProvider<TContent>
    {
        protected ContentTagDefinition(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        public abstract TContent GetContent();

        TContent IContentProvider<TContent>.GetContent() => GetContent();
    }
}
