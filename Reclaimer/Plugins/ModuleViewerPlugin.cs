using Reclaimer.Controls.Editors;
using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class ModuleViewerPlugin : Plugin
    {
        const string OpenKey = "ModuleViewer.OpenModule";
        const string OpenPath = "File\\Open Module";

        internal static ModuleViewerSettings Settings;

        public override string Name => "Module Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<ModuleViewerSettings>();
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenKey, OpenPath, OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            if (key != OpenKey) return;

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Halo Module Files|*.module",
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

        public override bool SupportsFileExtension(string extension)
        {
            return extension.ToLowerInvariant() == "module";
        }

        public override void OpenPhysicalFile(string fileName)
        {
            var tabId = $"{Key}::{fileName}";
            if (Substrate.ShowTabById(tabId))
                return;

            LogOutput($"Loading module file: {fileName}");

            try
            {
                var mv = new Controls.ModuleViewer();
                mv.TabModel.ContentId = tabId;
                mv.LoadModule(fileName);
                Substrate.AddTool(mv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                Substrate.AddRecentFile(fileName);

                LogOutput($"Loaded module file: {fileName}");
            }
            catch (Exception ex)
            {
                Substrate.ShowErrorMessage($"Unable to load {Path.GetFileName(fileName)}:{Environment.NewLine}{ex.Message}");
            }
        }
    }

    internal class ModuleViewerSettings
    {
        [Editor(typeof(BrowseFolderEditor), typeof(PropertyValueEditor))]
        [DisplayName("Modules Folder")]
        public string ModuleFolder { get; set; }

        [DisplayName("Hierarchy View")]
        public bool HierarchyView { get; set; }
    }
}
