using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public delegate void MenuItemClickHandler(string key);

    public class PluginMenuItem
    {
        private readonly string key;
        private readonly MenuItemClickHandler handler;

        public string Path { get; }

        public void ExecuteHandler() => handler(key);

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

    public abstract class Plugin
    {
        internal readonly List<LogEntry> logEntries = new List<LogEntry>();

        public Plugin() { }

        internal string Assembly => GetType().Assembly.FullName;

        internal virtual string Key => GetType().FullName;

        public abstract string Name { get; }

        public virtual void Initialise() { }

        public virtual void Suspend() { }

        protected T LoadSettings<T>() where T : new()
        {
            return Substrate.GetPluginSettings<T>(Key);
        }

        protected void SaveSettings<T>(T settings) where T : new()
        {
            Substrate.SavePluginSettings(Key, settings);
        }

        public virtual bool CanOpenFile(OpenFileArgs args) => false;

        public virtual void OpenFile(OpenFileArgs args) { }

        public virtual IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield break;
        }


        //themes (including existing)

        //context items for files

        //on context item click

        protected internal void LogOutput(string message)
        {
            var entry = new LogEntry(DateTime.Now, message);
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry);
        }

        protected internal void LogError(string message, Exception e)
        {
            var entry = new LogEntry(DateTime.Now, $"{message}{Environment.NewLine}{e.ToString()}");
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry);
        }

        protected internal void ClearLog()
        {
            logEntries.Clear();
        }
    }
}
