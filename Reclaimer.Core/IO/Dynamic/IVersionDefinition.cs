namespace Reclaimer.IO.Dynamic
{
    internal interface IVersionDefinition
    {
        IEnumerable<IFieldDefinition> Fields { get; }
        ByteOrder? ByteOrder { get; }
        double? MaxVersion { get; }
        string MaxVersionDisplay { get; }
        double? MinVersion { get; }
        string MinVersionDisplay { get; }
        long? Size { get; }
        IFieldDefinition VersionField { get; }
        IFieldDefinition DataLengthField { get; }
    }
}