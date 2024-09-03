using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.HaloInfinite
{
    public class MetadataHeader
    {
        public TagHeader Header { get; }

        public List<TagDependency> Dependencies { get; }

        public List<DataBlock> DataBlocks { get; }

        public List<TagStructureDefinition> StructureDefinitions { get; }

        public List<DataBlockReference> DataReferences { get; }

        public List<TagBlockReference> TagReferences { get; }
        public int SectionCount => DataBlocks.Max(b => b.Section) + 1;

        public MetadataHeader(DependencyReader reader)
        {
            reader.RegisterInstance(this);

            Header = reader.ReadObject<TagHeader>();
            Dependencies = reader.ReadEnumerable<TagDependency>(Header.DependencyCount).ToList();
            DataBlocks = reader.ReadEnumerable<DataBlock>(Header.DataBlockCount).ToList();
            StructureDefinitions = reader.ReadEnumerable<TagStructureDefinition>(Header.TagStructureCount).ToList();
            DataReferences = reader.ReadEnumerable<DataBlockReference>(Header.DataReferenceCount).ToList();
            TagReferences = reader.ReadEnumerable<TagBlockReference>(Header.TagReferenceCount).ToList();
            reader.Seek(Header.HeaderSize, System.IO.SeekOrigin.Begin);
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
    }
}
