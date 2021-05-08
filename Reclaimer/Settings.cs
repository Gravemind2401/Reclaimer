using Newtonsoft.Json;
using Reclaimer.Models;
using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer
{
    internal sealed class Settings
    {
        private static string settingsJson => Path.Combine(AppDataDirectory, "settings.json");

        public static string AppBaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        public static string AppDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gravemind2401\\Reclaimer");

        private static JsonSerializerSettings serializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            Converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        public string Theme { get; set; }
        public WindowState WindowState { get; set; }
        public DateTime LastUpdateCheck { get; set; }

        public AppRelease LatestRelease { get; set; }

        internal bool ShouldCheckUpdates => (DateTime.Now - LastUpdateCheck).TotalHours > 12;

        public Dictionary<string, string> DefaultHandlers
        {
            get { return Substrate.DefaultHandlers; }
            set { Substrate.DefaultHandlers = value; }
        }

        //will initially be read in as JObjects
        public Dictionary<string, object> PluginSettings { get; set; }

        public List<string> RecentFiles { get; set; }

        public UserSettings UserSettings { get; set; }

        public Settings()
        {
            Theme = App.Themes.First();
            DefaultHandlers = new Dictionary<string, string>();
            PluginSettings = new Dictionary<string, object>();
            RecentFiles = new List<string>();
            UserSettings = new UserSettings();
        }

        public static Settings FromFile()
        {
            var settingsContent = File.Exists(settingsJson)
                ? File.ReadAllText(settingsJson)
                : null;

            if (string.IsNullOrWhiteSpace(settingsContent))
                return new Settings();

            return JsonConvert.DeserializeObject<Settings>(settingsContent);
        }

        public void Save()
        {
            File.WriteAllText(settingsJson, JsonConvert.SerializeObject(this, serializerSettings));
        }
    }

    //settings that will be visible in the settings viewer property grid
    public sealed class UserSettings
    {
        [DisplayName("Restore Window State")]
        public bool RememberWindowState { get; set; }

        [DisplayName("Auto Updates Check")]
        public bool AutoUpdatesCheck { get; set; }
    }
}
