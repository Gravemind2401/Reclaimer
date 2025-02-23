using System.CommandLine;
using System.CommandLine.Parsing;

namespace Conduit
{
    internal static class Validate
    {
        public static Argument<FileInfo> AllowedExtensions(this Argument<FileInfo> argument, params string[] extensions)
        {
            argument.AddValidator(FileExtension(extensions));
            return argument;
        }

        public static ValidateSymbolResult<ArgumentResult> FileExtension(params string[] extensions)
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
                        result.ErrorMessage = $"Unsupported file extension. Supported file extensions are {string.Join(", ", extensions)}.";
                        return;
                    }
                }
            }
        }
    }
}
