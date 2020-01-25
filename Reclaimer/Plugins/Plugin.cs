using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public delegate void MenuItemClickHandler(string key);
    public delegate void ContextItemClickHandler(string key, OpenFileArgs context);

    /// <summary>
    /// Represents a menu item that will be added to the application's menu bar.
    /// </summary>
    public class PluginMenuItem
    {
        private readonly string key;
        private readonly MenuItemClickHandler handler;

        /// <summary>
        /// Gets the menu path to the menu item.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Executes the handler associated with the menu item.
        /// </summary>
        public void ExecuteHandler() => handler(key);

        /// <summary>
        /// Creates a new instance of the <see cref="PluginMenuItem"/> class.
        /// </summary>
        /// <param name="key">The unique key associated with this menu item.</param>
        /// <param name="path">The menu path to this menu item.</param>
        /// <param name="handler">The method to execute when the menu item is clicked.</param>
        public PluginMenuItem(string key, string path, MenuItemClickHandler handler)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            this.key = key;
            this.handler = handler;
            Path = path;
        }
    }

    /// <summary>
    /// Represents a context menu item that will be displayed for a file object.
    /// </summary>
    public class PluginContextItem
    {
        private readonly string key;
        private readonly ContextItemClickHandler handler;

        /// <summary>
        /// Gets the menu path to the context menu item.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Executes the handler associated with the context menu item.
        /// </summary>
        /// <param name="context">The file object that the context menu is attached to.</param>
        public void ExecuteHandler(OpenFileArgs context) => handler(key, context);

        /// <summary>
        /// Creates a new instance of the <see cref="PluginContextItem"/> class.
        /// </summary>
        /// <param name="key">The unique key associated with this menu item.</param>
        /// <param name="path">The menu path to this menu item.</param>
        /// <param name="handler">The method to execute when the context menu item is clicked.</param>
        public PluginContextItem(string key, string path, ContextItemClickHandler handler)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            this.key = key;
            this.handler = handler;
            Path = path;
        }
    }

    public abstract class Plugin
    {
        internal readonly List<LogEntry> logEntries = new List<LogEntry>();

        /// <summary>
        /// Creates a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        public Plugin() { }

        internal string Assembly => GetType().Assembly.FullName;

        internal virtual string Key => GetType().FullName;

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// When overidden in a derived class, performs any initialisation required by the plugin.
        /// This method is called by the <see cref="Substrate"/> when the plugin is first loaded.
        /// </summary>
        public virtual void Initialise() { }

        /// <summary>
        /// When overidden in a derived class, performs any cleanup required by the plugin.
        /// This method is called by the <see cref="Substrate"/> when the plugin is unloaded.
        /// </summary>
        public virtual void Suspend() { }

        /// <summary>
        /// Reads the current plugin's settings from the application's settings file.
        /// If the settings failed to be parsed the default value will be returned.
        /// </summary>
        /// <typeparam name="T">The type used to store the settings.</typeparam>
        protected T LoadSettings<T>() where T : new()
        {
            return Substrate.GetPluginSettings<T>(Key);
        }

        /// <summary>
        /// Saves the current plugin's settings to the application's settings file.
        /// </summary>
        /// <typeparam name="T">The type used to store the settings.</typeparam>
        /// <param name="settings">The settings.</param>
        protected void SaveSettings<T>(T settings) where T : new()
        {
            Substrate.SavePluginSettings(Key, settings);
        }

        /// <summary>
        /// When overidden in a derived class, gets a value indicating if the current
        /// plugin accepts a particular type of file object. If this method returns true,
        /// the <see cref="Substrate"/> may proceed to call <see cref="OpenFile(OpenFileArgs)"/>.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public virtual bool CanOpenFile(OpenFileArgs args) => false;

        /// <summary>
        /// When overidden in a derived class, processes the request to handle a particular file object.
        /// This method is called by the <see cref="Substrate"/> when the user requests for a file to be handled.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public virtual void OpenFile(OpenFileArgs args) { }

        /// <summary>
        /// When overidden in a derived class, gets a value indicating if the current
        /// plugin can handle physical files of a particular file extension. If this method
        /// returns true, the <see cref="Substrate"/> may proceed to call <see cref="OpenPhysicalFile(string fileName)"/>.
        /// </summary>
        /// <param name="extension">The file extension to handle.</param>
        public virtual bool SupportsFileExtension(string extension) => false;

        /// <summary>
        /// When overidden in a derived class, processes the request to handle a particular physical file.
        /// This method is called by the <see cref="Substrate"/> when the user requests for a physical file to be opened.
        /// </summary>
        /// <param name="args">The file arguments.</param>
        public virtual void OpenPhysicalFile(string fileName) { }

        /// <summary>
        /// When overidden in a derived class, gets a list of menu items provided by the current plugin.
        /// Any menu items returned by this method will be added to the application's menu bar.
        /// </summary>
        public virtual IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield break;
        }

        /// <summary>
        /// When overidden in a derived class, gets a list of context menu items provided by the current plugin
        /// for a particular file object. This method is called by the <see cref="Substrate"/> when another plugin
        /// requests context items for a particular file object.
        /// </summary>
        /// <param name="context">The file arguments.</param>
        public virtual IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            yield break;
        }

        //themes (including existing)

        /// <summary>
        /// Logs a message to the output pane.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected internal void LogOutput(string message)
        {
            var entry = new LogEntry(DateTime.Now, message);
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry);
        }

        /// <summary>
        /// Logs an error to the output pane with a related message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="e">The error to log.</param>
        protected internal void LogError(string message, Exception e)
        {
            var entry = new LogEntry(DateTime.Now, $"{message}{Environment.NewLine}{e.ToString()}");
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry);
        }

        /// <summary>
        /// Clears the current plugin's log message from the output pane.
        /// </summary>
        protected internal void ClearLog()
        {
            logEntries.Clear();
        }
    }

    /// <summary>
    /// Specifies that a plugin function should be made available for use by other plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExportFunctionAttribute : Attribute
    {
        /// <summary>
        /// The name of the exported function. If not specified, the source function name is used.
        /// </summary>
        public string Name { get; set; }
    }
}
