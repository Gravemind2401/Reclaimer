using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    [FixedSize(8)]
    public class StringId
    {
        private readonly MetadataHeader meta;

        [Offset(4)]
        public int StringOffset { get; set; }

        public string Value => meta.GetStringByOffset(StringOffset);

        public StringId(MetadataHeader meta)
        {
            if (meta == null)
                throw new ArgumentNullException(nameof(meta));

            this.meta = meta;
        }

        public override string ToString() => Value ?? "[invalid string]";
    }

    [FixedSize(4)]
    public struct StringHash
    {
        private readonly MetadataHeader header;

        [CLSCompliant(false)]
        public uint Hash { get; }

        public string Value => header.GetStringByHash(Hash);

        public StringHash(DependencyReader reader, MetadataHeader header)
        {
            this.header = header;
            Hash = reader.ReadUInt32();
        }

        public static implicit operator string(StringHash stringHash)
        {
            return stringHash.Value;
        }
    }
}
