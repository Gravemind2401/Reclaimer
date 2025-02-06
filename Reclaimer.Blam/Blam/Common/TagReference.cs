using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.Common
{
    [FixedSize(16, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(8, MinVersion = (int)CacheType.Halo2Xbox, MaxVersion = (int)CacheType.Halo3Beta)]
    [FixedSize(16, MinVersion = (int)CacheType.Halo3Beta)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly record struct TagReference : IWriteable
    {
        public static TagReference NullReference { get; } = new TagReference(null, -1, -1);

        private readonly ICacheFile cache;
        private readonly int classId;
        private readonly int tagId;

        public int TagId => (short)(tagId & ushort.MaxValue);
        public IIndexItem Tag => IsValid ? cache.TagIndex[TagId] : null;
        public bool IsValid => TagId >= 0 && cache != null;

        public TagReference(ICacheFile cache, int classId, int tagId)
        {
            this.cache = cache;
            this.classId = classId;
            this.tagId = tagId;
        }

        public TagReference(ICacheFile cache, EndianReader reader)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            ArgumentNullException.ThrowIfNull(reader);

            classId = reader.ReadInt32();

            if (cache.Metadata.Generation != CacheGeneration.Gen2)
                reader.Seek(8, SeekOrigin.Current);
            else if (cache.CacheType <= CacheType.Halo2Beta)
                reader.Seek(8, SeekOrigin.Current);

            tagId = reader.ReadInt32();
        }

        public void Write(EndianWriter writer) => Write(writer, null);

        public void Write(EndianWriter writer, double? version)
        {
            writer.Write(classId);

            if (cache.Metadata.Generation != CacheGeneration.Gen2)
            {
                writer.Write(0);
                writer.Write(0);
            }
            else if (cache.CacheType <= CacheType.Halo2Beta)
            {
                writer.Write(0);
                writer.Write(0);
            }

            writer.Write(tagId);
        }

        private string GetDebuggerDisplay()
        {
            var tag = Tag;
            return tag == null ? "{null reference}" : $"{{[{tag.ClassCode}] {tag.TagName}}}";
        }
    }
}
