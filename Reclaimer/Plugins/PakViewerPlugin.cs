using Reclaimer.Controls.Editors;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class PakViewerPlugin : Plugin
    {
        private const string OpenKey = "PakViewer.OpenPak";
        private const string OpenPath = "File\\Open Pak";
        private const string BrowseFileFilter = "Saber Pak Files|*.s3dpak";
        private const string PakFileExtension = "s3dpak";

        internal static PakViewerSettings Settings;

        public override string Name => "Pak Viewer";

        public override void Initialise() => Settings = LoadSettings<PakViewerSettings>();
        public override void Suspend() => SaveSettings(Settings);

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenKey, OpenPath, OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            if (key != OpenKey)
                return;

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = BrowseFileFilter,
                Multiselect = true,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(Settings.PakFolder))
                ofd.InitialDirectory = Settings.PakFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                OpenPhysicalFile(fileName);
        }

        public override bool SupportsFileExtension(string extension) => extension.ToLower() == PakFileExtension;

        public override void OpenPhysicalFile(string fileName)
        {
            var tabId = $"{Key}::{fileName}";
            if (Substrate.ShowTabById(tabId))
                return;

            LogOutput($"Loading pak file: {fileName}");

            try
            {
                var pv = new Controls.PakViewer();
                pv.TabModel.ContentId = tabId;
                pv.LoadPak(fileName);
                Substrate.AddTool(pv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                Substrate.AddRecentFile(fileName);

                LogOutput($"Loaded pak file: {fileName}");
            }
            catch (Exception ex)
            {
                var message = $"Unable to load {Path.GetFileName(fileName)}:{Environment.NewLine}{ex.Message}";
                Substrate.ShowErrorMessage(message);
                Substrate.LogError(message, ex);
            }
        }
    }

    internal sealed class PakViewerSettings
    {
        [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
        [DisplayName("s3dpak Folder")]
        public string PakFolder { get; set; }
    }
}
