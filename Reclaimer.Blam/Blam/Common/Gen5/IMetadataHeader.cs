namespace Reclaimer.Blam.Common.Gen5
{
    public interface IMetadataHeader
    {
        int HeaderSize { get; }
        List<TagDependency> Dependencies { get; }
        List<DataBlock> DataBlocks { get; }
        List<TagStructureDefinition> StructureDefinitions { get; }
        List<DataBlockReference> DataReferences { get; }
        List<TagBlockReference> TagReferences { get; }

        int GetSectionOffset(int section);

        sealed int SectionCount => DataBlocks.Max(b => b.Section + 1);
    }
}