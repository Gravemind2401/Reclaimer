using Reclaimer.Saber3D.Halo1X.Geometry;

namespace Reclaimer.Saber3D.Halo1X
{
    public interface INodeGraph
    {
        internal PakItem Item { get; }

        List<MaterialReferenceBlock> Materials { get; }
        NodeGraphBlock0xF000 NodeGraph { get; }
        Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }
        List<BoneBlock> Bones => null;
    }
}
