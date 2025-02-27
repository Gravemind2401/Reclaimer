using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Saber3D.Common;
using System.CommandLine;

namespace Conduit
{
    internal static class TagListCommand
    {
        public static Command Build()
        {
            var cmd = new Command("taglist");

            var fileArg = new Argument<FileInfo>("file").ExistingOnly().AllowedExtensions(Common.SupportedFileExtensions);

            var formatOption = new Option<string>("--output-format", "String format for output. Use %i for tag ID, %t for tag name (including path), %n for tag name (without path), %e for tag group (long name), %c for tag group (short name)");
            formatOption.AddAlias("-o");
            formatOption.SetDefaultValue("%t.%e");

            var filterOption = new Option<string>("--filter", "Only list tags matching the filter text. Use * for wildcard");
            filterOption.AddAlias("-f");

            var sortOption = new Option<string>("--sort", "Provide a format string to sort by, if different to the output format (same tokens as --output-format)");
            sortOption.AddAlias("-s");

            var limitOption = new Option<int>("--limit", "Limit the number of tags that will be listed");
            limitOption.AddAlias("-l");

            cmd.AddArgument(fileArg);
            cmd.AddOption(formatOption);
            cmd.AddOption(filterOption);
            cmd.AddOption(sortOption);
            cmd.AddOption(limitOption);

            cmd.SetHandler(Execute, fileArg, filterOption, formatOption, sortOption, limitOption);

            return cmd;
        }

        private static void Execute(FileInfo file, string filter, string format, string sort, int limit)
        {
            switch (file.Extension.ToLower())
            {
                case ".map":
                case ".yelo":
                    ExecuteCache(file, filter, format, sort, limit);
                    break;
                case ".module":
                    ExecuteModule(file, filter, format, sort, limit);
                    break;
                case ".s3dpak":
                case ".ipak":
                    ExecutePak(file, filter, format, sort, limit);
                    break;
                default:
                    throw new NotSupportedException();
            };
        }

        private static void ExecuteCache(FileInfo file, string filter, string format, string sort, int limit)
        {
            if (string.IsNullOrEmpty(sort))
                sort = format;

            var cache = CacheFactory.ReadCacheFile(file.FullName);
            var tags = cache.EnumerateTags(filter, sort, limit);

            foreach (var item in tags)
            {
                var output = item.MakeOutputString(format);
                Console.WriteLine(output);
            }
        }

        private static void ExecuteModule(FileInfo file, string filter, string format, string sort, int limit)
        {
            if (string.IsNullOrEmpty(sort))
                sort = format;

            var module = ModuleFactory.ReadModuleFile(file.FullName);
            var tags = module.EnumerateTags(filter, sort, limit);

            foreach (var item in tags)
            {
                var output = item.MakeOutputString(format);
                Console.WriteLine(output);
            }
        }

        private static void ExecutePak(FileInfo file, string filter, string format, string sort, int limit)
        {
            if (string.IsNullOrEmpty(sort))
                sort = format;

            var pak = PakFactory.ReadPakFile(file.FullName);
            var tags = pak.EnumerateTags(filter, sort, limit);

            foreach (var item in tags)
            {
                var output = item.MakeOutputString(format);
                Console.WriteLine(output);
            }
        }
    }
}
