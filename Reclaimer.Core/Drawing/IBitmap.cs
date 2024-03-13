using Reclaimer.Utilities;

namespace Reclaimer.Drawing
{
    public interface IBitmap : IExtractable
    {
        //TODO: replace this with a constructable class similar to Scene

        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        float GetSubmapGamma(int index) => 2.2f; //default to sRGB
        DdsImage ToDds(int index);
    }
}
