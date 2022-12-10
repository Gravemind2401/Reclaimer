using Reclaimer.IO;
using System;

namespace Reclaimer.Blam.Halo5
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
            this.meta = meta ?? throw new ArgumentNullException(nameof(meta));
        }

        public override string ToString() => Value ?? "[invalid string]";
    }

    [FixedSize(4)]
    public readonly struct StringHash
    {
        private readonly MetadataHeader header;

        public uint Hash { get; }

        public string Value => header.GetStringByHash(Hash);

        public StringHash(EndianReader reader, MetadataHeader header)
        {
            this.header = header;
            Hash = reader.ReadUInt32();
        }

        public static implicit operator string(StringHash value) => value.Value;

        public override string ToString() => Value ?? "[invalid string]";
    }
}
