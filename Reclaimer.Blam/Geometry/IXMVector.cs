using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public interface IXMVector
    {
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float W { get; set; }
        float Length { get; }

        VectorType VectorType { get; }
    }
}