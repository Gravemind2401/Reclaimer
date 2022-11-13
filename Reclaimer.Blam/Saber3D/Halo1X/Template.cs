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
