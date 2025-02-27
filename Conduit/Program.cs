using Reclaimer;
using System.CommandLine;

namespace Conduit
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            App.InitialiseCommandLine();

            var root = new RootCommand();
            root.AddCommand(TagListCommand.Build());
            root.AddCommand(ExportCommand.Build());
            root.Invoke(args);
        }
    }
}
