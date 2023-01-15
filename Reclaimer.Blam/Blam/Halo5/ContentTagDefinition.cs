using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo5
{
    public abstract class ContentTagDefinition : TagDefinition, IExtractable
    {
        protected ContentTagDefinition(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        string IExtractable.SourceFile => Item.Module.FileName;
        int IExtractable.Id => Item.GlobalTagId;
        string IExtractable.Name => Item.FullPath;
        string IExtractable.Class => Item.ClassName;
    }
}
