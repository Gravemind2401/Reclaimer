using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public enum VertexWeights
    {
        None,
        Skinned,
        Rigid
    }

    public enum IndexFormat
    {
        Triangles = 3,
        Stripped = 5
    }

    [Flags]
    public enum MaterialFlags
    {
        Transparent,
        ColourChange
    }

    public enum MaterialUsage
    {
        Diffuse,
        DiffuseDetail,
        ColourChange,
        Normal,
        NormalDetail,
        SelfIllumination,
        Specular
    }

    public enum TintUsage
    {
        Albedo,
        SelfIllumination,
        Specular
    }
}
