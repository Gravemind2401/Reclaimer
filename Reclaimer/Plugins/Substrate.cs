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
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public static class Substrate
    {
        #region Non Public
        private class DefaultPlugin : Plugin
        {
            internal override string Key => nameof(Reclaimer);
            public override string Name => nameof(Reclaimer);
        }

        private static readonly DefaultPlugin defaultPlugin = new DefaultPlugin();
        private static readonly Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

        private static IEnumerable<Plugin> FindPlugins(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Plugin)))
                .Select(t => Activator.CreateInstance(t) as Plugin);
        }

        internal static event EventHandler<LogEventArgs> Log;

        internal static Dictionary<string, string> DefaultHandlers { get; set; }

        internal static IEnumerable<Plugin> AllPlugins => plugins.Values;

        internal static Plugin GetPlugin(string key) => plugins.ContainsKey(key) ? plugins[key] : null;

        internal static void LoadPlugins()
        {
            var temp = new List<Plugin>();

            temp.Add(defaultPlugin);
            temp.AddRange(FindPlugins(typeof(Substrate).Assembly));

            if (Directory.Exists(PluginsDirectory))
            {
                foreach (var fileName in Directory.EnumerateFiles(PluginsDirectory, "*.dll"))
                {
                    LogOutput($"Scanning {fileName} for plugins");
                    try
                    {
                        var assembly = Assembly.LoadFrom(fileName);

                        foreach (var p in FindPlugins(assembly))
                        {
                            LogOutput($"Found plugin {p.Key} [{p.Name}]");
                            temp.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Could not scan file {fileName}", ex);
                    }
                }
            }

            foreach (var p in temp)
            {
                LogOutput($"Loading plugin {p.Key} [{p.Name}]");
                try
                {
                    plugins.Add(p.Key, p);
                    p.Initialise();
                }
                catch (Exception ex)
                {
                    plugins.Remove(p.Key);
                    LogError($"Could not load plugin {p.Key} [{p.Name}]", ex);
                }
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
            catch (Exception e)
            {
                LogError($"Error loading settings for {plugins[key].Name}:", e);
                return new T();
            }
        }

        internal static void SavePluginSettings<T>(string key, T settings) where T : new()
        {
            if (App.Settings.PluginSettings.ContainsKey(key))
                App.Settings.PluginSettings[key] = settings;
            else App.Settings.PluginSettings.Add(key, settings);

            App.Settings.Save();
        }

        internal static void LogOutput(string message) => defaultPlugin.LogOutput(message);

        internal static void LogError(string message, Exception e) => defaultPlugin.LogError(message, e);

        internal static void LogOutput(Plugin source, LogEntry entry) => Log?.Invoke(source, new LogEventArgs(source, entry));

        internal static Plugin GetDefaultHandler(OpenFileArgs args)
        {
            if (App.Settings.DefaultHandlers.ContainsKey(args.FileTypeKey))
            {
                var handlerKey = App.Settings.DefaultHandlers[args.FileTypeKey];
                if (plugins.ContainsKey(handlerKey)) //in case the plugin is no longer installed
                    return plugins[handlerKey];
            }

            var handler = AllPlugins
                .Where(p => p.CanOpenFile(args))
                .OrderBy(p => p.Name)
                .FirstOrDefault();

            if (handler == null)
                return handler;

            if (App.Settings.DefaultHandlers.ContainsKey(args.FileTypeKey))
                App.Settings.DefaultHandlers.Remove(args.FileTypeKey);

            App.Settings.DefaultHandlers.Add(args.FileTypeKey, handler.Key);

            return handler;
        }

        internal static bool HandlePhysicalFile(string fileName)
        {
            var ext = Path.GetExtension(fileName).TrimStart('.');
            var handler = AllPlugins.FirstOrDefault(p => p.SupportsFileExtension(ext));
            handler?.OpenPhysicalFile(fileName);
            return handler != null;
        }

        internal static void Shutdown()
        {
            foreach (var p in AllPlugins)
                p.Suspend();
        }
        #endregion

        #region Public
        /// <summary>
        /// Gets the directory path that plugin assemblies are loaded from.
        /// </summary>
        public static string PluginsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        /// <summary>
        /// Opens a file object with the associated default handler.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public static bool OpenWithDefault(OpenFileArgs args)
        {
            var defaultHandler = GetDefaultHandler(args);
            if (defaultHandler == null)
                return false;

            defaultHandler.OpenFile(args);
            return true;
        }

        /// <summary>
        /// Prompts the user to select which handler to open a file object with.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public static bool OpenWithPrompt(OpenFileArgs args)
        {
            var defaultHandler = GetDefaultHandler(args);
            if (defaultHandler == null)
                return false;

            var allHandlers = AllPlugins
                .Where(p => p.CanOpenFile(args));

            OpenWithDialog.HandleFile(allHandlers, args);
            return true;
        }

        /// <summary>
        /// Fetches context items from across all plugins for a particular file object.
        /// </summary>
        /// <param name="context">The file arguments.</param>
        public static IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            return AllPlugins.SelectMany(p => p.GetContextItems(context));
        }

        /// <summary>
        /// Adds a utility to the specified window. If a tab control exists in the target location
        /// the utility will be added to the existing tab control, otherwise a new tab control will be created.
        /// The size of the tab control will be set to the host's default dock size.
        /// </summary>
        /// <param name="item">The utility item to add.</param>
        /// <param name="host">The window the utility will be added to.</param>
        /// <param name="targetDock">The dock are the utility will be added to.</param>
        public static void AddUtility(ITabContent item, IMultiPanelHost host, Dock targetDock)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            AddUtility(item, host, targetDock, host.MultiPanel.DefaultDockSize);
        }

        /// <summary>
        /// Adds a utility to the specified window. If a tab control exists in the target location
        /// the utility will be added to the existing tab control, otherwise a new tab control will be created.
        /// The size of the tab control will be set to <param name="targetSize"/>.
        /// </summary>
        /// <param name="item">The utility item to add.</param>
        /// <param name="host">The window the utility will be added to.</param>
        /// <param name="targetDock">The dock are the utility will be added to.</param>
        public static void AddUtility(ITabContent item, IMultiPanelHost host, Dock targetDock, GridLength targetSize)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (item.TabUsage != TabItemUsage.Utility)
                throw new ArgumentException("item TabUsage must be TabItemUsage.Utility", nameof(item));

            var opposite = targetDock == Dock.Left || targetDock == Dock.Top
                ? (Dock)((int)targetDock + 2)
                : (Dock)((int)targetDock - 2);

            UtilityTabControl tc;

            var idealPath = host.MultiPanel.GetChildren()
                .Select(c => host.MultiPanel.GetPathToElement(c))
                .Where(p => !p.Contains(opposite))
                .Where(p => p.Last() == targetDock)
                .OrderBy(p => p.Count)
                .FirstOrDefault();

            if (idealPath != null)
            {
                tc = host.MultiPanel.GetElementAtPath(idealPath) as UtilityTabControl;
                if (tc != null)
                {
                    tc.Items.Add(item);
                    return;
                }
            }

            var targetHalf = host.MultiPanel.GetPathToElement(host.DocumentContainer)
                .Cast<Dock?>()
                .LastOrDefault(d => d == targetDock || d == opposite);

            tc = new UtilityTabControl();
            var targetElement = targetHalf == null || targetHalf == targetDock
                ? host.DocumentContainer
                : null;

            host.MultiPanel.AddElement(tc, targetElement, targetDock, targetSize);
            tc.Items.Add(item);
        }

        /// <summary>
        /// Displays the output utility if it is not already visible.
        /// </summary>
        public static void ShowOutput()
        {
            if (Controls.OutputViewer.Instance.Parent != null)
                return;

            AddUtility(Controls.OutputViewer.Instance, GetHostWindow(), Dock.Bottom, new GridLength(250));
        }

        /// <summary>
        /// Gets the application's main window.
        /// </summary>
        public static IMultiPanelHost GetHostWindow() => GetHostWindow(null);

        /// <summary>
        /// Gets the owner window of a particular <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The element to find the host for.</param>
        public static IMultiPanelHost GetHostWindow(UIElement element)
        {
            IMultiPanelHost host;
            if (element == null)
                host = Application.Current.MainWindow as IMultiPanelHost;
            else host = Window.GetWindow(element) as IMultiPanelHost ?? Application.Current.MainWindow as IMultiPanelHost;
            return host;
        } 
        #endregion
    }

    /// <summary>
    /// Contains details about a file object that may be displayed to the user or manipulated by plugins.
    /// </summary>
    public class OpenFileArgs
    {
        /// <summary>
        /// The name of the file object.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The file object.
        /// </summary>
        public object File { get; }

        /// <summary>
        /// A unique key representing the type of file.
        /// </summary>
        public string FileTypeKey { get; }

        /// <summary>
        /// The window the file should be sent to.
        /// </summary>
        public IMultiPanelHost TargetWindow { get; }

        public OpenFileArgs(string fileName, object file, string key)
            : this(fileName, file, key, Substrate.GetHostWindow())
        {

        }

        public OpenFileArgs(string fileName, object file, string fileTypeKey, IMultiPanelHost targetWindow)
        {
            FileName = fileName;
            File = file;
            FileTypeKey = fileTypeKey;
            TargetWindow = targetWindow;
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
