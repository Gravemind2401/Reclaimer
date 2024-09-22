namespace Reclaimer.Saber3D.Common
{
    public abstract class ItemDefinition
    {
        protected readonly IPakItem Item;
        protected readonly IPakFile Container;

        protected ItemDefinition(IPakItem item)
        {
            Item = item;
            Container = item.Container;
        }
    }
}
