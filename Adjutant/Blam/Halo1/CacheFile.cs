using Adjutant.Blam.Definitions;
using Adjutant.IO;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class CacheFile : ICacheFile
    {
        public string FileName { get; private set; }
        public string BuildString => Header.BuildString;
        public CacheType Type => Header.CacheType;

        public CacheHeader Header { get; private set; }
        public IndexHeader IndexHeader { get; private set; }
        public List<IndexItem> IndexItems { get; private set; }
        public Dictionary<int, string> TagNames { get; private set; }

        public AddressTranslator AddressTranslator { get; private set; }

        public CacheFile(string fileName)
        {
            FileName = fileName;

            AddressTranslator = new AddressTranslator(this);

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new DependencyReader(fs, ByteOrder.LittleEndian))
            {
                reader.RegisterType<Pointer>(() => new Pointer(reader.ReadInt32(), AddressTranslator));

                Header = reader.ReadObject<CacheHeader>();

                reader.Seek(Header.IndexAddress, SeekOrigin.Begin);
                IndexHeader = reader.ReadObject<IndexHeader>();
                IndexItems = new List<IndexItem>();
                for (int i = 0; i < IndexHeader.TagCount; i++)
                    IndexItems.Add(reader.ReadObject(new IndexItem(this)));

                TagNames = new Dictionary<int, string>();
                foreach (var i in IndexItems)
                {
                    reader.Seek(i.FileNamePointer.Address, SeekOrigin.Begin);
                    TagNames.Add(i.Id, reader.ReadNullTerminatedString());
                }
            }
        }
    }

    public class AddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.IndexHeader.Magic - (cache.Header.IndexAddress + 40);

        public AddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public int GetAddress(int pointer)
        {
            return pointer - Magic;
        }

        public int GetPointer(int address)
        {
            return address + Magic;
        }
    }

    public class CacheHeader
    {
        [Offset(0)]
        public int Head { get; set; }

        [Offset(4)]
        [VersionNumber]
        public int Version { get; set; }

        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public int IndexAddress { get; set; }

        [Offset(64)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }

        public CacheType CacheType
        {
            get
            {
                switch (Version)
                {
                    case 5: return CacheType.Halo1Xbox;
                    case 7: return CacheType.Halo1PC;
                    case 609: return CacheType.Halo1CE;
                    default: return CacheType.Unknown;
                }
            }
        }
    }

    [FixedSize(40)]
    public class IndexHeader
    {
        [Offset(0)]
        public int Magic { get; set; }

        [Offset(12)]
        public int TagCount { get; set; }

        [Offset(16)]
        public int VertexDataCount { get; set; }

        [Offset(20)]
        public int VertexDataOffset { get; set; }

        [Offset(24)]
        public int IndexDataCount { get; set; }

        [Offset(28)]
        public int IndexDataOffset { get; set; }
    }

    [FixedSize(32)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;

        public IndexItem(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        [ByteOrder(ByteOrder.BigEndian)]
        public int ClassId { get; set; }

        //parent class codes here

        [Offset(12)]
        [StoreType(typeof(ushort))]
        public int Id { get; set; }

        [Offset(16)]
        public Pointer FileNamePointer { get; set; }

        [Offset(20)]
        public Pointer MetaPointer { get; set; }

        [Offset(24)]
        public int Unknown1 { get; set; }

        [Offset(28)]
        public int Unknown2 { get; set; }

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId)).TrimEnd();

        public string FileName => cache.TagNames[Id];

        public T ReadMetadata<T>()
        {
            using (var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read))
            using (var reader = new DependencyReader(fs, ByteOrder.LittleEndian))
            {
                reader.RegisterType<IAddressTranslator>(() => cache.AddressTranslator);

                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                return (T)reader.ReadObject(typeof(T), cache.Header.Version);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FileName}");
        }
    }
}
