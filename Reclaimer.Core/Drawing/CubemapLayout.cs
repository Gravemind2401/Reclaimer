using System.Drawing;

namespace Reclaimer.Drawing
{
    public class CubemapLayout
    {
        public static CubemapLayout NonCubemap { get; } = new CubemapLayout();

        public CubemapFace Face1 { get; set; }
        public CubemapFace Face2 { get; set; }
        public CubemapFace Face3 { get; set; }
        public CubemapFace Face4 { get; set; }
        public CubemapFace Face5 { get; set; }
        public CubemapFace Face6 { get; set; }

        public RotateFlipType Orientation1 { get; set; }
        public RotateFlipType Orientation2 { get; set; }
        public RotateFlipType Orientation3 { get; set; }
        public RotateFlipType Orientation4 { get; set; }
        public RotateFlipType Orientation5 { get; set; }
        public RotateFlipType Orientation6 { get; set; }

        public bool IsValid => (Face1 | Face2 | Face3 | Face4 | Face5 | Face6) > 0;
    }
}
