using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Windows;
using Adjutant.Blam.Halo5;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class MetaViewerPlugin : Plugin
    {
        public override string Name => "Halo 5 Meta Viewer";

        public override bool CanOpenFile(OpenFileArgs args)
        {
            var match = Regex.Match(args.FileTypeKey, @"Blam\.(\w+)\.(.*)");
            if (!match.Success) return false;

            ModuleType moduleType;
            if (!Enum.TryParse(match.Groups[1].Value, out moduleType))
                return false;

            var item = args.File.OfType<ModuleItem>().FirstOrDefault();
            if (item == null) return false;

            var xml = GetDefinitionPath(item);
            return File.Exists(xml);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var item = args.File.OfType<ModuleItem>().FirstOrDefault();

            var viewer = new Controls.MetaViewerHalo5();
            viewer.LoadMetadata(item, GetDefinitionPath(item));

            var container = args.TargetWindow.DocumentPanel;
            container.AddItem(viewer.TabModel);
        }

        private string GetDefinitionPath(ModuleItem item)
        {
            var xmlName = string.Join("_", item.ClassCode.Split(Path.GetInvalidFileNameChars())).PadRight(4);
            return Path.Combine(Substrate.PluginsDirectory, "Meta Viewer", PluginFolder(item.Module.ModuleType), $"({xmlName}){item.ClassName}.xml");
        }

        private string PluginFolder(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.Halo5Server:
                case ModuleType.Halo5Forge:
                    return "Halo5";

                default: throw new NotSupportedException();
            }
        }
    }
}
