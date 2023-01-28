using Reclaimer.IO;
using System.Globalization;

namespace Reclaimer.Blam.Utilities
{
    public readonly record struct Pointer64 : IWriteable
    {
        private readonly IAddressTranslator translator;

        public long Value { get; }

        public Pointer64(long pointer, IAddressTranslator translator)
        {
            Value = pointer;
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public Pointer64(DependencyReader reader, IAddressTranslator translator)
        {
            ArgumentNullException.ThrowIfNull(reader);

            Value = reader.ReadInt64();
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public long Address => translator?.GetAddress(Value) ?? default;

        public void Write(EndianWriter writer, double? version) => writer.Write(Value);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        public static implicit operator long(Pointer64 value) => value.Address;
    }
}
