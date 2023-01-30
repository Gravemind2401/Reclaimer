namespace Reclaimer.IO
{
    internal interface IDataBuffer
    {
        Type DataType { get; }
        int SizeOf { get; }
        int Count { get; }
        ReadOnlySpan<byte> Buffer { get; }
        int Start { get; }
        int Stride { get; }
        int Offset { get; }
        ReadOnlySpan<byte> GetBytes(int index);
    }
}
