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
        private static string settingsJson => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static JsonSerializerSettings serializerSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        public string Theme { get; set; }
        public WindowState WindowState { get; set; }

        public Dictionary<string, string> DefaultHandlers
        {
            get { return Substrate.DefaultHandlers; }
            set { Substrate.DefaultHandlers = value; }
        }

        //will initially be read in as JObjects
        public Dictionary<string, object> PluginSettings { get; set; }

        public Settings()
        {
            Theme = App.Themes.First();
            DefaultHandlers = new Dictionary<string, string>();
            PluginSettings = new Dictionary<string, object>();
        }

        public static Settings FromFile()
        {
            if (!File.Exists(settingsJson))
                return new Settings();
            else return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsJson));
        }

        public void Save()
        {
            File.WriteAllText(settingsJson, JsonConvert.SerializeObject(this, serializerSettings));
        }
    }
}
