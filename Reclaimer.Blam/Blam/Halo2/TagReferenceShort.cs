using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.Common
{
    [FixedSize(4)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly record struct TagReferenceShort : IWriteable
    {
        public static TagReferenceShort NullReference { get; } = new TagReferenceShort(null, -1);

        private readonly ICacheFile cache;
        private readonly int tagId;

        public int TagId => (short)(tagId & ushort.MaxValue);
        public IIndexItem Tag => IsValid ? cache.TagIndex[TagId] : null;
        public bool IsValid => TagId >= 0 && cache != null && TagId < cache.TagIndex.TagCount;

        public TagReferenceShort(ICacheFile cache, int tagId)
        {
            this.cache = cache;
            this.tagId = tagId;
        }

        public TagReferenceShort(ICacheFile cache, EndianReader reader)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            ArgumentNullException.ThrowIfNull(reader);

            tagId = reader.ReadInt32();
        }

        public void Write(EndianWriter writer) => Write(writer, null);
        public void Write(EndianWriter writer, double? version) => writer.Write(tagId);

        private string GetDebuggerDisplay()
        {
            var tag = Tag;
            return tag == null ? "{null reference}" : $"{{[{tag.ClassCode}] {tag.TagName}}}";
        }
    }
}
