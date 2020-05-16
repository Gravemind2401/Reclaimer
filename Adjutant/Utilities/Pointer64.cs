using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public struct Pointer64
    {
        private readonly long _value;
        private readonly IAddressTranslator translator;

        public Pointer64(long value, IAddressTranslator translator)
        {
            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            this._value = value;
            this.translator = translator;
        }

        public Pointer64(DependencyReader reader, IAddressTranslator translator)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            this._value = reader.ReadInt64();
            this.translator = translator;
        }

        public long Value => _value;
        public long Address => translator?.GetAddress(_value) ?? default(long);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(Pointer64 Pointer641, Pointer64 Pointer642)
        {
            return Pointer641._value == Pointer642._value;
        }

        public static bool operator !=(Pointer64 Pointer641, Pointer64 Pointer642)
        {
            return !(Pointer641 == Pointer642);
        }

        public static bool Equals(Pointer64 Pointer641, Pointer64 Pointer642)
        {
            return Pointer641._value.Equals(Pointer642._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Pointer64))
                return false;

            return Pointer64.Equals(this, (Pointer64)obj);
        }

        public bool Equals(Pointer64 value)
        {
            return Pointer64.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        public static implicit operator long(Pointer64 Pointer64)
        {
            return Pointer64.Address;
        }
    }
}
