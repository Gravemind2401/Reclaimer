using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class PakFile : IPakFile
    {
        private string PakStreamFileName => isCompressed ? "pak_stream.s3dpak" : "pak_stream_decompressed.s3dpak";

        private readonly bool isCompressed;
        private readonly ILookup<PakItemType, PakItem> itemsByType;
        private PakFile pakStream;

        public string FileName { get; }
        public IReadOnlyList<PakItem> Items { get; }

        public PakFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            FileName = fileName;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, ByteOrder.LittleEndian))
            {
                //read the first few fields as if it was uncompressed and see if it makes sense

                //in compressed files this will still be a count
                var itemCount = reader.ReadInt32();

                //in compressed files this will still be an address
                var address = reader.ReadInt32();

                //in compressed files this will be an address, which would also look about right as a size so its still ambiguous
                var size = reader.ReadInt32();
                
                //in compressed files this would the third chunk address - way too big for a string length
                //can also be zero in compressed files that only have one chunk
                var nameLength = reader.ReadInt32();

                isCompressed = nameLength <= 0 || nameLength > 1024;
            }

            using (var reader = CreateReader())
            {
                var count = reader.ReadInt32();
                var items = new List<PakItem>(count);
                for (var i = 0; i < count; i++)
                    items.Add(new PakItem(this, reader));
                
                Items = items;
                itemsByType = items.ToLookup(i => i.ItemType);
            }
        }

        public DependencyReader CreateReader()
        {
            var dataStream = isCompressed
                ? (Stream)new PakStream(FileName)
                : new FileStream(FileName, FileMode.Open, FileAccess.Read);

            return new DependencyReader(dataStream, ByteOrder.LittleEndian);
        }

        public PakItem FindItem(PakItemType itemType, string name, bool external)
        {
            var item = itemsByType[itemType].FirstOrDefault(i => i.Name == name);
            if (item != null || !external)
                return item;

            if (pakStream == null)
            {
                var targetFile = Path.Combine(Path.GetDirectoryName(FileName), PakStreamFileName);
                if (targetFile == FileName || !File.Exists(targetFile))
                    return null;

                pakStream = new PakFile(targetFile);
            }

            return pakStream.FindItem(itemType, name, false);
        }

        IReadOnlyList<IPakItem> IPakFile.Items => Items;
        IPakItem IPakFile.FindItem(PakItemType itemType, string name, bool external) => FindItem(itemType, name, external);
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
