using Reclaimer.Saber3D.Halo1X;

namespace Reclaimer.Saber3D.Common
{
    public abstract class ItemDefinition
    {
        protected readonly PakItem Item;
        protected readonly PakFile Container;

        protected ItemDefinition(PakItem item)
        {
            Item = item;
            Container = item.Container;
        }
    }
}
