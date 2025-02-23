using System.CommandLine;

namespace Conduit
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var root = new RootCommand();
            root.AddCommand(TagListCommand.Build());
            root.Invoke(args);
        }
    }
}
