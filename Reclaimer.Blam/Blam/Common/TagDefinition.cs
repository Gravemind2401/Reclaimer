namespace Reclaimer.Blam.Common
{
    public abstract class TagDefinition
    {
        protected readonly IIndexItem Item;
        protected readonly ICacheFile Cache;

        protected TagDefinition(IIndexItem item)
        {
            Item = item;
            Cache = item.CacheFile;
        }
    }
}
