using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public struct Int16N
    {
        private const float multiplier = short.MaxValue;
        private readonly short bits;

        public float Value => bits / multiplier;

        public Int16N(short value)
        {
            bits = value;
        }

        public Int16N(float value)
        {
            bits = (short)(multiplier * Utils.Clamp(value, -1, 1));
        }

        public static explicit operator short(Int16N value) => value.bits;
        public static explicit operator Int16N(short value) => new Int16N(value);
        public static implicit operator Int16N(float value) => new Int16N(value);

        #region Equality Operators

        public static bool operator ==(Int16N value1, Int16N value2) => value1.bits == value2.bits;
        public static bool operator !=(Int16N value1, Int16N value2) => !(value1 == value2);

        public static bool Equals(Int16N value1, Int16N value2) => value1.bits.Equals(value2.bits);
        public override bool Equals(object obj)=> obj is Int16N value && Int16N.Equals(this, value);
        public bool Equals(Int16N value) => Int16N.Equals(this, value);

        public override int GetHashCode() => bits.GetHashCode();

        #endregion
    }
}
