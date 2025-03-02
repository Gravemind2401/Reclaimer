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

            cmd.SetHandler(ExecuteAsync, fileArg, tagNameArg, outputDirArg, folderModeOption, modelFormatOption);

            return cmd;
        }

        public static Task ExecuteAsync(FileInfo file, string filter, DirectoryInfo outputDir, FolderMode? folderMode, string modelFormat)
        {
            ConfigureOutput(folderMode, modelFormat: modelFormat);

            return file.Extension.ToLower() switch
            {
                ".map" or ".yelo" => ExecuteCache(file, filter, outputDir),
                ".module" => ExecuteModule(file, filter, outputDir),
                ".s3dpak" or ".ipak" => ExecutePak(file, filter, outputDir),
                _ => throw new NotSupportedException()
            };
        }

        private static Task ExecuteCache(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var cache = CacheFactory.ReadCacheFile(file.FullName);
            var tags = cache.EnumerateTags(filter).Where(t => BlamContentFactory.TryGetGeometryContent(t, out _));

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }

        private static Task ExecuteModule(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var module = ModuleFactory.ReadModuleFile(file.FullName);
            var tags = module.EnumerateTags(filter).Where(t => BlamContentFactory.TryGetGeometryContent(t, out _));

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }

        private static Task ExecutePak(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var pak = PakFactory.ReadPakFile(file.FullName);
            var tags = pak.EnumerateTags(filter).Where(t => SaberContentFactory.TryGetGeometryContent(t, out _));

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }
    }
}
