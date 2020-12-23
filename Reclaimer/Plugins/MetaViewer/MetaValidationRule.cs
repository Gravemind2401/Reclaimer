using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reclaimer.Plugins.MetaViewer
{
    public class MetaValidationRule : ValidationRule
    {
        public static bool Validate(FieldDefinition field, object value)
        {
            var str = value?.ToString();
            switch (field.ValueType)
            {
                case MetaValueType.SByte:
                    return ValidateSByte(str);

                case MetaValueType.Int16:
                    return ValidateInt16(str);

                case MetaValueType.Int32:
                case MetaValueType.Undefined:
                    return ValidateInt32(str);

                case MetaValueType.Int64:
                    return ValidateInt64(str);

                case MetaValueType.Byte:
                case MetaValueType.Enum8:
                case MetaValueType.Bitmask8:
                    return ValidateByte(str);

                case MetaValueType.UInt16:
                case MetaValueType.Enum16:
                case MetaValueType.Bitmask16:
                    return ValidateUInt16(str);

                case MetaValueType.UInt32:
                case MetaValueType.Enum32:
                case MetaValueType.Bitmask32:
                    return ValidateUInt32(str);

                case MetaValueType.UInt64:
                    return ValidateUInt64(str);

                case MetaValueType.Float32:
                case MetaValueType.Angle:
                    return ValidateSingle(str);

                case MetaValueType.String:
                    return str?.Length <= field.Size;

                default: return true;
            }
        }

        public MetaValidationRule()
            : base(ValidationStep.UpdatedValue, true)
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var binding = value as Binding;
            var expr = value as BindingExpression;

            var meta = (expr?.DataItem ?? binding?.Source) as MetaValueBase;
            var prop = meta.GetType().GetProperty((expr?.ParentBinding ?? binding).Path.Path);

            if (prop == null)
                return ValidationResult.ValidResult;

            var propValue = prop.GetValue(meta);
            return new ValidationResult(Validate(meta.FieldDefinition, propValue), null);
        }

        private static bool ValidateSByte(string value)
        {
            sbyte _;
            return sbyte.TryParse(value, out _);
        }

        private static bool ValidateByte(string value)
        {
            byte _;
            return byte.TryParse(value, out _);
        }

        private static bool ValidateInt16(string value)
        {
            short _;
            return short.TryParse(value, out _);
        }

        private static bool ValidateUInt16(string value)
        {
            ushort _;
            return ushort.TryParse(value, out _);
        }

        private static bool ValidateInt32(string value)
        {
            int _;
            return int.TryParse(value, out _);
        }

        private static bool ValidateUInt32(string value)
        {
            uint _;
            return uint.TryParse(value, out _);
        }

        private static bool ValidateInt64(string value)
        {
            long _;
            return long.TryParse(value, out _);
        }

        private static bool ValidateUInt64(string value)
        {
            ulong _;
            return ulong.TryParse(value, out _);
        }

        private static bool ValidateSingle(string value)
        {
            float _;
            return float.TryParse(value, out _);
        }
    }
}
