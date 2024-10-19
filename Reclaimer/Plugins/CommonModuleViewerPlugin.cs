using Microsoft.Win32;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.HaloInfinite.Oodle;
using Reclaimer.Blam.Utilities;
using Reclaimer.Controls.Editors;
using Reclaimer.IO;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class CommonModuleViewerPlugin : Plugin
    {
        private const string OpenKey = "CommonModuleViewer.OpenModule";
        private const string OpenPath = "File\\Open Module";
        private const string BrowseFileFilter = "Halo Module Files|*.module";
        private const string ModuleFileExtension = "module";

        internal static ModuleViewerSettings Settings;

        private bool oodleIsAvailable = true;

        public override string Name => "Module Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<ModuleViewerSettings>();
            if (!OodleDecompressor.DependencyExists())
            {
                LogOutput("WARNING: Oodle DLL required for Halo Infinite decompression was not found.");
                oodleIsAvailable = false;
            }
        }

        public override void Suspend() => SaveSettings(Settings);

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenKey, OpenPath, OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            if (key != OpenKey)
                return;

            var ofd = new OpenFileDialog
            {
                Filter = BrowseFileFilter,
                Multiselect = true,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(Settings.ModuleFolder))
                ofd.InitialDirectory = Settings.ModuleFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                OpenPhysicalFile(fileName);
        }

        private static ModuleType GetModuleVersion(string fileName)
        {
            if (File.Exists(fileName))
            {
                var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var reader = new DependencyReader(stream, ByteOrder.LittleEndian);
                reader.Seek(4, SeekOrigin.Begin);
                return (ModuleType)reader.ReadInt32();
            } 
            else
            {
                throw new FileNotFoundException("The file does not exist.", fileName);
            }
        }

        public override bool SupportsFileExtension(string extension) => extension.ToLowerInvariant() == ModuleFileExtension;

        public override void OpenPhysicalFile(string fileName)
        {
            var tabId = $"{Key}::{fileName}";
            if (Substrate.ShowTabById(tabId))
                return;

            LogOutput($"Loading module file: {fileName}");

            var moduleVersion = GetModuleVersion(fileName);

            try
            {
                if (moduleVersion == ModuleType.HaloInfinite && oodleIsAvailable)
                {
                    var mv = new Controls.HaloInfiniteModuleViewer();
                    mv.TabModel.ContentId = tabId;
                    mv.LoadModule(fileName);

                    Substrate.AddTool(mv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                    Substrate.AddRecentFile(fileName);

                    LogOutput($"Loaded module file: {fileName}");
                }
                else if (moduleVersion == ModuleType.Halo5Forge | moduleVersion == ModuleType.Halo5Server)
                {
                    var mv = new Controls.Halo5ModuleViewer();
                    mv.TabModel.ContentId = tabId;
                    mv.LoadModule(fileName);

                    Substrate.AddTool(mv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                    Substrate.AddRecentFile(fileName);

                    LogOutput($"Loaded module file: {fileName}");
                }
            }
            catch (Exception ex)
            {
                var message = $"Unable to load {Path.GetFileName(fileName)}:{Environment.NewLine}{ex.Message}";
                Substrate.ShowErrorMessage(message);
                Substrate.LogError(message, ex);
            }
        }

        internal class ModuleViewerSettings
        {
            [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
            [DisplayName("Modules Folder")]
            public string ModuleFolder { get; set; }

            [DisplayName("Hierarchy View")]
            public bool HierarchyView { get; set; }

            [Editor(typeof(BrowseFileEditor), typeof(BrowseFileEditor))]
            [DisplayName("String ID File")]
            public string StringIdFile { get; set; }
        }
    }

}
