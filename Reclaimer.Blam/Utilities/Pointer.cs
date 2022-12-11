using Reclaimer.IO;
using System.Globalization;

namespace Reclaimer.Blam.Utilities
{
    public readonly record struct Pointer : IWriteable
    {
        private readonly IAddressTranslator translator;
        private readonly IPointerExpander expander;

        public int Value { get; }

        public Pointer(int value, IAddressTranslator translator)
            : this(value, translator, null)
        { }

        public Pointer(int value, Pointer copyFrom)
            : this(value, copyFrom.translator, copyFrom.expander)
        { }

        public Pointer(int pointer, IAddressTranslator translator, IPointerExpander expander)
        {
            Value = pointer;
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.expander = expander;
        }

        public Pointer(DependencyReader reader, IAddressTranslator translator)
            : this(reader, translator, null)
        { }

        public Pointer(DependencyReader reader, IAddressTranslator translator, IPointerExpander expander)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Value = reader.ReadInt32();
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.expander = expander;
        }

        public long Address => translator?.GetAddress(expander?.Expand(Value) ?? Value) ?? default;

        public void Write(EndianWriter writer, double? version) => writer.Write(Value);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        public static implicit operator long(Pointer value) => value.Address;
    }
}
