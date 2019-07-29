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
    public class BitmapViewerPlugin : Plugin
    {
        public override string Name => "Bitmap Viewer";

        public override bool CanOpenFile(OpenFileArgs args)
        {
            CacheType cacheType;
            if (!Enum.TryParse(args.FileTypeKey.Split('.').First(), out cacheType))
                return false;

            return args.File is IIndexItem && args.FileTypeKey.EndsWith(".bitm");
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var item = args.File as IIndexItem;
            var container = args.TargetWindow.DocumentContainer;

            LogOutput($"Loading image: {item.FullPath}");

            try
            {
                if (item.ClassCode == "bitm")
                {
                    Adjutant.Utilities.IBitmap bitm;
                    switch (item.CacheFile.CacheType)
                    {
                        case CacheType.Halo1CE:
                        case CacheType.Halo1PC:
                            bitm = item.ReadMetadata<Adjutant.Blam.Halo1.bitmap>();
                            break;
                        case CacheType.Halo2Xbox:
                            bitm = item.ReadMetadata<Adjutant.Blam.Halo2.bitmap>();
                            break;
                        case CacheType.Halo3Beta:
                        case CacheType.Halo3Retail:
                        case CacheType.Halo3ODST:
                            bitm = item.ReadMetadata<Adjutant.Blam.Halo3.bitmap>();
                            break;
                        default: throw Exceptions.TagClassNotSupported(item);
                    }

                    var viewer = new Controls.BitmapViewer();
                    viewer.LoadImage(bitm, $"{item.FullPath}.{item.ClassCode}");

                    container.Items.Add(viewer);
                }
                else throw Exceptions.TagClassNotSupported(item);

                LogOutput($"Loaded image: {item.FullPath}");
            }
            catch (Exception e)
            {
                LogError($"Error loading image: {item.FullPath}", e);
            }
        }
    }
}
