using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo5
{
    public class MetadataHeader : IMetadataHeader
    {
        private readonly List<string> stringTable;
        private readonly Dictionary<int, int> stringsByOffset;
        private readonly Dictionary<uint, int> stringsByHash;

        public TagHeader Header { get; }

        public List<TagDependency> Dependencies { get; }

        public List<DataBlock> DataBlocks { get; }

        public List<TagStructureDefinition> StructureDefinitions { get; }

        public List<DataBlockReference> DataReferences { get; }

        public List<TagBlockReference> TagReferences { get; }

        public List<StringId> StringIds { get; }

        public int SectionCount => DataBlocks.Max(b => b.Section) + 1;

        int IMetadataHeader.HeaderSize => Header.HeaderSize;

        public MetadataHeader(DependencyReader reader)
        {
            reader.RegisterInstance(this);
            reader.RegisterInstance<IMetadataHeader>(this);

            Header = reader.ReadObject<TagHeader>();
            Dependencies = reader.ReadArray<TagDependency>(Header.DependencyCount).ToList();
            DataBlocks = reader.ReadArray<DataBlock>(Header.DataBlockCount).ToList();
            StructureDefinitions = reader.ReadArray<TagStructureDefinition>(Header.TagStructureCount).ToList();
            DataReferences = reader.ReadArray<DataBlockReference>(Header.DataReferenceCount).ToList();
            TagReferences = reader.ReadArray<TagBlockReference>(Header.TagReferenceCount).ToList();
            StringIds = reader.ReadArray<StringId>(Header.StringIdCount).ToList();

            stringTable = new List<string>(StringIds.Count);
            stringsByOffset = new Dictionary<int, int>(StringIds.Count);
            stringsByHash = new Dictionary<uint, int>(StringIds.Count);

            var startPos = reader.BaseStream.Position;
            while (reader.BaseStream.Position < startPos + Header.StringTableSize)
            {
                var relative = reader.BaseStream.Position - startPos;
                var currentValue = reader.ReadNullTerminatedString();
                stringsByOffset.Add((int)relative, stringTable.Count);

                var hash = MurMur3.Hash32(currentValue);
                if (!stringsByHash.ContainsKey(hash))
                    stringsByHash.Add(hash, stringTable.Count);

                stringTable.Add(currentValue);
            }
        }

        public int GetSectionOffset(int section)
        {
            Exceptions.ThrowIfIndexOutOfRange(section, SectionCount);

            //treat 0 as being the header
            if (section == 0)
                return 0;

            var totalPrevious = DataBlocks.Where(b => b.Section < section).Sum(b => b.Size);
            return Header.HeaderSize + totalPrevious;
        }

        internal string GetStringByOffset(int offset)
        {
            return stringsByOffset.TryGetValue(offset, out var index) ? stringTable[index] : null;
        }

        internal string GetStringByHash(uint hash)
        {
            return stringsByHash.TryGetValue(hash, out var index) ? stringTable[index] : null;
        }
    }
}
