using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public interface IVertex
    {
        IXMVector[] Position { get; }
        IXMVector[] TexCoords { get; }
        IXMVector[] Normal { get; }
        IXMVector[] Binormal { get; }
        IXMVector[] Tangent { get; }
        IXMVector[] BlendIndices { get; }
        IXMVector[] BlendWeight { get; }
        IXMVector[] Color { get; }
    }
}
