namespace Reclaimer.Blam.Utilities
{
    public interface IPointerExpander
    {
        long Expand(int pointer);
        int Contract(long pointer);
    }
}
