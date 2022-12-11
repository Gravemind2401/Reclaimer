namespace Reclaimer.Blam.Common
{
    public interface IStringIndex : IEnumerable<string>
    {
        int StringCount { get; }
        string this[int id] { get; }
        int GetStringId(string value);
    }
}
