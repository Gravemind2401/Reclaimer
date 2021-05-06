using Adjutant.Blam.Common;
using Adjutant.Blam.Halo5;
using Reclaimer.Controls.Editors;
using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MetaViewer
{
    public class MetaViewerPlugin : Plugin
    {
        internal static MetaViewerSettings Settings { get; private set; }

        internal override int? FilePriority => 0;

        public override string Name => "Meta Viewer";

        public override void Initialise()
        {
            Settings = LoadSettings<MetaViewerSettings>();
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        public override bool CanOpenFile(OpenFileArgs args)
        {
            var match = Regex.Match(args.FileTypeKey, @"Blam\.(\w+)\.(.*)");
            if (!match.Success) return false;

            CacheType cacheType;
            if (Enum.TryParse(match.Groups[1].Value, out cacheType))
            {
                var item = args.File.OfType<IIndexItem>().FirstOrDefault();
                if (item == null) return false;

                try
                {
                    return !string.IsNullOrEmpty(GetDefinitionPath(item));
                }
                catch { return false; }
            }

            ModuleType moduleType;
            if (Enum.TryParse(match.Groups[1].Value, out moduleType))
            {
                var item = args.File.OfType<ModuleItem>().FirstOrDefault();
                if (item == null) return false;

                var xml = GetDefinitionPath(item);
                return File.Exists(xml);
            }

            return false;
        }

        public override void OpenFile(OpenFileArgs args)
        {
            if (args.File.Any(i => i is IIndexItem))
            {
                var item = args.File.OfType<IIndexItem>().FirstOrDefault();
                var tabId = $"{Key}::{item.CacheFile.FileName}::{item.Id}";
                InitViewer(tabId, args, v => v.LoadMetadata(item, GetDefinitionPath(item)));
            }
            else if (args.File.Any(i => i is ModuleItem))
            {
                var item = args.File.OfType<ModuleItem>().FirstOrDefault();
                var tabId = $"{Key}::{item.Module.FileName}::{item.GlobalTagId}";
                InitViewer(tabId, args, v => v.LoadMetadata(item, GetDefinitionPath(item)));
            }
        }

        private void InitViewer(string tabId, OpenFileArgs args, Action<Controls.MetaViewer> loadAction)
        {
            if (Substrate.ShowTabById(tabId))
                return;

            var viewer = new Controls.MetaViewer();
            viewer.TabModel.ContentId = tabId;

            loadAction(viewer);

            var container = args.TargetWindow.DocumentPanel;
            container.AddItem(viewer.TabModel);
        }

        private string GetDefinitionPath(IIndexItem item)
        {
            return GetDefinitionPath(p => p.ValidFor(item.CacheFile.CacheType), item.ClassCode, item.ClassName);
        }

        private string GetDefinitionPath(ModuleItem item)
        {
            return GetDefinitionPath(p => p.ValidFor(item.Module.ModuleType), item.ClassCode, item.ClassName);
        }


        private string GetDefinitionPath(Predicate<PluginProfile> validate, string classCode, string className)
        {
            if (string.IsNullOrEmpty(Settings.PluginFolder) || !Directory.Exists(Settings.PluginFolder))
                return null;

            foreach (var profile in Settings.PluginProfiles.Where(p => validate(p)))
            {
                try
                {
                    var safeCode = string.Join("_", classCode.Split(Path.GetInvalidFileNameChars())).Trim();
                    var xmlName = string.Format(profile.FileNameFormat, safeCode, className);
                    var path = Path.Combine(Settings.PluginFolder, profile.Subfolder, xmlName);
                    if (File.Exists(path))
                        return path;
                }
                catch (Exception ex)
                {
                    Substrate.LogError($"Error validating plugin profile '{profile.Subfolder}'", ex);
                }
            }

            return null;
        }

        internal class MetaViewerSettings
        {
            [Editor(typeof(BrowseFolderEditor), typeof(PropertyValueEditor))]
            [DisplayName("Plugins Folder")]
            public string PluginFolder { get; set; }

            [DisplayName("Show Invisibles")]
            public bool ShowInvisibles { get; set; }

            [DisplayName("Plugin Profiles")]
            public List<PluginProfile> PluginProfiles { get; set; }
        }

        internal class PluginProfile
        {
            [DisplayName("Subfolder")]
            public string Subfolder { get; set; }

            [DisplayName("Filename Format")]
            public string FileNameFormat { get; set; } //0 = code, 1 = name

            [Editor(typeof(EnumMultiSelectEditor), typeof(PropertyValueEditor))]
            [EditorOption(Name = "CollectionType", Value = typeof(CacheType))]
            [DisplayName("Map Types")]
            public List<CacheType> MapTypes { get; set; }

            [Editor(typeof(EnumMultiSelectEditor), typeof(PropertyValueEditor))]
            [EditorOption(Name = "CollectionType", Value = typeof(ModuleType))]
            [DisplayName("Module Types")]
            public List<ModuleType> ModuleTypes { get; set; }

            public PluginProfile()
            { }

            public bool ValidFor(CacheType cacheType)
            {
                return !string.IsNullOrEmpty(Subfolder)
                    && MapTypes?.Contains(cacheType) == true;
            }

            public bool ValidFor(ModuleType moduleType)
            {
                return !string.IsNullOrEmpty(Subfolder)
                    && ModuleTypes?.Contains(moduleType) == true;
            }

            public override string ToString() => Subfolder;
        }
    }
}