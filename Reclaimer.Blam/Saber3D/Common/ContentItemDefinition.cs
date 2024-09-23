using Reclaimer.Utilities;

namespace Reclaimer.Saber3D.Common
{
    public abstract class ContentItemDefinition : ItemDefinition, IExtractable
    {
        protected ContentItemDefinition(IPakItem item)
            : base(item)
        { }

        string IExtractable.SourceFile => Item.Container.FileName;
        int IExtractable.Id => Item.Address;
        string IExtractable.Name => Item.Name;
        string IExtractable.Class => Item.ItemType.ToString();
    }
}
