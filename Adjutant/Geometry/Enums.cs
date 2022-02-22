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
        Default = 0,
        LineList = 1,
        LineStrip = 2,
        TriangleList = 3,
        TrianglePatch = 4,
        TriangleStrip = 5,
        QuadList = 6,
        RectList = 7
    }

    [Flags]
    public enum MaterialFlags
    {
        None = 0,
        Transparent = 1,
        ColourChange = 2,
        TerrainBlend = 4
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
