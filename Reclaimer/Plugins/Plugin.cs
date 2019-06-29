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
        private readonly Dictionary<DateTime, string> log = new Dictionary<DateTime, string>();

        public Plugin() { }

        internal string Key => GetType().FullName;

        public abstract string Name { get; }

        public virtual void Initialise() { }

        public virtual void Suspend() { }

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

        protected void LogOutput(string message)
        {
            log.Add(DateTime.Now, message);
            Substrate.LogOutput(this, message);
        }

        protected internal void ClearLog()
        {
            log.Clear();
        }
    }
}
