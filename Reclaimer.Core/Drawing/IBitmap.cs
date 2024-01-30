using Reclaimer.Utilities;

namespace Reclaimer.Drawing
{
    public interface IBitmap : IExtractable
    {
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }
}
