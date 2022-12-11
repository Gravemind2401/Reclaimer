namespace Reclaimer.Blam.Common
{
    public interface ITagIndex<out TIndexItem> : IEnumerable<TIndexItem> where TIndexItem : IIndexItem
    {
        int TagCount { get; }
        TIndexItem this[int index] { get; }
        TIndexItem GetGlobalTag(string classCode);
    }
}
