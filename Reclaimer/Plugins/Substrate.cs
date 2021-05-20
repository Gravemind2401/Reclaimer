using Newtonsoft.Json;
using Reclaimer.Models;
using Reclaimer.Utilities;
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

            public override void Initialise()
            {
                settings = App.UserSettings;
            }
        }

        private static readonly DefaultPlugin defaultPlugin = new DefaultPlugin();
        private static readonly Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
        private static readonly Dictionary<string, Tuple<Plugin, MethodInfo>> sharedFunctions = new Dictionary<string, Tuple<Plugin, MethodInfo>>();

        private static IEnumerable<Plugin> FindPlugins(Assembly assembly)
        {
            return assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Plugin)))
                .Select(t => Activator.CreateInstance(t) as Plugin);
        }

        private static IEnumerable<MethodInfo> FindExportFunctions(Plugin plugin)
        {
            return plugin.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(SharedFunctionAttribute)));
        }

        internal static event EventHandler RecentsChanged;
        internal static event EventHandler<LogEventArgs> Log;
        internal static event EventHandler<LogEventArgs> EmptyLog;
        internal static event EventHandler<StatusChangedArgs> StatusChanged;

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
                foreach (var folder in Directory.EnumerateDirectories(PluginsDirectory))
                {
                    var fileName = Path.Combine(folder, $"{Path.GetFileName(folder)}.dll");
                    if (!File.Exists(fileName))
                        continue;

                    LogOutput($"Scanning {fileName} for plugins");
                    try
                    {
                        var assembly = Assembly.LoadFile(fileName);

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

                    foreach (var m in FindExportFunctions(p))
                    {
                        var attr = m.GetCustomAttribute<SharedFunctionAttribute>();
                        var funcName = string.IsNullOrWhiteSpace(attr.Name) ? m.Name : attr.Name;
                        var key = $"{p.Key}.{funcName}";

                        if (!sharedFunctions.ContainsKey(key))
                            sharedFunctions.Add($"{p.Key}.{funcName}", Tuple.Create(p, m));
                    }
                }
                catch (Exception ex)
                {
                    plugins.Remove(p.Key);
                    LogError($"Could not load plugin {p.Key} [{p.Name}]", ex);
                }
            }

            foreach (var p in AllPlugins)
            {
                try
                {
                    p.PostInitialise();
                }
                catch (Exception ex)
                {
                    LogError($"Failed post-initialise for plugin {p.Key} [{p.Name}]", ex);
                }
            }
        }

        internal static T GetPluginSettings<T>(string key) where T : new()
        {
            if (!App.Settings.PluginSettings.ContainsKey(key))
                return GetDefaultPluginSettings<T>();

            //we cant just cast it because we cant guarantee it has the correct type
            //but we still want to preserve settings between changes, so re-serializing
            //will preserve any properties that still match

            try
            {
                var json = JsonConvert.SerializeObject(App.Settings.PluginSettings[key], Settings.SerializerSettings);
                var settings = JsonConvert.DeserializeObject<T>(json, Settings.SerializerSettings);
                (settings as IPluginSettings)?.ApplyDefaultValues(false);

                return settings;
            }
            catch (Exception e)
            {
                LogError($"Error loading settings for {plugins[key].Name}:", e);
                return GetDefaultPluginSettings<T>();
            }
        }

        private static T GetDefaultPluginSettings<T>() where T : new()
        {
            var settings = new T();
            (settings as IPluginSettings)?.ApplyDefaultValues(true);
            return settings;
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

        internal static void LogOutput(Plugin source, LogEntry entry, bool focusOutput)
        {
            if (focusOutput) //only focus if already present in ui - do not add new tab
                ShowTab(Controls.OutputViewer.Instance);

            Log?.Invoke(source, new LogEventArgs(source, entry));
        }

        internal static void ClearLogOutput(Plugin source) => EmptyLog?.Invoke(source, new LogEventArgs(source, default(LogEntry)));

        internal static void SetSystemWorkingStatus(string status)
        {
            defaultPlugin.SetWorkingStatus(status);
        }

        internal static void ClearSystemWorkingStatus()
        {
            defaultPlugin.ClearWorkingStatus();
        }

        internal static void RaiseWorkingStatusChanged(Plugin source)
        {
            if (source.WorkingStatus != null)
                StatusChanged?.Invoke(typeof(Substrate), new StatusChangedArgs(source.Name, source.WorkingStatus));
            else
            {
                var mostRecent = AllPlugins
                    .Where(p => p.WorkingStatus != null)
                    .OrderByDescending(p => p.WorkingStatusTime)
                    .FirstOrDefault();

                StatusChanged?.Invoke(typeof(Substrate), new StatusChangedArgs(mostRecent?.Name, mostRecent?.WorkingStatus));
            }
        }

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
                .OrderByDescending(p => p.FilePriority ?? int.MaxValue)
                .ThenBy(p => p.Name)
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
        /// Gets a list of available function keys.
        /// </summary>
        public static IEnumerable<string> GetSharedFunctionKeys()
        {
            return sharedFunctions.Keys.OrderBy(s => s);
        }

        /// <summary>
        /// Returns a shared function using the specified key.
        /// </summary>
        /// <typeparam name="T">The type of delegate to return.</typeparam>
        /// <param name="key">The function identifier.</param>
        public static T GetSharedFunction<T>(string key) where T : class
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
                return null;

            if (!sharedFunctions.ContainsKey(key))
                return null;

            var t = sharedFunctions[key];

            try
            {
                return t.Item2.CreateDelegate(typeof(T), t.Item2.IsStatic ? null : t.Item1) as T;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the directory path that plugin assemblies are loaded from.
        /// </summary>
        public static string PluginsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.PluginsFolderName);

        /// <summary>
        /// Opens a file object with the associated default handler.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public static bool OpenWithDefault(OpenFileArgs args)
        {
            var defaultHandler = GetDefaultHandler(args);
            if (defaultHandler == null || !defaultHandler.CanOpenFile(args))
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
        /// Adds an entry to the recent files menu.
        /// </summary>
        /// <param name="fileName">The full path of the file to add.</param>
        public static void AddRecentFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            App.Settings.RecentFiles.RemoveAll(s => s.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            App.Settings.RecentFiles.Insert(0, fileName);

            while (App.Settings.RecentFiles.Count > App.UserSettings.MaxRecentFiles)
                App.Settings.RecentFiles.RemoveAt(App.UserSettings.MaxRecentFiles);

            RecentsChanged?.Invoke(typeof(Substrate), EventArgs.Empty);
        }

        /// <summary>
        /// Adds a tool to the specified window. If a tab control exists in the target location
        /// the tool will be added to the existing tab control, otherwise a new tab control will be created.
        /// The size of the tab control will be set to the host's default dock size.
        /// </summary>
        /// <param name="item">The tool item to add.</param>
        /// <param name="host">The window the tool will be added to.</param>
        /// <param name="targetDock">The dock area the tool will be added to.</param>
        public static void AddTool(TabModel item, ITabContentHost host, Dock targetDock)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            AddTool(item, host, targetDock, new GridLength(DockContainerModel.DefaultDockSize));
        }

        /// <summary>
        /// Adds a tool to the specified window. If a tab control exists in the target location
        /// the tool will be added to the existing tab control, otherwise a new tab control will be created.
        /// The size of the tab control will be set to <param name="targetSize"/>.
        /// </summary>
        /// <param name="item">The tool item to add.</param>
        /// <param name="host">The window the tool will be added to.</param>
        /// <param name="targetDock">The dock area the tool will be added to.</param>
        public static void AddTool(TabModel item, ITabContentHost host, Dock targetDock, GridLength targetSize)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            if (item.Usage != TabItemType.Tool)
                throw new ArgumentException("item Usage must be TabItemType.Tool", nameof(item));

            var well = host.DockContainer.AllTabs
                .Where(t => (t.Parent as ToolWellModel)?.Dock == targetDock)
                .Select(t => t.Parent as ToolWellModel)
                .FirstOrDefault();

            host.DockContainer.AddTool2(item, targetDock, targetSize);
        }

        /// <summary>
        /// Displays a <see cref="MessageBox"/> using the application name as the title, an OK button and an error icon.
        /// </summary>
        /// <param name="message"></param>
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, Application.Current.MainWindow.Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Displays the output tool if it is not already visible.
        /// </summary>
        public static void ShowOutput()
        {
            if (Controls.OutputViewer.Instance.Parent != null)
                FocusOutput();
            else AddTool(Controls.OutputViewer.Instance, GetHostWindow(), Dock.Bottom, new GridLength(250));
        }

        /// <summary>
        /// Gives focus to the output tool if it has been added to the window.
        /// </summary>
        public static void FocusOutput()
        {
            ShowTab(Controls.OutputViewer.Instance);
        }

        /// <summary>
        /// Gets the application's main window.
        /// </summary>
        public static ITabContentHost GetHostWindow() => GetHostWindow(null);

        /// <summary>
        /// Gets the owner window of a particular <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The element to find the host for.</param>
        public static ITabContentHost GetHostWindow(UIElement element)
        {
            ITabContentHost host;
            if (element == null)
                host = Application.Current.MainWindow as ITabContentHost;
            else host = Window.GetWindow(element) as ITabContentHost ?? Application.Current.MainWindow as ITabContentHost;
            return host;
        }

        /// <summary>
        /// Finds the <see cref="TabModel"/> with the specified <paramref name="contentId"/> and, if a tab was found, gives it focus.
        /// </summary>
        /// <param name="contentId">The ContentId of the <see cref="TabModel"/> to show.</param>
        /// <returns>true if a tab with a matching ID was found, otherwise false.</returns>
        public static bool ShowTabById(string contentId)
        {
            var hosts = Application.Current.Windows.OfType<ITabContentHost>();

            var tab = hosts.SelectMany(h => h.DockContainer.AllTabs)
                .FirstOrDefault(t => t.ContentId == contentId);

            return ShowTab(tab);
        }

        /// <summary>
        /// Gives focus to the specified <see cref="TabModel"/>, if possible.
        /// </summary>
        /// <param name="tab">The <see cref="TabModel"/> to show.</param>
        /// <returns>true if the tab was focused, otherwise false.</returns>
        public static bool ShowTab(TabModel tab)
        {
            if (tab?.Parent == null)
                return false;

            var well = tab.Parent as TabWellModelBase;
            if (well != null)
            {
                var currentIndex = well.Children.IndexOf(tab);
                if (currentIndex > 0)
                    well.Children.Move(currentIndex, 0);

                well.SelectedItem = tab;
                well.IsActive = true;

                return true;
            }

            var container = tab.Parent as DockContainerModel;
            if (container != null)
            {
                container.SelectedDockItem = tab;
                tab.IsActive = true;

                return true;
            }

            return false;
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
        public object[] File { get; }

        /// <summary>
        /// A unique key representing the type of file.
        /// </summary>
        public string FileTypeKey { get; }

        /// <summary>
        /// The window the file should be sent to.
        /// </summary>
        public ITabContentHost TargetWindow { get; }

        public OpenFileArgs(string fileName, string fileTypeKey, params object[] file)
            : this(fileName, fileTypeKey, Substrate.GetHostWindow(), file)
        {

        }

        public OpenFileArgs(string fileName, string fileTypeKey, ITabContentHost targetWindow, params object[] file)
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

    internal class StatusChangedArgs : EventArgs
    {
        public string PluginName { get; }
        public string Status { get; }

        public StatusChangedArgs(string pluginName, string status)
        {
            PluginName = pluginName;
            Status = status;
        }
    }
}
