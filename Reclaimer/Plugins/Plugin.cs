using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public struct PluginMenuItem
    {
        public string Key { get; }
        public string Path { get; }

        public PluginMenuItem(string key, string path)
        {
            Key = key;
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

        public virtual bool CanOpenFile(object file, string key) => false;

        public virtual void OpenFile(object file, string key, IMultiPanelHost targetWindow) { }

        public virtual IEnumerable<PluginMenuItem> MenuItems
        {
            get { yield break; }
        }

        public virtual void OnMenuItemClick(string key) { }

        //themes (including existing)

        //context items for files

        //on context item click

        protected internal void LogOutput(string message)
        {
            var entry = new LogEntry(DateTime.Now, message);
            logEntries.Add(entry);
            Substrate.LogOutput(this, entry);
        }

        protected internal void ClearLog()
        {
            logEntries.Clear();
        }
    }
}
