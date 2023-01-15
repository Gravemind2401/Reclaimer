using Reclaimer.Blam.Utilities;
using Reclaimer.Saber3D.Halo1X;

namespace Reclaimer.Saber3D.Common
{
    public abstract class ContentItemDefinition : ItemDefinition, IExtractable
    {
        protected ContentItemDefinition(PakItem item)
            : base(item)
        { }

        string IExtractable.SourceFile => Item.Container.FileName;
        int IExtractable.Id => Item.Address;
        string IExtractable.Name => Item.Name;
        string IExtractable.Class => Item.ItemType.ToString();
    }
}
