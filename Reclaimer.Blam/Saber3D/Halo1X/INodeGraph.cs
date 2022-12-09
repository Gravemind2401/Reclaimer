using Reclaimer.Blam.Utilities;
using Reclaimer.Saber3D.Halo1X.Geometry;

namespace Reclaimer.Saber3D.Halo1X
{
    public interface INodeGraph : IRenderGeometry
    {
        internal PakItem Item { get; }

        List<MaterialReferenceBlock> Materials { get; }
        NodeGraphBlock0xF000 NodeGraph { get; }
        Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }
        List<BoneBlock> Bones { get; }

        string IExtractable.SourceFile => Item.Container.FileName;
        int IExtractable.Id => Item.Address;
        string IExtractable.Name => Item.Name;
        string IExtractable.Class => Item.ItemType.ToString();
    }
}
