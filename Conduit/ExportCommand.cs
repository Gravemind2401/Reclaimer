using Reclaimer;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using System.CommandLine;

namespace Conduit
{
    internal static class ExportCommand
    {
        private delegate bool SaveImage(IContentProvider<IBitmap> provider, string baseDir);
        private delegate bool SaveModel(IContentProvider<Scene> provider, string baseDir);

        private static SaveImage SaveImageFunc;
        private static SaveModel SaveModelFunc;

        public static Command Build()
        {
            SaveImageFunc = Substrate.GetSharedFunction<SaveImage>(Constants.SharedFuncBatchSaveImage);
            SaveModelFunc = Substrate.GetSharedFunction<SaveModel>(Constants.SharedFuncBatchSaveModel);

            var cmd = new Command("export");

            cmd.AddCommand(ExportAllCommand.Build());
            cmd.AddCommand(ExportBitmapCommand.Build());
            cmd.AddCommand(ExportModelCommand.Build());

            return cmd;
        }

        public static void TrySavePrimary(object content, DirectoryInfo baseDir)
        {
            try
            {
                if (content is IContentProvider<IBitmap> bitmapProvider)
                    SaveImageFunc(bitmapProvider, baseDir.FullName);
                else if (content is IContentProvider<Scene> geometryProvider)
                    SaveModelFunc(geometryProvider, baseDir.FullName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void TrySaveImage(IContentProvider<IBitmap> content, DirectoryInfo baseDir)
        {
            try
            {
                SaveImageFunc(content, baseDir.FullName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public static void TrySaveGeometry(IContentProvider<Scene> content, DirectoryInfo baseDir)
        {
            try
            {
                SaveModelFunc(content, baseDir.FullName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
