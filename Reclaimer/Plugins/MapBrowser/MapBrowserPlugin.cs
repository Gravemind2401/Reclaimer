using Reclaimer.Controls.Editors;
using System.ComponentModel;

namespace Reclaimer.Plugins.MapBrowser
{
    public class MapBrowserPlugin : Plugin
    {
        internal static MapBrowserSettings Settings { get; private set; }

        public override string Name => "Map Browser";

        public override void Initialise() => Settings = LoadSettings<MapBrowserSettings>();
        public override void Suspend() => SaveSettings(Settings);

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem("", "View\\Map Browser", OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            var tabId = $"{Key}::BrowserControl";
            if (Substrate.ShowTabById(tabId))
                return;

            var container = Substrate.GetHostWindow().DocumentPanel;
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
    }
}
