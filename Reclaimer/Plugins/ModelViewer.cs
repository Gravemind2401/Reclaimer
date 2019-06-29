using Adjutant.Blam.Common;
using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public class ModelViewer : Plugin
    {
        public override string Name => "Model Viewer";

        public override bool CanOpenFile(object file, string key)
        {
            CacheType cacheType;
            if (!Enum.TryParse(key.Split('.').First(), out cacheType))
                return false;

            return file is IIndexItem && (key.EndsWith(".mode") || key.EndsWith(".mod2") || key.EndsWith(".sbsp"));
        }

        public override void OpenFile(object file, string key, IMultiPanelHost targetWindow)
        {
            var item = file as IIndexItem;
            var cacheType = (CacheType)Enum.Parse(typeof(CacheType), key.Split('.').First());
            var container = targetWindow.DocumentContainer;

            LogOutput($"Loading model: {item.FileName}");

            if (item.ClassCode == "sbsp")
            {
                Adjutant.Utilities.IRenderGeometry sbsp;
                switch (cacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo1.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo2Xbox:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo2.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo3.scenario_structure_bsp>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new Controls.ModelViewer();
                viewer.LoadGeometry(sbsp, $"{item.FileName}.{item.ClassCode}");

                container.Items.Add(viewer);
                return;
            }

            if (item.ClassCode == "mod2" || item.ClassCode == "mode")
            {
                Adjutant.Utilities.IRenderGeometry mode;
                switch (cacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo1.gbxmodel>();
                        break;
                    case CacheType.Halo2Xbox:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo2.render_model>();
                        break;
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo3.render_model>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new Controls.ModelViewer();
                viewer.LoadGeometry(mode, $"{item.FileName}.{item.ClassCode}");

                container.Items.Add(viewer);
                return;
            }

            LogOutput($"Loaded model: {item.FileName}");
        }
    }
}
