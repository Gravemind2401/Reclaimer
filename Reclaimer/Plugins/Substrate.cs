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
        internal static event EventHandler<LogEventArgs> Log;

        private static readonly Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

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

            temp.AddRange(FindPlugins(typeof(Substrate).Assembly));

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (Directory.Exists(dir))
            {
                foreach (var fileName in Directory.EnumerateFiles(dir, "*.dll"))
                {
                    var assembly = Assembly.LoadFrom(fileName);
                    temp.AddRange(FindPlugins(assembly).ToList());
                }
            }

            foreach (var p in temp)
                plugins.Add(p.Key, p);
        }

        internal static void LogOutput(Plugin source, string message) => Log?.Invoke(source, new LogEventArgs(source, message));

        public static bool OpenWithDefault(object file, string key, IMultiPanelHost targetWindow)
        {
            var handler = AllPlugins.FirstOrDefault(p => p.CanOpenFile(file, key));
            handler?.OpenFile(file, key, targetWindow);
            return handler != null;
        }

        //open with prompt

        //add utility/tab

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

    internal class LogEventArgs : EventArgs
    {
        public Plugin Source { get; }
        public string Message { get; }

        public LogEventArgs(Plugin source, string message)
        {
            Source = source;
            Message = message;
        }
    }
}
