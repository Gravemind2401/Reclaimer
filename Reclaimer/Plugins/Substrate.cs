using Newtonsoft.Json;
using Reclaimer.Windows;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Plugins
{
    public static class Substrate
    {
        private class DefaultPlugin : Plugin
        {
            internal override string Key => nameof(Reclaimer);
            public override string Name => nameof(Reclaimer);
        }

        internal static event EventHandler<LogEventArgs> Log;

        private static readonly DefaultPlugin defaultPlugin = new DefaultPlugin();
        private static readonly Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

        internal static Dictionary<string, string> DefaultHandlers { get; set; }

        internal static IEnumerable<Plugin> AllPlugins => plugins.Values;

        internal static Plugin GetPlugin(string key) => plugins.ContainsKey(key) ? plugins[key] : null;

        private static IEnumerable<Plugin> FindPlugins(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Plugin)))
                .Select(t => Activator.CreateInstance(t) as Plugin);
        }

        internal static void LoadPlugins()
        {
            var temp = new List<Plugin>();

            temp.Add(defaultPlugin);
            temp.AddRange(FindPlugins(typeof(Substrate).Assembly));

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (Directory.Exists(dir))
            {
                foreach (var fileName in Directory.EnumerateFiles(dir, "*.dll"))
                {
                    LogOutput($"Scanning {fileName} for plugins");
                    var assembly = Assembly.LoadFrom(fileName);

                    foreach (var p in FindPlugins(assembly))
                    {
                        LogOutput($"Found plugin {p.Key} [{p.Name}]");
                        temp.Add(p);
                    }
                }
            }

            foreach (var p in temp)
            {
                LogOutput($"Loading plugin {p.Key} [{p.Name}]");
                plugins.Add(p.Key, p);
                p.Initialise();
            }
        }

        internal static T GetPluginSettings<T>(string key) where T : new()
        {
            if (!App.Settings.PluginSettings.ContainsKey(key))
                return new T();

            //we cant just cast it because we cant guarantee it has the correct type
            //but we still want to preserve settings between changes, so re-serializing
            //will preserve any properties that still match

            try
            {
                var json = JsonConvert.SerializeObject(App.Settings.PluginSettings[key]);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch { return new T(); }
        }

        internal static void SavePluginSettings<T>(string key, T settings) where T : new()
        {
            if (App.Settings.PluginSettings.ContainsKey(key))
                App.Settings.PluginSettings[key] = settings;
            else App.Settings.PluginSettings.Add(key, settings);

            App.Settings.Save();
        }

        internal static void LogOutput(string message) => defaultPlugin.LogOutput(message);

        internal static void LogOutput(Plugin source, LogEntry entry) => Log?.Invoke(source, new LogEventArgs(source, entry));

        public static bool OpenWithDefault(object file, string key, IMultiPanelHost targetWindow)
        {
            var found = false;
            foreach (var p in AllPlugins.Where(p => p.CanOpenFile(file, key)))
            {
                found = true;
                p.OpenFile(file, key, targetWindow);
            }

            return found;
        }

        //open with prompt

        //add utility/tab

        internal static void Shutdown()
        {
            foreach (var p in AllPlugins)
                p.Suspend();
        }

        public static IMultiPanelHost GetHostWindow() => GetHostWindow(null);

        public static IMultiPanelHost GetHostWindow(UIElement element)
        {
            IMultiPanelHost host;
            if (element == null)
                host = Application.Current.MainWindow as IMultiPanelHost;
            else host = Window.GetWindow(element) as IMultiPanelHost ?? Application.Current.MainWindow as IMultiPanelHost;
            return host;
        }
    }

    internal struct LogEntry
    {
        public readonly DateTime Timestamp;
        public readonly string Message;

        public LogEntry(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message;
        }
    }

    internal class LogEventArgs : EventArgs
    {
        public Plugin Source { get; }
        public LogEntry Entry { get; }

        public LogEventArgs(Plugin source, LogEntry entry)
        {
            Source = source;
            Entry = entry;
        }
    }
}
