namespace Reclaimer.IO.Dynamic
{
    internal interface IVersionDefinition
    {
        IEnumerable<IFieldDefinition> Fields { get; }
        ByteOrder? ByteOrder { get; }
        double? MaxVersion { get; }
        double? MinVersion { get; }
        long? Size { get; }
        IFieldDefinition VersionField { get; }
        IFieldDefinition DataLengthField { get; }
    }
}