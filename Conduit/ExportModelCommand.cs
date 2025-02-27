using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Saber3D.Common;
using System.CommandLine;
using static Reclaimer.Plugins.BatchExtractPlugin;

using BlamContentFactory = Reclaimer.Blam.Common.ContentFactory;
using SaberContentFactory = Reclaimer.Saber3D.Common.ContentFactory;

namespace Conduit
{
    internal static class ExportModelCommand
    {
        public static Command Build()
        {
            var cmd = new Command("model");

            var fileArg = new Argument<FileInfo>("file").ExistingOnly().AllowedExtensions(Common.SupportedFileExtensions);
            var tagNameArg = new Argument<string>("tagNameOrFilter");
            var outputDirArg = new Argument<DirectoryInfo>("outputDir").ExistingOnly();

            var folderModeOption = new Option<FolderMode?>("--folder-mode").FromEnumValues();
            folderModeOption.AddAlias("-fm");

            var modelFormatOption = new Option<string>("--model-format").FromAmongModelFormats();
            modelFormatOption.AddAlias("-mf");

            cmd.AddArgument(fileArg);
            cmd.AddArgument(tagNameArg);
            cmd.AddArgument(outputDirArg);
            cmd.AddOption(folderModeOption);
            cmd.AddOption(modelFormatOption);

            cmd.SetHandler(Execute, fileArg, tagNameArg, outputDirArg, folderModeOption, modelFormatOption);

            return cmd;
        }

        public static void Execute(FileInfo file, string filter, DirectoryInfo outputDir, FolderMode? folderMode, string modelFormat)
        {
            ConfigureOutput(folderMode, modelFormat: modelFormat);

            switch (file.Extension.ToLower())
            {
                case ".map":
                case ".yelo":
                    ExecuteCache(file, filter, outputDir);
                    break;
                case ".module":
                    ExecuteModule(file, filter, outputDir);
                    break;
                case ".s3dpak":
                case ".ipak":
                    ExecutePak(file, filter, outputDir);
                    break;
                default:
                    throw new NotSupportedException();
            };
        }

        private static void ExecuteCache(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var cache = CacheFactory.ReadCacheFile(file.FullName);
            var tags = cache.EnumerateTags(filter);

            foreach (var item in tags)
            {
                if (BlamContentFactory.TryGetGeometryContent(item, out var content))
                {
                    Console.WriteLine($"Exporting: {item.TagName}.{item.ClassName}");
                    ExportCommand.TrySaveGeometry(content, outputDir);
                }
            }
        }

        private static void ExecuteModule(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var module = ModuleFactory.ReadModuleFile(file.FullName);
            var tags = module.EnumerateTags(filter);

            foreach (var item in tags)
            {
                if (BlamContentFactory.TryGetGeometryContent(item, out var content))
                {
                    Console.WriteLine($"Exporting: {item.TagName}.{item.ClassName}");
                    ExportCommand.TrySaveGeometry(content, outputDir);
                }
            }
        }

        private static void ExecutePak(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var pak = PakFactory.ReadPakFile(file.FullName);
            var tags = pak.EnumerateTags(filter);

            foreach (var item in tags)
            {
                if (SaberContentFactory.TryGetGeometryContent(item, out var content))
                {
                    Console.WriteLine($"Exporting: {item.Name}");
                    ExportCommand.TrySaveGeometry(content, outputDir);
                }
            }
        }
    }
}
