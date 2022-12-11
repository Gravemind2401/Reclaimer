namespace Reclaimer.Blam.Utilities
{
    public interface IAddressTranslator
    {
        long GetAddress(long pointer);
        long GetPointer(long address);
    }
}
