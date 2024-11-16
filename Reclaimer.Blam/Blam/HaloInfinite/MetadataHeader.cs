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

        #region IMetadataHeader

        int IMetadataHeader.HeaderSize => Header.HeaderSize;

        string IMetadataHeader.GetStringByOffset(int offset) => throw new NotImplementedException();
        string IMetadataHeader.GetStringByHash(uint hash) => StringMapper.Instance.StringMappings.TryGetValue(unchecked((int)hash), out var value) ? value : hash.ToString();

        #endregion

        public MetadataHeader(DependencyReader reader)
        {
            reader.RegisterInstance(this);
            reader.RegisterInstance<IMetadataHeader>(this);

            Header = reader.ReadObject<TagHeader>();
            Dependencies = reader.ReadList<TagDependency>(Header.DependencyCount);
            DataBlocks = reader.ReadList<DataBlock>(Header.DataBlockCount);
            StructureDefinitions = reader.ReadList<TagStructureDefinition>(Header.TagStructureCount);
            DataReferences = reader.ReadList<DataBlockReference>(Header.DataReferenceCount);
            TagReferences = reader.ReadList<TagBlockReference>(Header.TagReferenceCount);

            for (var i = 0; i < DataBlocks.Count; i++)
                DataBlocks[i].Index = i;

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
