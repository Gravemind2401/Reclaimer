using Reclaimer.Annotations;
using Reclaimer.Drawing;
using Reclaimer.Utilities;
using Reclaimer.Windows;

namespace Reclaimer.Plugins
{
    public class BitmapViewerPlugin : Plugin
    {
        internal override int? FilePriority => 1;

        public override string Name => "Bitmap Viewer";

        public override bool CanOpenFile(OpenFileArgs args) => args.File.Any(i => i is IContentProvider<IBitmap>);

        public override void OpenFile(OpenFileArgs args)
        {
            var bitm = args.File.OfType<IContentProvider<IBitmap>>().FirstOrDefault();
            DisplayBitmap(args.TargetWindow, bitm, args.FileName);
        }

        [SharedFunction]
        public void DisplayBitmap(ITabContentHost targetWindow, IContentProvider<IBitmap> bitmap, string fileName)
        {
            var tabId = $"{Key}::{bitmap.SourceFile}::{bitmap.Id}";
            if (Substrate.ShowTabById(tabId))
                return;

            var container = targetWindow.DocumentPanel;

            LogOutput($"Loading image: {fileName}");

            try
            {
                var viewer = new Controls.BitmapViewer();
                viewer.TabModel.ContentId = tabId;
                viewer.LoadImage(bitmap, fileName);

                container.AddItem(viewer.TabModel);

                LogOutput($"Loaded image: {fileName}");
            }
            catch (Exception e)
            {
                LogError($"Error loading image: {fileName}", e, true);
            }
        }
    }
}
