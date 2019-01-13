using Adjutant.IO;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    [FixedSize(8)]
    public class BlockCollection<T> : Collection<T>, IBlockCollection<T>
    {
        public Pointer Pointer { get; set; }

        public BlockCollection(DependencyReader reader, IAddressTranslator translator)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            var count = reader.ReadInt32();
            Pointer = new Pointer(reader.ReadInt32(), translator);

            if (count == 0)
                return;

            reader.BaseStream.Position = Pointer.Address;
            for (int i = 0; i < count; i++)
                Add((T)reader.ReadObject(typeof(T)));
        }
    }
}