using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Windows;
using Adjutant.Blam.Common;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MetaViewer
{
    public class MetaViewerPlugin : Plugin
    {
        internal static MetaViewerSettings Settings { get; private set; }

        public override string Name => "Meta Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<MetaViewerSettings>();
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override bool CanOpenFile(OpenFileArgs args)
        {
            var match = Regex.Match(args.FileTypeKey, @"Blam\.(\w+)\.(.*)");
            if (!match.Success) return false;

            CacheType cacheType;
            if (!Enum.TryParse(match.Groups[1].Value, out cacheType))
                return false;

            var item = args.File.OfType<IIndexItem>().FirstOrDefault();
            if (item == null) return false;

            var xml = GetDefinitionPath(item);
            return File.Exists(xml);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var item = args.File.OfType<IIndexItem>().FirstOrDefault();

            var viewer = new Controls.MetaViewer();
            viewer.LoadMetadata(item, GetDefinitionPath(item));

            var container = args.TargetWindow.DocumentPanel;
            container.AddItem(viewer.TabModel);
        }

        private string GetDefinitionPath(IIndexItem item)
        {
            var xmlName = string.Join("_", item.ClassCode.Split(Path.GetInvalidFileNameChars())).PadRight(4);
            return Path.Combine(Substrate.PluginsDirectory, "Meta Viewer", PluginFolder(item.CacheFile.CacheType), $"{xmlName}.xml");
        }

        private string PluginFolder(CacheType cacheType)
        {
            switch (cacheType)
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                case CacheType.Halo1AE:
                    return "Halo1";

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    return "Halo2";

                case CacheType.Halo3Beta:
                    return "Halo3Beta";

                case CacheType.Halo3Retail:
                    return "Halo3";

                case CacheType.Halo3ODST:
                    return "ODST";

                case CacheType.HaloReachBeta:
                    return "ReachBeta";

                case CacheType.HaloReachRetail:
                    return "Reach";

                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
                    return "Halo4";

                default: throw new NotSupportedException();
            }
        }

        internal class MetaViewerSettings
        {
            public bool ShowInvisibles { get; set; }
        }
    }
}
