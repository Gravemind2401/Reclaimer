using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Windows;
using Adjutant.Blam.Common;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public class MetaViewerPlugin : Plugin
    {
        private Settings settings;

        public override string Name => "Meta Viewer";

        public override void Initialise()
        {
            settings = LoadSettings<Settings>();
        }

        public override void Suspend()
        {
            SaveSettings(settings);
        }

        public override bool CanOpenFile(object file, string fileTypeKey)
        {
            CacheType cacheType;
            if (!Enum.TryParse(fileTypeKey.Split('.').First(), out cacheType))
                return false;

            var item = file as IIndexItem;
            if (item == null) return false;

            var xml = GetDefinitionPath(item);
            return File.Exists(xml);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var item = args.File as IIndexItem;

            var doc = new XmlDocument();
            doc.Load(GetDefinitionPath(item));

            var container = args.TargetWindow.DocumentContainer;

            var viewer = new Controls.MetaViewer();
            viewer.LoadMetadata(item, doc);

            container.Items.Add(viewer);
        }

        private string GetDefinitionPath(IIndexItem item)
        {
            var xmlName = string.Join("_", item.ClassCode.Split(Path.GetInvalidFileNameChars())).PadRight(4);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", PluginFolder(item.CacheFile.CacheType), $"{xmlName}.xml");
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

        private class Settings
        {
            public bool ShowInvisibles { get; set; }
        }
    }
}
