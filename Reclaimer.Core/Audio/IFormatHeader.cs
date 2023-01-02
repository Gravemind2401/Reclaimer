namespace Reclaimer.Audio
{
    public interface IFormatHeader
    {
        int Length { get; }
        byte[] GetBytes();
    }
}
