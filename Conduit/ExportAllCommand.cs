using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Saber3D.Common;
using System.CommandLine;
using static Reclaimer.Plugins.BatchExtractPlugin;

namespace Conduit
{
    internal static class ExportAllCommand
    {
        public static Command Build()
        {
            var cmd = new Command("all");

            var fileArg = new Argument<FileInfo>("file").ExistingOnly().AllowedExtensions(Common.SupportedFileExtensions);
            var tagNameArg = new Argument<string>("tagNameOrFilter");
            var outputDirArg = new Argument<DirectoryInfo>("outputDir").ExistingOnly();

            var overwriteOption = new Option<bool?>("--overwrite");
            overwriteOption.AddAlias("-ow");

            var folderModeOption = new Option<FolderMode?>("--folder-mode").FromEnumValues();
            folderModeOption.AddAlias("-fm");

            var bitmapFormatOption = new Option<BitmapFormat?>("--bitmap-format").FromEnumValues();
            bitmapFormatOption.AddAlias("-bf");

            var bitmapModeOption = new Option<BitmapMode?>("--bitmap-mode").FromEnumValues();
            bitmapModeOption.AddAlias("-bm");

            var modelFormatOption = new Option<string>("--model-format").FromAmongModelFormats();
            modelFormatOption.AddAlias("-mf");

            cmd.AddArgument(fileArg);
            cmd.AddArgument(tagNameArg);
            cmd.AddArgument(outputDirArg);
            cmd.AddOption(overwriteOption);
            cmd.AddOption(folderModeOption);
            cmd.AddOption(bitmapFormatOption);
            cmd.AddOption(bitmapModeOption);
            cmd.AddOption(modelFormatOption);

            cmd.SetHandler(ExecuteAsync, fileArg, tagNameArg, outputDirArg, overwriteOption, folderModeOption, bitmapFormatOption, bitmapModeOption, modelFormatOption);

            return cmd;
        }

        public static Task ExecuteAsync(FileInfo file, string filter, DirectoryInfo outputDir, bool? overwrite, FolderMode? folderMode, BitmapFormat? bitmapFormat, BitmapMode? bitmapMode, string modelFormat)
        {
            ConfigureOutput(overwrite, folderMode, bitmapMode, bitmapFormat, modelFormat);

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
            var tags = cache.EnumerateTags(filter);

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }

        private static Task ExecuteModule(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var module = ModuleFactory.ReadModuleFile(file.FullName);
            var tags = module.EnumerateTags(filter);

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }

        private static Task ExecutePak(FileInfo file, string filter, DirectoryInfo outputDir)
        {
            var pak = PakFactory.ReadPakFile(file.FullName);
            var tags = pak.EnumerateTags(filter);

            return ExportCommand.ExtractEnumerableAsync(tags, outputDir);
        }
    }
}
