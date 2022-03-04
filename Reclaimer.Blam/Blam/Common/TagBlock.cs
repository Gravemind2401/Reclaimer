using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    [FixedSize(12, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(8, MinVersion = (int)CacheType.Halo2Xbox, MaxVersion = (int)CacheType.Halo3Beta)]
    [FixedSize(12, MinVersion = (int)CacheType.Halo3Beta)]
    public class TagBlock : IWriteable
    {
        public int Count { get; }
        public Pointer Pointer { get; }
        public bool IsInvalid { get; }

        public TagBlock(int count, Pointer pointer)
        {
            Count = count;
            Pointer = pointer;

            IsInvalid = Count <= 0 || Pointer.Address < 0;
        }

        public TagBlock(DependencyReader reader, ICacheFile cache, IAddressTranslator translator)
            : this(reader, cache, translator, null)
        { }

        public TagBlock(DependencyReader reader, ICacheFile cache, IAddressTranslator translator, IPointerExpander expander)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            Count = reader.ReadInt32();
            Pointer = new Pointer(reader.ReadInt32(), translator, expander);

            IsInvalid = Count <= 0 || Pointer.Address < 0 || Pointer.Address >= reader.BaseStream.Length;
        }

        public void Write(EndianWriter writer, double? version)
        {
            writer.Write(Count);
            Pointer.Write(writer, version);
        }
    }
}