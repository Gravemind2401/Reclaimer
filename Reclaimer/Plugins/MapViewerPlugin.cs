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
    public class MapViewerPlugin : Plugin
    {
        private const string OpenKey = "MapViewer.OpenMap";
        private const string OpenPath = "File\\Open Map";
        private const string BrowseFileFilter = "Halo Map Files|*.map;*.yelo";
        private const string MapFileExtension = "map";
        private const string YeloFileExtension = "yelo";

        internal static MapViewerSettings Settings { get; private set; }

        public override string Name => "Map Viewer";

        public override void Initialise() => Settings = LoadSettings<MapViewerSettings>();
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

            if (!string.IsNullOrEmpty(Settings.MapFolder))
                ofd.InitialDirectory = Settings.MapFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                OpenPhysicalFile(fileName);
        }

        public override bool SupportsFileExtension(string extension)
        {
            return extension.ToLower() == MapFileExtension || extension.ToLower() == YeloFileExtension;
        }

        public override void OpenPhysicalFile(string fileName)
        {
            var tabId = $"{Key}::{fileName}";
            if (Substrate.ShowTabById(tabId))
                return;

            LogOutput($"Loading map file: {fileName}");

            try
            {
                var mv = new Controls.MapViewer();
                mv.TabModel.ContentId = tabId;
                mv.LoadMap(fileName);
                Substrate.AddTool(mv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                Substrate.AddRecentFile(fileName);

                if (Settings.AutoMapFolder)
                    Settings.MapFolder = Path.GetDirectoryName(fileName);

                LogOutput($"Loaded map file: {fileName}");
            }
            catch (Exception ex)
            {
                var message = $"Unable to load {Path.GetFileName(fileName)}:{Environment.NewLine}{ex.Message}";
                Substrate.ShowErrorMessage(message);
                Substrate.LogError(message, ex);
            }
        }
    }

    internal sealed class MapViewerSettings
    {
        [Editor(typeof(BrowseFolderEditor), typeof(PropertyValueEditor))]
        [DisplayName("Maps Folder")]
        public string MapFolder { get; set; }

        [DisplayName("Hierarchy View")]
        public bool HierarchyView { get; set; }

        [DisplayName("Auto Update Maps Folder")]
        public bool AutoMapFolder { get; set; }
    }
}
