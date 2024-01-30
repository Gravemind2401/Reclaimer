using Reclaimer.Utilities;

namespace Reclaimer.Drawing
{
    public interface IBitmap : IExtractable
    {
        //TODO: replace this with a constructable class similar to Scene

        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }
}
