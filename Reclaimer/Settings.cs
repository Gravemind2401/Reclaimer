using Newtonsoft.Json;
using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer
{
    internal class Settings
    {
        private static string oldSettingsJson => Path.Combine(AppBaseDirectory, "settings.json");
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
        public bool RememberWindowState { get; set; }
        public WindowState WindowState { get; set; }

        public Dictionary<string, string> DefaultHandlers
        {
            get { return Substrate.DefaultHandlers; }
            set { Substrate.DefaultHandlers = value; }
        }

        //will initially be read in as JObjects
        public Dictionary<string, object> PluginSettings { get; set; }

        public List<string> RecentFiles { get; set; }

        public Settings()
        {
            Theme = App.Themes.First();
            DefaultHandlers = new Dictionary<string, string>();
            PluginSettings = new Dictionary<string, object>();
            RecentFiles = new List<string>();
        }

        public static Settings FromFile()
        {
            var settingsContent = File.Exists(settingsJson)
                ? File.ReadAllText(settingsJson)
                : null;

            if (string.IsNullOrWhiteSpace(settingsContent) && File.Exists(oldSettingsJson))
            {
                settingsContent = File.ReadAllText(oldSettingsJson);
                try { File.Delete(oldSettingsJson); }
                catch (Exception ex) { Substrate.LogError("Unable to delete old settings.json.", ex); }
            }

            if (string.IsNullOrWhiteSpace(settingsContent))
                return new Settings();

            return JsonConvert.DeserializeObject<Settings>(settingsContent);
        }

        public void Save()
        {
            File.WriteAllText(settingsJson, JsonConvert.SerializeObject(this, serializerSettings));
        }
    }
}
