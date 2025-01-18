using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;

namespace Reclaimer.Saber3D.Halo1X
{
    public class InplacePakFile : IPakFile
    {
        public string FileName { get; }
        public IReadOnlyList<InplacePakItem> Items { get; }

        IReadOnlyList<IPakItem> IPakFile.Items => Items;
        bool IPakFile.IsMcc => true;

        public InplacePakFile(string fileName)
        {
            Exceptions.ThrowIfFileNotFound(fileName);

            FileName = fileName;

            using (var reader = CreateReader())
            {
                var count = reader.ReadInt32();
                var items = new List<InplacePakItem>(count);
                for (var i = 0; i < count; i++)
                    items.Add(new InplacePakItem(this, reader));

                Items = items;
            }
        }

        public DependencyReader CreateReader()
        {
            return new DependencyReader(new PakStream(this), ByteOrder.LittleEndian);
        }

        public IPakItem FindItem(PakItemType itemType, string name, bool external)
        {
            return Items.FirstOrDefault(i => i.Name == name);
        }
    }

    [FixedSize(328)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class InplacePakItem : IPakItem
    {
        public InplacePakFile Container { get; }

        public string Name { get; }
        public int Unknown1 { get; }
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public int MipCount { get; }
        public int FaceCount { get; }
        public TextureFormat Format { get; }
        public long Unknown2 { get; }
        public int Size { get; }
        public int Address { get; }

        IPakFile IPakItem.Container => Container;
        PakItemType IPakItem.ItemType => PakItemType.Textures;

        public InplacePakItem(InplacePakFile parent, EndianReader reader)
        {
            Container = parent;
            reader.ReadInt32();

            Name = reader.ReadNullTerminatedString(256);

            reader.ReadInt32();
            reader.ReadInt32();

            Unknown1 = reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Depth = reader.ReadInt32();
            MipCount = reader.ReadInt32();
            FaceCount = reader.ReadInt32();
            Format = (TextureFormat)reader.ReadInt32();
            Unknown2 = reader.ReadInt64();

            reader.ReadInt32(); //size
            reader.ReadInt32(); //zero

            Size = reader.ReadInt32();
            Address = reader.ReadInt32();

            reader.ReadInt32(); //zero
            reader.ReadInt32(); //size

            //this is to maintain compatibility with KnownTextureFormat
            Format = Format switch
            {
                TextureFormat.AlsoDXT1 => TextureFormat.DXT1,
                TextureFormat.AlsoA8Y8 => TextureFormat.A8Y8,
                TextureFormat.AlsoA8R8G8B8 => TextureFormat.A8R8G8B8,
                TextureFormat.AlsoDXN => TextureFormat.DXN,
                _ => Format
            };
        }

        private string GetDebuggerDisplay() => Name;
    }
}
