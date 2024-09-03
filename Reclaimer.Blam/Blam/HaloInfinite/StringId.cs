using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    [FixedSize(4)]
    public readonly struct StringHash
    {
        private readonly MetadataHeader header;

        public uint Hash { get; }

        public string Value => Hash.ToString();

        public StringHash(EndianReader reader, MetadataHeader header)
        {
            this.header = header;
            Hash = reader.ReadUInt32();
        }

        public static implicit operator string(StringHash value) => value.Value;

        public override string ToString() => Value ?? "[invalid string]";
    }
}
