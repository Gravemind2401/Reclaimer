using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.HaloInfinite
{
    public class MetadataHeader : IMetadataHeader
    {
        public TagHeader Header { get; }

        public List<TagDependency> Dependencies { get; }

        public List<DataBlock> DataBlocks { get; }

        public List<TagStructureDefinition> StructureDefinitions { get; }

        public List<DataBlockReference> DataReferences { get; }

        public List<TagBlockReference> TagReferences { get; }

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
