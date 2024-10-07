using System.Collections;
using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    internal class ModuleItemGroup : IGrouping<int, ModuleItem>, IReadOnlyList<ModuleItem>
    {
        private readonly List<ModuleItem> items = new();

        public int Key { get; }

        public int Count => items.Count;
        public ModuleItem this[int index] => items[index];

        public ModuleItemGroup(params ModuleItem[] initialItems)
            : this(initialItems.AsEnumerable())
        { }

        public ModuleItemGroup(IEnumerable<ModuleItem> initialItems)
        {
            ArgumentNullException.ThrowIfNull(initialItems);

            if (!initialItems.Any())
                throw new ArgumentException("Collection cannot be empty", nameof(initialItems));
            
            if (initialItems.DistinctBy(i => i.GlobalTagId).Skip(1).Any())
                throw new ArgumentException($"All items must have the same {nameof(ModuleItem.GlobalTagId)}", nameof(initialItems));

            items.AddRange(initialItems);
            Key = items[0].GlobalTagId;
        }

        public void Add(ModuleItem item)
        {
            if (items.Any(i => i.Module.FileName == item.Module.FileName))
                return;

            items.Add(item);
            items.Sort((a, b) => Path.GetFileName(b.Module.FileName).CompareTo(Path.GetFileName(a.Module.FileName)));
        }

        public IEnumerator<ModuleItem> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
    }
}
