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
        private readonly int _value;
        private readonly IAddressTranslator translator;
        private readonly IPointerExpander expander;

        public Pointer(int value, IAddressTranslator translator)
            : this(value, translator, null)
        { }

        public Pointer(int value, Pointer copyFrom)
            : this(value, copyFrom.translator, copyFrom.expander)
        { }

        public Pointer(int value, IAddressTranslator translator, IPointerExpander expander)
        {
            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            _value = value;
            this.translator = translator;
            this.expander = expander;
        }

        public Pointer(DependencyReader reader, IAddressTranslator translator)
            : this(reader, translator, null)
        { }

        public Pointer(DependencyReader reader, IAddressTranslator translator, IPointerExpander expander)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            _value = reader.ReadInt32();
            this.translator = translator;
            this.expander = expander;
        }

        public int Value => _value;
        public long Address => translator?.GetAddress(expander?.Expand(_value) ?? _value) ?? default(long);

        public void Write(EndianWriter writer, double? version) => writer.Write(Value);

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(Pointer pointer1, Pointer pointer2)
        {
            return pointer1._value == pointer2._value;
        }

        public static bool operator !=(Pointer pointer1, Pointer pointer2)
        {
            return !(pointer1 == pointer2);
        }

        public static bool Equals(Pointer pointer1, Pointer pointer2)
        {
            return pointer1._value.Equals(pointer2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Pointer))
                return false;

            return Pointer.Equals(this, (Pointer)obj);
        }

        public bool Equals(Pointer value)
        {
            return Pointer.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        public static implicit operator long(Pointer pointer)
        {
            return pointer.Address;
        }
    }
}
