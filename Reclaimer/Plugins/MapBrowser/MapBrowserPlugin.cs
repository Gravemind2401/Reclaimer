using Reclaimer.Controls.Editors;
using System.ComponentModel;

namespace Reclaimer.Plugins.MapBrowser
{
    public class MapBrowserPlugin : Plugin
    {
        internal static MapBrowserPlugin Instance { get; private set; }
        internal static MapBrowserSettings Settings { get; private set; }

        public override string Name => "Map Browser";

        public override void Initialise()
        {
            Instance = this;
            Settings = LoadSettings<MapBrowserSettings>();
        }

        public override void Suspend() => SaveSettings(Settings);

        public override void PostInitialise()
        {
            if (Settings.ShowOnStartup)
                OnMenuItemClick(null);
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem("", "View\\Map Browser", OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            var tabId = $"{Key}::BrowserControl";
            if (Substrate.ShowTabById(tabId))
                return;

            var container = Substrate.GetHostWindow()?.DocumentPanel;
            if (container == null)
                return;

            var control = new Controls.MapBrowser();

            control.TabModel.ContentId = tabId;
            container.AddItem(control.TabModel);
        }
    }

    internal sealed class MapBrowserSettings
    {
        [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
        [DisplayName("Steam Library Folder")]
        public string SteamLibraryFolder { get; set; }

        [DisplayName("Additional Map Folders")]
        public List<MapFolder> CustomFolders { get; set; }

        [DisplayName("Show On Startup")]
        public bool ShowOnStartup { get; set; }

        [DisplayName("Close After Selecting Map")]
        public bool CloseAfterSelection { get; set; }
    }

    internal sealed class MapFolder
    {
        [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
        [DisplayName("Map Folder")]
        public string Directory { get; set; }

        public override string ToString() => Directory;
    }
}
