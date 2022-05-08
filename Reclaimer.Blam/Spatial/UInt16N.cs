using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public struct UInt16N
    {
        private const float multiplier = ushort.MaxValue;
        private readonly ushort bits;

        public float Value => bits / multiplier;

        public UInt16N(ushort value)
        {
            bits = value;
        }

        public UInt16N(float value)
        {
            bits = (ushort)(multiplier * Utils.Clamp(value, 0, 1));
        }

        public static explicit operator ushort(UInt16N value) => value.bits;
        public static explicit operator UInt16N(ushort value) => new UInt16N(value);
        public static implicit operator UInt16N(float value) => new UInt16N(value);

        #region Equality Operators

        public static bool operator ==(UInt16N value1, UInt16N value2) => value1.bits == value2.bits;
        public static bool operator !=(UInt16N value1, UInt16N value2) => !(value1 == value2);

        public static bool Equals(UInt16N value1, UInt16N value2) => value1.bits.Equals(value2.bits);
        public override bool Equals(object obj)=> obj is UInt16N value && UInt16N.Equals(this, value);
        public bool Equals(UInt16N value) => UInt16N.Equals(this, value);

        public override int GetHashCode() => bits.GetHashCode();

        #endregion
    }
}
