﻿using Adjutant.Blam.Common;
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

        internal sealed class MetaViewerSettings : IPluginSettings
        {
            [Editor(typeof(BrowseFolderEditor), typeof(PropertyValueEditor))]
            [DisplayName("Plugins Folder")]
            public string PluginFolder { get; set; }

            [DisplayName("Show Invisibles")]
            public bool ShowInvisibles { get; set; }

            [DisplayName("Plugin Profiles")]
            public List<PluginProfile> PluginProfiles { get; set; }

            //cant use RuntimeDefaultValue because json seems to think it is always the default value and ignores it
            void IPluginSettings.ApplyDefaultValues(bool newInstance)
            {
                if (PluginProfiles != null)
                    return;

                PluginProfiles = new List<PluginProfile>
                {
                    new PluginProfile("Halo5", ModuleType.Halo5Server, ModuleType.Halo5Forge),
                    new PluginProfile("Halo2AMCC", CacheType.MccHalo2X),
                    new PluginProfile("Halo4MCC", CacheType.MccHalo4),
                    new PluginProfile("Halo4", CacheType.Halo4Retail, CacheType.MccHalo4, CacheType.MccHalo2X),
                    new PluginProfile("Halo4NetTest", CacheType.Halo4Beta),
                    new PluginProfile("ReachMCC", CacheType.MccHaloReach, CacheType.MccHaloReachU3),
                    new PluginProfile("Reach", CacheType.HaloReachRetail, CacheType.MccHaloReach, CacheType.MccHaloReachU3),
                    new PluginProfile("ReachBeta", CacheType.HaloReachBeta),
                    new PluginProfile("ODSTMCC", CacheType.MccHalo3ODST),
                    new PluginProfile("ODST", CacheType.Halo3ODST),
                    new PluginProfile("Halo3MCC", CacheType.MccHalo3, CacheType.MccHalo3U4),
                    new PluginProfile("Halo3", CacheType.Halo3Retail, CacheType.MccHalo3, CacheType.MccHalo3U4),
                    new PluginProfile("Halo3Beta", CacheType.Halo3Alpha, CacheType.Halo3Beta),
                    new PluginProfile("Halo2MCC", CacheType.MccHalo2),
                    new PluginProfile("Halo2", CacheType.Halo2Xbox, CacheType.Halo2Vista, CacheType.MccHalo2),
                    new PluginProfile("Halo1", CacheType.Halo1Xbox, CacheType.Halo1PC, CacheType.Halo1CE, CacheType.Halo1AE, CacheType.MccHalo1),
                };
            }
        }

        internal sealed class PluginProfile
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

            //json/propertygrid constructor
            public PluginProfile()
            { }

            //used for default settings
            public PluginProfile(string path, params CacheType[] builds)
            {
                FileNameFormat = "{0,-4}.xml";
                Subfolder = path;
                MapTypes = builds.ToList();
                ModuleTypes = new List<ModuleType>();
            }

            //used for default settings
            public PluginProfile(string path, params ModuleType[] builds)
            {
                FileNameFormat = "({0}){1}.xml";
                Subfolder = path;
                MapTypes = new List<CacheType>();
                ModuleTypes = builds.ToList();
            }

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