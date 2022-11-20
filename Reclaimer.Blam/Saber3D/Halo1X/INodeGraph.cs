using Reclaimer.Saber3D.Halo1X.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X
{
    public interface INodeGraph
    {
        List<MaterialReferenceBlock> Materials { get; }
        NodeGraphBlock0xF000 NodeGraph { get; }
        Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }
        List<BoneBlock> Bones { get; }
    }
}
