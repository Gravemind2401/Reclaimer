namespace Reclaimer.Blam.Halo1
{
    public interface ITagIndexGen1
    {
        int Magic { get; }
        int VertexDataCount { get; }
        int VertexDataOffset { get; }
        int IndexDataCount { get; }
        int IndexDataOffset { get; }
    }
}
