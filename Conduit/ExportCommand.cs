using Reclaimer;
using Reclaimer.Drawing;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using System.CommandLine;

namespace Conduit
{
    internal static class ExportCommand
    {
        private delegate bool SaveImage(IContentProvider<IBitmap> provider, string baseDir);

        private static SaveImage SaveImageFunc;

        public static Command Build()
        {
            SaveImageFunc = Substrate.GetSharedFunction<SaveImage>(Constants.SharedFuncSaveImage);

            var cmd = new Command("export");

            cmd.AddCommand(ExportBitmapCommand.Build());

            return cmd;
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
    }
}
