using Newtonsoft.Json;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;

namespace Reclaimer
{
    internal sealed class Settings
    {
        private static string settingsJson => Path.Combine(AppDataDirectory, Constants.SettingsFileName);

        public static string AppBaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        public static string AppDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.AppDataSubfolder);

        internal static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        public string Theme { get; set; }
        public WindowState WindowState { get; set; }
        public DateTime LastUpdateCheck { get; set; }

        public AppRelease LatestRelease { get; set; }

        internal bool ShouldCheckUpdates => (DateTime.Now - LastUpdateCheck).TotalHours > 12;

        public Dictionary<string, string> DefaultHandlers
        {
            get => Substrate.DefaultHandlers;
            set => Substrate.DefaultHandlers = value;
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
            UserSettings = Utils.CreateDefaultInstance<UserSettings>();
        }

        public static Settings FromFile()
        {
            var settingsContent = File.Exists(settingsJson)
                ? File.ReadAllText(settingsJson)
                : null;

            if (string.IsNullOrWhiteSpace(settingsContent))
                return new Settings();

            var result = JsonConvert.DeserializeObject<Settings>(settingsContent, SerializerSettings);
            return result ?? new Settings();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsJson));
            File.WriteAllText(settingsJson, JsonConvert.SerializeObject(this, SerializerSettings));
        }
    }

    //settings that will be visible in the settings viewer property grid
    public sealed class UserSettings
    {
        [DisplayName("Restore Window State")]
        [DefaultValue(false)]
        public bool RememberWindowState { get; set; }

        [DisplayName("Auto Updates Check")]
        [DefaultValue(false)]
        public bool AutoUpdatesCheck { get; set; }

        [DisplayName("Max Recent Files Count")]
        [DefaultValue(10), Range(0, 50)]
        public int MaxRecentFiles { get; set; }
    }
}
