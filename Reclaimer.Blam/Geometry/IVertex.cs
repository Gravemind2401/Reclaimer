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
        IReadOnlyList<IXMVector> Position { get; }
        IReadOnlyList<IXMVector> TexCoords { get; }
        IReadOnlyList<IXMVector> Normal { get; }
        IReadOnlyList<IXMVector> Binormal { get; }
        IReadOnlyList<IXMVector> Tangent { get; }
        IReadOnlyList<IXMVector> BlendIndices { get; }
        IReadOnlyList<IXMVector> BlendWeight { get; }
        IReadOnlyList<IXMVector> Color { get; }
    }
}
