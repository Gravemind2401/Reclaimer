using Reclaimer;
using Reclaimer.Plugins;
using System.Collections;
using System.CommandLine;

namespace Conduit
{
    internal static class ExportCommand
    {
        private delegate Task BatchExtract(IEnumerable items, string baseDir);

        private static BatchExtract BatchExtractFunc;

        public static Command Build()
        {
            BatchExtractFunc = Substrate.GetSharedFunction<BatchExtract>(Constants.SharedFuncBatchExtractEnumerable);

            var cmd = new Command("export");

            cmd.AddCommand(ExportAllCommand.Build());
            cmd.AddCommand(ExportBitmapCommand.Build());
            cmd.AddCommand(ExportModelCommand.Build());

            return cmd;
        }

        public static Task ExtractEnumerableAsync(IEnumerable items, DirectoryInfo baseDir)
        {
            return BatchExtractFunc(items, baseDir.FullName);
        }
    }
}
