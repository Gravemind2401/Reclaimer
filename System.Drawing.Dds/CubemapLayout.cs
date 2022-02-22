using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    public class CubemapLayout
    {
        private static readonly CubemapLayout invalid = new CubemapLayout();
        public static CubemapLayout NonCubemap => invalid;

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
