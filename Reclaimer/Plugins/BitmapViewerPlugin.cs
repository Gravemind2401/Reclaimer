using Adjutant.Utilities;
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
            return args.File.Any(i => i is IBitmap);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var bitm = args.File.OfType<IBitmap>().FirstOrDefault();
            var container = args.TargetWindow.DocumentPanel;

            LogOutput($"Loading image: {args.FileName}");

            try
            {
                var viewer = new Controls.BitmapViewer();
                viewer.LoadImage(bitm, args.FileName);

                container.AddItem(viewer.TabModel);

                LogOutput($"Loaded image: {args.FileName}");
            }
            catch (Exception e)
            {
                LogError($"Error loading image: {args.FileName}", e);
            }
        }
    }
}
