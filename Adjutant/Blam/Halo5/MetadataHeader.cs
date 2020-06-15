using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    public class MetadataHeader
    {
        private readonly List<string> stringTable;
        private readonly Dictionary<int, int> stringsByOffset;
        private readonly Dictionary<uint, int> stringsByHash;

        public TagHeader Header { get; }

        public List<TagDependency> Dependencies { get; }

        public List<DataBlock> DataBlocks { get; }

        public List<TagStructureDefinition> StructureDefinitions { get; }

        public List<DataBlockReference> DataReferences { get; }

        public List<TagBlockReference> TagReferences { get; }

        public List<StringId> StringIds { get; }

        public MetadataHeader(DependencyReader reader)
        {
            reader.RegisterInstance(this);

            Header = reader.ReadObject<TagHeader>();
            Dependencies = reader.ReadEnumerable<TagDependency>(Header.DependencyCount).ToList();
            DataBlocks = reader.ReadEnumerable<DataBlock>(Header.DataBlockCount).ToList();
            StructureDefinitions = reader.ReadEnumerable<TagStructureDefinition>(Header.TagStructureCount).ToList();
            DataReferences = reader.ReadEnumerable<DataBlockReference>(Header.DataReferenceCount).ToList();
            TagReferences = reader.ReadEnumerable<TagBlockReference>(Header.TagReferenceCount).ToList();
            StringIds = reader.ReadEnumerable<StringId>(Header.StringIdCount).ToList();

            stringTable = new List<string>(StringIds.Count);
            stringsByOffset = new Dictionary<int, int>(StringIds.Count);
            stringsByHash = new Dictionary<uint, int>(StringIds.Count);

            var startPos = reader.BaseStream.Position;
            while (reader.BaseStream.Position < startPos + Header.StringTableSize)
            {
                var relative = reader.BaseStream.Position - startPos;
                var currentValue = reader.ReadNullTerminatedString();
                stringsByOffset.Add((int)relative, stringTable.Count);
                stringsByHash.Add(MurMur3.Hash32(currentValue), stringTable.Count);
                stringTable.Add(currentValue);
            }
        }

        internal string GetStringByOffset(int offset)
        {
            if (stringsByOffset.ContainsKey(offset))
                return stringTable[stringsByOffset[offset]];
            else return null;
        }

        internal string GetStringByHash(uint hash)
        {
            if (stringsByHash.ContainsKey(hash))
                return stringTable[stringsByHash[hash]];
            else return null;
        }
    }
}
