using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    [FixedSize(12, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(8, MinVersion = (int)CacheType.Halo2Xbox, MaxVersion = (int)CacheType.Halo3Beta)]
    [FixedSize(12, MinVersion = (int)CacheType.Halo3Beta)]
    public class BlockCollection<T> : Collection<T>
    {
        public Pointer Pointer { get; }

        public BlockCollection(DependencyReader reader, ICacheFile cache, IAddressTranslator translator)
            : this(reader, cache, translator, null)
        { }
        
        public BlockCollection(DependencyReader reader, ICacheFile cache, IAddressTranslator translator, IPointerExpander expander)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            var count = reader.ReadInt32();
            Pointer = new Pointer(reader.ReadInt32(), translator, expander);

            if (count == 0)
                return;

            reader.BaseStream.Position = Pointer.Address;
            for (int i = 0; i < count; i++)
                Add((T)reader.ReadObject(typeof(T), (int)cache.CacheType));
        }
    }
}