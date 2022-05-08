using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public struct Pointer64 : IWriteable
    {
        private readonly long pointer;
        private readonly IAddressTranslator translator;

        public Pointer64(long pointer, IAddressTranslator translator)
        {
            this.pointer = pointer;
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public Pointer64(DependencyReader reader, IAddressTranslator translator)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            pointer = reader.ReadInt64();
            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public long Value => pointer;
        public long Address => translator?.GetAddress(pointer) ?? default;

        public void Write(EndianWriter writer, double? version) => writer.Write(Value);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(Pointer64 value1, Pointer64 value2) => value1.pointer == value2.pointer;
        public static bool operator !=(Pointer64 value1, Pointer64 value2) => !(value1 == value2);

        public static bool Equals(Pointer64 value1, Pointer64 value2) => value1.pointer.Equals(value2.pointer);
        public override bool Equals(object obj) => obj is Pointer64 value && Pointer64.Equals(this, value);
        public bool Equals(Pointer64 value) => Pointer64.Equals(this, value);

        public override int GetHashCode() => pointer.GetHashCode();

        #endregion

        public static implicit operator long(Pointer64 value) => value.Address;
    }
}
