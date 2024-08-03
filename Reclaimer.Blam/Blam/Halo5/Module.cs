using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

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
            reader.RegisterType(reader.ReadMatrix3x4);

            return reader;
        }

        /// <summary>
        /// Merges the tag list from the target module into the current module instance, allowing cross-module tag references to be resolved.
        /// </summary>
        /// <param name="fileName">The file path of the target module.</param>
        public void AddLinkedModule(string fileName)
        {
            if (!linkedModules.Any(m => m.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
            {
                var module = new Module(fileName, this);
                linkedModules.Add(module);
                tagIndex.ImportTags(module.Items);
            }
        }

        /// <summary>
        /// Returns all tags matching the specified <paramref name="globalTagId"/> across this module and all linked modules.
        /// </summary>
        /// <param name="globalTagId">The global identifier used to identify the same tag across different modules.</param>
        public IEnumerable<ModuleItem> FindAlternateTagInstances(int globalTagId)
        {
            return tagIndex.ItemsById.GetValueOrDefault(globalTagId) ?? Enumerable.Empty<ModuleItem>();
        }

        /// <summary>
        /// Returns all unique tag classes used by any tags in this module or any linked module.
        /// </summary>
        public IEnumerable<TagClass> GetTagClasses() => tagIndex.Classes.Values;

        /// <summary>
        /// Finds a tag contained either in this module or in a linked module.
        /// </summary>
        /// <remarks>
        /// If the tag is contained in multiple modules, the value returned will come from the first linked module that contained the matching tag.
        /// </remarks>
        /// <param name="globalTagId">The global identifier used to identify the same tag across different modules.</param>
        public ModuleItem GetItemById(int globalTagId) => tagIndex.ItemsById.GetValueOrDefault(globalTagId)?[0];

        /// <summary>
        /// Returns all tags matching the specified tag class across this module and all linked modules.
        /// </summary>
        /// <param name="classCode">The <see cref="TagClass.ClassCode"/> value to match on.</param>
        public IEnumerable<ModuleItem> GetItemsByClass(string classCode)
        {
            return tagIndex.ItemsByClass.TryGetValue(classCode, out var classItems)
                ? classItems.Select(g => g[0])
                : Enumerable.Empty<ModuleItem>();
        }

        /// <summary>
        /// Returns all unique tag instances from this module and all linked modules.
        /// </summary>
        /// <remarks>
        /// When the same tag is contained in multiple modules, only first copy of each tag will be returned, based on the module file name descending.
        /// </remarks>
        public IEnumerable<ModuleItem> GetLinkedItems() => tagIndex.ItemsById.Values.Select(g => g[0]);

        private class TagIndex
        {
            public Dictionary<string, TagClass> Classes { get; }
            public Dictionary<int, ModuleItemGroup> ItemsById { get; }
            public Dictionary<string, List<ModuleItemGroup>> ItemsByClass { get; }

            public TagIndex(IEnumerable<ModuleItem> items)
            {
                Classes = new Dictionary<string, TagClass>();
                ItemsById = new Dictionary<int, ModuleItemGroup>();
                ItemsByClass = new Dictionary<string, List<ModuleItemGroup>>();

                ImportTags(items);
            }

            public void ImportTags(IEnumerable<ModuleItem> items)
            {
                foreach (var item in items.Where(i => i.GlobalTagId != -1))
                {
                    if (!Classes.ContainsKey(item.ClassCode))
                    {
                        Classes.Add(item.ClassCode, new TagClass(item.ClassCode, item.ClassName));
                        ItemsByClass.Add(item.ClassCode, new List<ModuleItemGroup>());
                    }

                    if (ItemsById.TryGetValue(item.GlobalTagId, out var group))
                        group.Add(item);
                    else
                    {
                        group = new ModuleItemGroup(item);
                        ItemsById.Add(item.GlobalTagId, group);
                        ItemsByClass[item.ClassCode].Add(group);
                    }
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
