using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using Reclaimer.Saber3D.Halo1X.Geometry;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Template : ItemDefinition, INodeGraph
    {
        public List<DataBlock> Blocks { get; }
        public Dictionary<int, NodeGraphBlock0xF000> NodeLookup { get; }

        public string Name => Blocks.OfType<StringBlock0xE502>().SingleOrDefault()?.Value;
        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;
        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();
        public List<BoneBlock> Bones => Blocks.OfType<BoneListBlock>().SingleOrDefault()?.Bones;
        public string UnknownString => Blocks.OfType<StringBlock0x0403>().SingleOrDefault()?.Value;
        public MatrixListBlock0x0D03 MatrixList => Blocks.OfType<TransformBlock0x0503>().SingleOrDefault()?.MatrixList;
        public BoundsBlock0x0803 Bounds => Blocks.OfType<BoundsBlock0x0803>().SingleOrDefault();

        public Template(PakItem item)
            : base(item)
        {
            using (var x = CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);
                var root = reader.ReadBlock(this);

                Blocks = (root as TemplateBlock)?.ChildBlocks;
                NodeLookup = NodeGraph.AllDescendants.Where(n => n.MeshId.HasValue).ToDictionary(n => n.MeshId.Value);
            }
        }

        private EndianReader CreateReader()
        {
            var reader = Container.CreateReader();
            reader.RegisterInstance(this);
            reader.RegisterInstance(Item);
            return reader;
        }
    }
}
