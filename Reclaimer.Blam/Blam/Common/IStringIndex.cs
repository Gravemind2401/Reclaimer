namespace Reclaimer.Blam.Common
{
    public interface IStringIndex : IEnumerable<string>
    {
        int StringCount { get; }
        string this[int id] { get; }
        bool TryGetValue(int id, out string value);
        int GetStringId(string value);
    }
}
