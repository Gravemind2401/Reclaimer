using Adjutant.Blam.Common;
using Reclaimer.Utilities;
using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public class ModelViewerPlugin : Plugin
    {
        public override string Name => "Model Viewer";

        public override bool CanOpenFile(OpenFileArgs args)
        {
            CacheType cacheType;
            if (!Enum.TryParse(args.FileTypeKey.Split('.').First(), out cacheType))
                return false;

            return args.File is IIndexItem && (args.FileTypeKey.EndsWith(".mode") || args.FileTypeKey.EndsWith(".mod2") || args.FileTypeKey.EndsWith(".sbsp"));
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var item = args.File as IIndexItem;
            var container = args.TargetWindow.DocumentContainer;

            LogOutput($"Loading model: {item.FullPath}");

            try
            {
                if (item.ClassCode == "sbsp")
                {
                    Adjutant.Utilities.IRenderGeometry sbsp;
                    switch (item.CacheFile.CacheType)
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
                        case CacheType.HaloReachBeta:
                        case CacheType.HaloReachRetail:
                            sbsp = item.ReadMetadata<Adjutant.Blam.HaloReach.scenario_structure_bsp>();
                            break;
                        default: throw Exceptions.TagClassNotSupported(item);
                    }

                    var viewer = new Controls.ModelViewer();
                    viewer.LoadGeometry(sbsp, $"{item.FullPath}.{item.ClassCode}");

                    container.Items.Add(viewer);
                }
                else if (item.ClassCode == "mod2" || item.ClassCode == "mode")
                {
                    Adjutant.Utilities.IRenderGeometry mode;
                    switch (item.CacheFile.CacheType)
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
                        case CacheType.HaloReachBeta:
                        case CacheType.HaloReachRetail:
                            mode = item.ReadMetadata<Adjutant.Blam.HaloReach.render_model>();
                            break;
                        default: throw Exceptions.TagClassNotSupported(item);
                    }

                    var viewer = new Controls.ModelViewer();
                    viewer.LoadGeometry(mode, $"{item.FullPath}.{item.ClassCode}");

                    container.Items.Add(viewer);
                }
                else throw Exceptions.TagClassNotSupported(item);

                LogOutput($"Loaded model: {item.FullPath}");
            }
            catch (Exception e)
            {
                LogError($"Error loading image: {item.FullPath}", e);
            }
        }
    }
}
