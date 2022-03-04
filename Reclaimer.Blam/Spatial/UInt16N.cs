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

        public static explicit operator ushort(UInt16N value)
        {
            return value.bits;
        }

        public static explicit operator UInt16N(ushort value)
        {
            return new UInt16N(value);
        }

        public static implicit operator UInt16N(float value)
        {
            return new UInt16N(value);
        }

        #region Equality Operators

        public static bool operator ==(UInt16N value1, UInt16N value2)
        {
            return value1.bits == value2.bits;
        }

        public static bool operator !=(UInt16N value1, UInt16N value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(UInt16N value1, UInt16N value2)
        {
            return value1.bits.Equals(value2.bits);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UInt16N))
                return false;

            return UInt16N.Equals(this, (UInt16N)obj);
        }

        public bool Equals(UInt16N value)
        {
            return UInt16N.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
        }

        #endregion
    }
}
