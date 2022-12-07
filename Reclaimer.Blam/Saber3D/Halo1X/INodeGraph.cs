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

        IEnumerable<IBitmap> IRenderGeometry.GetAllBitmaps()
        {
            return from m in Materials
                   let i = Item.Container.FindItem(Common.PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }

        IEnumerable<IBitmap> IRenderGeometry.GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            return from m in shaderIndexes.Select(i => Materials[i])
                   let i = Item.Container.FindItem(Common.PakItemType.Textures, m.Value, true)
                   where i != null
                   select new Texture(i);
        }
    }
}
