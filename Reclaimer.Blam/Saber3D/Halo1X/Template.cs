using Reclaimer.Saber3D.Halo1X.Geometry;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Template
    {
        private readonly PakItem item;

        public List<DataBlock> Blocks { get; }

        public string Name => Blocks.OfType<StringBlock0xE502>().SingleOrDefault()?.Value;
        public BoundsBlock0x0803 Bounds => Blocks.OfType<BoundsBlock0x0803>().SingleOrDefault();
        public NodeGraphBlock0xF000 NodeGraph => Blocks.OfType<NodeGraphBlock0xF000>().SingleOrDefault();
        public List<BoneBlock> Bones => Blocks.OfType<BoneListBlock>().SingleOrDefault()?.Bones;
        public List<MaterialReferenceBlock> Materials => Blocks.OfType<MaterialListBlock>().SingleOrDefault()?.Materials;
        public MatrixListBlock0x0D03 MatrixList => Blocks.OfType<TransformBlock0x0503>().SingleOrDefault()?.MatrixList;

        public Template(PakItem item)
        {
            this.item = item;

            using (var x = item.Container.CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(0, SeekOrigin.Begin);
                var root = reader.ReadBlock();

                Blocks = (root as TemplateBlock)?.ChildBlocks;
            }
        }
    }
}
