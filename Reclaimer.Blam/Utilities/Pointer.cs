using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public struct Pointer : IWriteable
    {
        private readonly int pointer;
        private readonly IAddressTranslator translator;
        private readonly IPointerExpander expander;

        public Pointer(int value, IAddressTranslator translator)
            : this(value, translator, null)
        { }

        public Pointer(int value, Pointer copyFrom)
            : this(value, copyFrom.translator, copyFrom.expander)
        { }

        public Pointer(int pointer, IAddressTranslator translator, IPointerExpander expander)
        {
            this.pointer = pointer;
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
            pointer = reader.ReadInt32();
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            this.expander = expander;
        }

        public int Value => pointer;
        public long Address => translator?.GetAddress(expander?.Expand(pointer) ?? pointer) ?? default;

        public void Write(EndianWriter writer, double? version) => writer.Write(Value);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(Pointer value1, Pointer value2) => value1.pointer == value2.pointer;
        public static bool operator !=(Pointer value1, Pointer value2) => !(value1 == value2);

        public static bool Equals(Pointer value1, Pointer value2) => value1.pointer.Equals(value2.pointer);
        public override bool Equals(object obj) => obj is Pointer value && Pointer.Equals(this, value);
        public bool Equals(Pointer value) => Pointer.Equals(this, value);

        public override int GetHashCode() => pointer.GetHashCode();

        #endregion

        public static implicit operator long(Pointer value) => value.Address;
    }
}
