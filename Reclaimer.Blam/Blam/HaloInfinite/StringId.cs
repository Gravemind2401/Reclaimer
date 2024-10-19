using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    [FixedSize(4)]
    public readonly struct StringHash
    {
        private readonly MetadataHeader header;

        public int Hash { get; }

        public string Value => StringMapper.Instance.StringMappings.TryGetValue(Hash, out var value)
                               ? value : Hash.ToString();


        public StringHash(EndianReader reader, MetadataHeader header)
        {
            this.header = header;
            Hash = reader.ReadInt32();
        }

        public static implicit operator string(StringHash value) => value.Value;

        public override string ToString() => Value ?? "[invalid string]";
    }
}
