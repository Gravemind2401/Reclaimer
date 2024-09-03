namespace Reclaimer.Blam.Halo5
{
    [Flags]
    public enum FileEntryFlags : byte
    {
        Compressed = 0,
        HasBlocks = 1,
        RawFile = 2
    }

    public enum DataBlockSection : short
    {
        Header = 0,
        TagData = 1,
        ResourceData = 2
    }

    public enum StructureType : short
    {
        Main = 0,
        TagBlock = 1,
        Resource = 2,
        Custom = 3
    }
}
