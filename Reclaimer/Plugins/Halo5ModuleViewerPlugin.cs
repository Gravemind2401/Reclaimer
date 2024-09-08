using Microsoft.Win32;
using Reclaimer.Blam.Halo5;
using System.IO;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins
{
    public class Halo5ModuleViewerPlugin : Plugin
    {
        private const string Halo5TagKeyRegex = @"Blam\.Halo5\w+";

        public override string Name => "Module Viewer (Halo 5)";

        private PluginContextItem ExtractBinaryContextItem => new PluginContextItem("ExtractBinary", "Extract Tag Binary", OnContextItemClick);

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
                if (Regex.IsMatch(context.FileTypeKey, Halo5TagKeyRegex))
                yield return ExtractBinaryContextItem;
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            var item = context.File.OfType<ModuleItem>().First();

            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = item.FileName,
                Filter = "Binary Files|*.bin",
                FilterIndex = 1,
                AddExtension = true
            };

            if (sfd.ShowDialog() != true)
                return;

            using (var tagReader = item.CreateReader())
            using (var fs = File.Open(sfd.FileName, FileMode.Create))
                tagReader.BaseStream.CopyTo(fs);
        }
    }
}
