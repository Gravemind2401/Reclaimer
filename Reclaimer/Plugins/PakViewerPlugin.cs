﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class PakViewerPlugin : Plugin
    {
        const string OpenKey = "PakViewer.OpenPak";
        const string OpenPath = "File\\Open Pak";

        internal static PakViewerSettings Settings;

        public override string Name => "Pak Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<PakViewerSettings>();
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenKey, OpenPath, OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            if (key != OpenKey) return;

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Saber Pak Files|*.s3dpak",
                Multiselect = true,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(Settings.PakFolder))
                ofd.InitialDirectory = Settings.PakFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                OpenPhysicalFile(fileName);
        }

        public override bool SupportsFileExtension(string extension)
        {
            return extension.ToLower() == "s3dpak";
        }

        public override void OpenPhysicalFile(string fileName)
        {
            LogOutput($"Loading pak file: {fileName}");

            var pv = new Controls.PakViewer();
            pv.LoadPak(fileName);
            Substrate.AddTool(pv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));

            LogOutput($"Loaded pak file: {fileName}");
        }
    }

    internal class PakViewerSettings
    {
        public string PakFolder { get; set; }
    }
}
