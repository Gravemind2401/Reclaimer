using Reclaimer.IO;

namespace Reclaimer.Blam.Common.Gen5
{
    [FixedSize(8)]
    public class StringIdGen5
    {
        private readonly IMetadataHeader meta;

        [Offset(4)]
        public int StringOffset { get; set; }

        public string Value => meta.GetStringByOffset(StringOffset);

        public StringIdGen5(IMetadataHeader meta)
        {
            this.meta = meta ?? throw new ArgumentNullException(nameof(meta));
        }

        public override string ToString() => Value ?? "[invalid string]";
    }

    [FixedSize(4)]
    public readonly struct StringHashGen5
    {
        private readonly IMetadataHeader header;

        public uint Hash { get; }

        public string Value => header.GetStringByHash(Hash);

        public StringHashGen5(EndianReader reader, IMetadataHeader header)
        {
            this.header = header;
            Hash = reader.ReadUInt32();
        }

        public static implicit operator string(StringHashGen5 value) => value.Value;

        public override string ToString() => Value ?? "[invalid string]";
    }
}
