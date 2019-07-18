using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class MapViewerPlugin : Plugin
    {
        const string OpenKey = "MapViewer.OpenMap";
        const string OpenPath = "File\\Open Map";

        internal static MapViewerSettings Settings;

        public override string Name => "Map Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<MapViewerSettings>();
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override IEnumerable<PluginMenuItem> MenuItems
        {
            get
            {
                yield return new PluginMenuItem(OpenKey, OpenPath);
            }
        }

        public override void OnMenuItemClick(string key)
        {
            if (key != OpenKey) return;

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Halo Map Files|*.map",
                Multiselect = false,
                CheckFileExists = true
            };

            if (ofd.ShowDialog() != true)
                return;

            LogOutput($"Loading map file: {ofd.FileName}");

            var host = Substrate.GetHostWindow(null);

            var tc = host.MultiPanel.GetElementAtPath(Dock.Left) as UtilityTabControl;
            var mv = new Controls.MapViewer();

            if (tc == null) tc = new UtilityTabControl();

            if (!host.MultiPanel.GetChildren().Contains(tc))
                host.MultiPanel.AddElement(tc, null, Dock.Left, new GridLength(400));

            mv.LoadMap(ofd.FileName);
            tc.Items.Add(mv);

            LogOutput($"Loaded map file: {ofd.FileName}");
        }
    }

    internal class MapViewerSettings
    {
        public bool HierarchyView { get; set; }
    }
}
