namespace Reclaimer.Audio
{
    public interface ISoundContainer
    {
        //TODO: replace with IContentProvider<GameSound>

        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
