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
        ColourChange,
        TerrainBlend
    }

    public enum MaterialUsage
    {
        BlendMap = -1,
        Diffuse = 0,
        DiffuseDetail = 1,
        ColourChange = 2,
        Normal = 3,
        NormalDetail = 4,
        SelfIllumination = 5,
        Specular = 6
    }

    public enum TintUsage
    {
        Albedo,
        SelfIllumination,
        Specular
    }
}
