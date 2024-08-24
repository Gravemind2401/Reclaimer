﻿using Reclaimer.Utilities;

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
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }

    /// <summary>
    /// Represents a context menu item that will be displayed for a file object.
    /// </summary>
    public class PluginContextItem
    {
        private readonly ContextItemClickHandler handler;

        /// <summary>
        /// The unique key associated with this menu item.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the menu path to the context menu item.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Executes the handler associated with the context menu item.
        /// </summary>
        /// <param name="context">The file object that the context menu is attached to.</param>
        public void ExecuteHandler(OpenFileArgs context) => handler(Key, context);

        /// <summary>
        /// Creates a new instance of the <see cref="PluginContextItem"/> class.
        /// </summary>
        /// <param name="key">The unique key associated with this menu item.</param>
        /// <param name="path">The menu path to this menu item.</param>
        /// <param name="handler">The method to execute when the context menu item is clicked.</param>
        public PluginContextItem(string key, string path, ContextItemClickHandler handler)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
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

        internal DateTime WorkingStatusTime { get; private set; }

        internal object settings { get; set; }

        private string workingStatus;
        internal string WorkingStatus
        {
            get => workingStatus;
            private set
            {
                if (workingStatus != value)
                {
                    workingStatus = value;
                    WorkingStatusTime = DateTime.Now;
                }
            }
        }

        internal virtual int? FilePriority => null;

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// <para>When overidden in a derived class, performs any initialisation required by the plugin.</para>
        /// <para>Any initialisation that does not depend on another plugin should be done here.</para>
        /// <para>This method is called by the <see cref="Substrate"/> when the plugin is first loaded.</para>
        /// </summary>
        public virtual void Initialise() { }

        /// <summary>
        /// <para>When overidden in a derived class, performs any additional initialisation required by the plugin.</para>
        /// <para>Any initialisation that depends on another plugin should be done here.</para>
        /// <para>This method is called by the <see cref="Substrate"/> after all plugins have been loaded.</para>
        /// </summary>
        public virtual void PostInitialise() { }

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
            var result = Substrate.GetPluginSettings<T>(Key);
            settings = result;
            return result;
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
        /// Updates the working status that appears in the statusbar.
        /// </summary>
        /// <param name="status">The status to display.</param>
        protected internal void SetWorkingStatus(string status)
        {
            Exceptions.ThrowIfNullOrWhiteSpace(status);

            WorkingStatus = status;
            Substrate.RaiseWorkingStatusChanged(this);
        }

        /// <summary>
        /// Removes this plugin's working status from the status bar.
        /// </summary>
        protected internal void ClearWorkingStatus()
        {
            WorkingStatus = null;
            Substrate.RaiseWorkingStatusChanged(this);
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
            Substrate.LogOutput(this, entry, false);
        }

        /// <summary>
        /// Logs an error to the output pane with a related message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="e">The error to log.</param>
        protected internal void LogError(string message, Exception e) => LogError(message, e, false);

        /// <summary>
        /// Logs an error to the output pane with a related message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="e">The error to log.</param>
        /// <param name="focusOutput">true to focus the output window.</param>
        protected internal void LogError(string message, Exception e, bool focusOutput)
        {
            var entry = new LogEntry(DateTime.Now, $"{message}{Environment.NewLine}{e}");
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry, focusOutput);
        }

        /// <summary>
        /// Clears the current plugin's log message from the output pane.
        /// </summary>
        protected internal void ClearLog()
        {
            logEntries.Clear();
            Substrate.ClearLogOutput(this);
        }
    }
}
