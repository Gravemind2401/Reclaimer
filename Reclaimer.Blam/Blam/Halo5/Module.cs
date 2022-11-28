using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo5
{
    public class Module
    {
        internal const int ModuleHeader = 0x64686f6d;

        private readonly TagIndex tagIndex;
        private readonly List<Module> linkedModules;

        public string FileName { get; }

        public ModuleType ModuleType => Header.Version;
        public ModuleHeader Header { get; }

        public List<ModuleItem> Items { get; }
        public Dictionary<int, string> Strings { get; }
        public List<int> Resources { get; }
        public List<Block> Blocks { get; }

        public long DataAddress { get; }

        public Module(string fileName) : this(fileName, null) { }

        private Module(string fileName, Module parentModule)
        {
            FileName = fileName;

            using var reader = CreateReader();

            Header = reader.ReadObject<ModuleHeader>();

            Items = new List<ModuleItem>(Header.ItemCount);
            for (var i = 0; i < Header.ItemCount; i++)
                Items.Add(reader.ReadObject<ModuleItem>((int)Header.Version));

            var origin = reader.BaseStream.Position;
            Strings = new Dictionary<int, string>();
            while (reader.BaseStream.Position < origin + Header.StringsSize)
                Strings.Add((int)(reader.BaseStream.Position - origin), reader.ReadNullTerminatedString());

            Resources = new List<int>(Header.ResourceCount);
            for (var i = 0; i < Header.ResourceCount; i++)
                Resources.Add(reader.ReadInt32());

            Blocks = new List<Block>(Header.BlockCount);
            for (var i = 0; i < Header.BlockCount; i++)
                Blocks.Add(reader.ReadObject<Block>((int)Header.Version));

            DataAddress = reader.BaseStream.Position;

            tagIndex = parentModule?.tagIndex ?? new TagIndex(Items);
            linkedModules = parentModule?.linkedModules ?? new List<Module>(Enumerable.Repeat(this, 1));
        }

        public DependencyReader CreateReader()
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            return CreateReader(fs);
        }

        internal DependencyReader CreateReader(Stream stream)
        {
            var reader = new DependencyReader(stream, ByteOrder.LittleEndian);

            //verify header when reading a module file
            if (stream is FileStream)
            {
                var header = reader.PeekInt32();

                if (header != ModuleHeader)
                    throw Exceptions.NotAValidMapFile(FileName);
            }

            reader.RegisterInstance(this);

            return reader;
        }

        public void AddLinkedModule(string fileName)
        {
            if (!linkedModules.Any(m => m.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                var module = new Module(fileName, this);
                linkedModules.Add(module);
                tagIndex.ImportTags(module.Items);
            }
        }

        public IEnumerable<ModuleItem> FindAlternateTagInstances(int globalTagId)
        {
            return tagIndex.InstancesById.GetValueOrDefault(globalTagId);
        }

        public IEnumerable<TagClass> GetTagClasses() => tagIndex.Classes.Values;

        public ModuleItem GetItemById(int id) => tagIndex.ItemsById.GetValueOrDefault(id);

        public IEnumerable<ModuleItem> GetItemsByClass(string classCode)
        {
            return tagIndex.ItemsByClass.ContainsKey(classCode)
                ? tagIndex.ItemsByClass[classCode]
                : Enumerable.Empty<ModuleItem>();
        }

        private class TagIndex
        {
            public Dictionary<string, TagClass> Classes { get; }
            public Dictionary<int, ModuleItem> ItemsById { get; }
            public Dictionary<string, List<ModuleItem>> ItemsByClass { get; }
            public Dictionary<int, List<ModuleItem>> InstancesById { get; }

            public TagIndex(IEnumerable<ModuleItem> items)
            {
                Classes = new Dictionary<string, TagClass>();
                ItemsById = new Dictionary<int, ModuleItem>();
                ItemsByClass = new Dictionary<string, List<ModuleItem>>();
                InstancesById = new Dictionary<int, List<ModuleItem>>();

                ImportTags(items);
            }

            public void ImportTags(IEnumerable<ModuleItem> items)
            {
                foreach (var item in items.Where(i => i.GlobalTagId != -1))
                {
                    if (!Classes.ContainsKey(item.ClassCode))
                    {
                        Classes.Add(item.ClassCode, new TagClass(item.ClassCode, item.ClassName));
                        ItemsByClass.Add(item.ClassCode, new List<ModuleItem>());
                    }

                    if (!ItemsById.ContainsKey(item.GlobalTagId))
                    {
                        ItemsById.Add(item.GlobalTagId, item);
                        ItemsByClass[item.ClassCode].Add(item);
                        InstancesById.Add(item.GlobalTagId, new List<ModuleItem> { item });
                    }

                    if (!InstancesById[item.GlobalTagId].Any(i => i.Module.FileName == item.Module.FileName))
                        InstancesById[item.GlobalTagId].Add(item);
                }
            }
        }
    }

    [FixedSize(48, MaxVersion = (int)ModuleType.Halo5Forge)]
    [FixedSize(56, MinVersion = (int)ModuleType.Halo5Forge)]
    public class ModuleHeader
    {
        [Offset(0)]
        public int Head { get; set; }

        [Offset(4)]
        [VersionNumber]
        public ModuleType Version { get; set; }

        [Offset(8)]
        public long ModuleId { get; set; }

        [Offset(16)]
        public int ItemCount { get; set; }

        [Offset(20)]
        public int ManifestCount { get; set; }

        [Offset(24)]
        public int ResourceIndex { get; set; }

        [Offset(28)]
        public int StringsSize { get; set; }

        [Offset(32)]
        public int ResourceCount { get; set; }

        [Offset(36)]
        public int BlockCount { get; set; }
    }

    [FixedSize(20, MaxVersion = (int)ModuleType.Halo5Forge)]
    [FixedSize(32, MinVersion = (int)ModuleType.Halo5Forge)]
    public class Block
    {
        [Offset(0, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(8, MinVersion = (int)ModuleType.Halo5Forge)]
        public uint CompressedOffset { get; set; }

        [Offset(4, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(12, MinVersion = (int)ModuleType.Halo5Forge)]
        public uint CompressedSize { get; set; }

        [Offset(8, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(16, MinVersion = (int)ModuleType.Halo5Forge)]
        public uint UncompressedOffset { get; set; }

        [Offset(12, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(20, MinVersion = (int)ModuleType.Halo5Forge)]
        public uint UncompressedSize { get; set; }

        [Offset(16, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(24, MinVersion = (int)ModuleType.Halo5Forge)]
        public int Compressed { get; set; }
    }

    public class TagClass
    {
        public string ClassCode { get; }
        public string ClassName { get; }

        public TagClass(string code, string name)
        {
            ClassCode = code;
            ClassName = name;
        }
    }
}
