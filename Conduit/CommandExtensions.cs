using System.CommandLine;
using System.CommandLine.Parsing;

namespace Conduit
{
    internal static class CommandExtensions
    {
        public static Argument<FileInfo> AllowedExtensions(this Argument<FileInfo> argument, params string[] extensions)
        {
            argument.AddValidator(FileExtension(extensions));
            return argument;

            static ValidateSymbolResult<ArgumentResult> FileExtension(params string[] extensions)
            {
                return Validate;

                void Validate(ArgumentResult result)
                {
                    for (var i = 0; i < result.Tokens.Count; i++)
                    {
                        var token = result.Tokens[i];
                        var ext = Path.GetExtension(token.Value);

                        if (!extensions.Any(e => e.TrimStart('.').Equals(ext.TrimStart('.'), StringComparison.OrdinalIgnoreCase)))
                        {
                            result.ErrorMessage = $"Unsupported file extension. Supported file extensions are: {string.Join(", ", extensions)}.";
                            return;
                        }
                    }
                }
            }
        }

        public static Option<TEnum> FromEnumValues<TEnum>(this Option<TEnum> option)
            where TEnum : struct, Enum
        {
            option.AddValidator(ValidateEnumResult<TEnum>());
            return option;
        }

        public static Option<TEnum?> FromEnumValues<TEnum>(this Option<TEnum?> option)
            where TEnum : struct, Enum
        {
            option.AddValidator(ValidateEnumResult<TEnum>());
            return option;
        }

        private static ValidateSymbolResult<OptionResult> ValidateEnumResult<TEnum>()
            where TEnum : struct, Enum
        {
            //FromAmongValues doesnt work well for enums because it is case-sensitive

            return Validate;

            static void Validate(OptionResult result)
            {
                for (var i = 0; i < result.Tokens.Count; i++)
                {
                    var token = result.Tokens[i];

                    if (!Enum.TryParse<TEnum>(token.Value, true, out _))
                    {
                        result.ErrorMessage = $"Argument '{token.Value}' not recognized. Must be one of:"
                            + Environment.NewLine
                            + string.Join(Environment.NewLine, Enum.GetValues<TEnum>().Select(e => $"        '{e}'"));
                        return;
                    }
                }
            }
        }
    }
}
