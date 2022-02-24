using Reclaimer.Saber3D.Common;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Reclaimer.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X
{
    public class PakFile : IPakFile
    {
        public string FileName { get; }

        public IReadOnlyList<PakItem> Items { get; }

        public PakFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            FileName = fileName;

            using (var reader = CreateReader())
            {
                var count = reader.ReadInt32();
                var items = new List<PakItem>(count);
                for (int i = 0; i < count; i++)
                    items.Add(new PakItem(this, reader));
                Items = items;
            }
        }

        public DependencyReader CreateReader()
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder.LittleEndian);

            return reader;
        }

        IReadOnlyList<IPakItem> IPakFile.Items => Items;
    }

    public class PakItem : IPakItem
    {
        public PakFile Container { get; }

        public int Address { get; }
        public int Size { get; }
        public string Name { get; }
        public PakItemType ItemType { get; }
        public int Unknown0 { get; }
        public int Unknown1 { get; }

        public PakItem(PakFile parent, EndianReader reader)
        {
            Container = parent;
            Address = reader.ReadInt32();
            Size = reader.ReadInt32();
            Name = reader.ReadString();
            ItemType = (PakItemType)reader.ReadInt32();
            Unknown0 = reader.ReadInt32();
            Unknown1 = reader.ReadInt32();
        }

        public override string ToString() => Name;

        IPakFile IPakItem.Container => Container;
    }
}
