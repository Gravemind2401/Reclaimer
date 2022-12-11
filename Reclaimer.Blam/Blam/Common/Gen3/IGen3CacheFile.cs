namespace Reclaimer.Blam.Common.Gen3
{
    public interface IGen3CacheFile : ICacheFile
    {
        IGen3Header Header { get; }
        ILocaleIndex LocaleIndex { get; }
        bool UsesStringEncryption { get; }
    }
}
