using Reclaimer.IO;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Scene : INodeGraph
    {
        private readonly PakItem item;

        public List<DataBlock> Blocks { get; } = new List<DataBlock>();

        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }

        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;
        public UnknownBoundsBlock0x2002 Bounds => Blocks.OfType<UnknownListBlock0x1F01>().SingleOrDefault().Bounds;
        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();

        List<BoneBlock> INodeGraph.Bones { get; }

        public Scene(PakItem item)
        {
            this.item = item;

            using (var x = CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);

                while (reader.Position < item.Size)
                    Blocks.Add(reader.ReadBlock(this));

                NodeLookup = NodeGraph.AllDescendants.Where(n => n.MeshId.HasValue).ToDictionary(n => n.MeshId.Value);
            }
        }

        private EndianReader CreateReader()
        {
            var reader = item.Container.CreateReader();
            reader.RegisterInstance(this);
            reader.RegisterInstance(item);
            return reader;
        }
    }
}
