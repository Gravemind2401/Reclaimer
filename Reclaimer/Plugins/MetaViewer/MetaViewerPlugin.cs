using Reclaimer.Blam.Common;
using Reclaimer.Controls.Editors;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MetaViewer
{
    public class MetaViewerPlugin : Plugin
    {
        internal static MetaViewerSettings Settings { get; private set; }

        internal override int? FilePriority => 0;

        public override string Name => "Meta Viewer";

        public override void Initialise() => Settings = LoadSettings<MetaViewerSettings>();
        public override void Suspend() => SaveSettings(Settings);

        public override bool CanOpenFile(OpenFileArgs args)
        {
            var match = Regex.Match(args.FileTypeKey, @"Blam\.(\w+)\.(.*)");
            if (!match.Success)
                return false;

            if (Enum.TryParse(match.Groups[1].Value, out CacheType _))
            {
                var item = args.File.OfType<IIndexItem>().FirstOrDefault();
                if (item == null)
                    return false;

                try
                {
                    return !string.IsNullOrEmpty(GetDefinitionPath(item));
                }
                catch { return false; }
            }

            // Kinda hacky, want to implement something proper in the future.
            if (Enum.TryParse(match.Groups[1].Value, out ModuleType type))
            {
                string xml = null;

                if (type == ModuleType.Halo5Forge || type == ModuleType.Halo5Server)
                {
                    var item = args.File.OfType<Blam.Halo5.ModuleItem>().FirstOrDefault();
                    if (item != null)
                    {
                        xml = GetDefinitionPath(item);
                    }
                }
                else if (type == ModuleType.HaloInfinite)
                {
                    var item = args.File.OfType<Blam.HaloInfinite.ModuleItem>().FirstOrDefault();
                    if (item != null)
                    {
                        xml = GetDefinitionPath(item);
                    }
                }

                return xml != null && File.Exists(xml);
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
            else if (args.File.Any(i => i is Blam.Halo5.ModuleItem))
            {
                var item = args.File.OfType<Blam.Halo5.ModuleItem>().FirstOrDefault();
                var tabId = $"{Key}::{item.Module.FileName}::{item.GlobalTagId}";
                InitViewer(tabId, args, v => v.LoadMetadata(item, GetDefinitionPath(item)));
            }
            else if (args.File.Any(i => i is Blam.HaloInfinite.ModuleItem))
            {
                var item = args.File.OfType<Blam.HaloInfinite.ModuleItem>().FirstOrDefault();
                var tabId = $"{Key}::{item.Module.FileName}::{item.GlobalTagId}";
                InitViewer(tabId, args, v => v.LoadMetadata(item, GetDefinitionPath(item)));
            }
        }

        private static void InitViewer(string tabId, OpenFileArgs args, Action<Controls.MetaViewer> loadAction)
        {
            if (Substrate.ShowTabById(tabId))
                return;

            var viewer = new Controls.MetaViewer();
            viewer.TabModel.ContentId = tabId;

            loadAction(viewer);

            var container = args.TargetWindow.DocumentPanel;
            container.AddItem(viewer.TabModel);
        }

        private static string GetDefinitionPath(IIndexItem item) => GetDefinitionPath(p => p.ValidFor(item.CacheFile.CacheType), item.ClassCode, item.ClassName);
        private static string GetDefinitionPath(Blam.Halo5.ModuleItem item) => GetDefinitionPath(p => p.ValidFor(item.Module.ModuleType), item.ClassCode, item.ClassName);
        private static string GetDefinitionPath(Blam.HaloInfinite.ModuleItem item) => GetDefinitionPath(p => p.ValidFor(item.Module.ModuleType), item.ClassCode, item.ClassName);
        private static string GetDefinitionPath(Predicate<PluginProfile> validate, string classCode, string className)
        {
            if (string.IsNullOrEmpty(Settings.PluginFolder) || !Directory.Exists(Settings.PluginFolder) || classCode == null)
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

                    if (xmlName.Contains(' '))
                    {
                        //try again with no spaces
                        xmlName = xmlName.Replace(" ", "");
                        path = Path.Combine(Settings.PluginFolder, profile.Subfolder, xmlName);
                        if (File.Exists(path))
                            return path;
                    }
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
            [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
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

                var cacheTypes = Enum.GetValues(typeof(CacheType)).OfType<CacheType>();

                IEnumerable<CacheType> CacheTypesByPrefix(CacheType prefix) => cacheTypes.Where(t => t.ToString().StartsWith(prefix.ToString()));

                PluginProfiles = new List<PluginProfile>
                {
                    new PluginProfile("HaloInfinite", ModuleType.HaloInfinite),
                    new PluginProfile("Halo5", ModuleType.Halo5Server, ModuleType.Halo5Forge),
                    new PluginProfile("Halo2AMCC", CacheTypesByPrefix(CacheType.MccHalo2X)),
                    new PluginProfile("Halo4MCC", CacheTypesByPrefix(CacheType.MccHalo4)),
                    new PluginProfile("Halo4", CacheType.Halo4Retail, CacheType.MccHalo4, CacheType.MccHalo2X),
                    new PluginProfile("Halo4NetTest", CacheType.Halo4Beta),
                    new PluginProfile("ReachMCC", CacheTypesByPrefix(CacheType.MccHaloReach)),
                    new PluginProfile("Reach", CacheTypesByPrefix(CacheType.MccHaloReach).Prepend(CacheType.HaloReachRetail)),
                    new PluginProfile("ReachBeta", CacheType.HaloReachBeta),
                    new PluginProfile("ODSTMCC", CacheTypesByPrefix(CacheType.MccHalo3ODST)),
                    new PluginProfile("ODST", CacheTypesByPrefix(CacheType.MccHalo3ODST).Prepend(CacheType.Halo3ODST)),
                    new PluginProfile("Halo3MCC", CacheTypesByPrefix(CacheType.MccHalo3).Where(t => !t.ToString().StartsWith(nameof(CacheType.MccHalo3ODST)))),
                    new PluginProfile("Halo3", CacheTypesByPrefix(CacheType.MccHalo3).Where(t => !t.ToString().StartsWith(nameof(CacheType.MccHalo3ODST))).Prepend(CacheType.Halo3Retail)),
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

            [Editor(typeof(EnumMultiSelectEditor), typeof(EnumMultiSelectEditor))]
            [DisplayName("Map Types")]
            public List<CacheType> MapTypes { get; set; }

            [Editor(typeof(EnumMultiSelectEditor), typeof(EnumMultiSelectEditor))]
            [DisplayName("Module Types")]
            public List<ModuleType> ModuleTypes { get; set; }

            //json/propertygrid constructor
            public PluginProfile()
            { }

            //used for default settings
            public PluginProfile(string path, params CacheType[] builds)
                : this(path, builds.AsEnumerable()) { }

            public PluginProfile(string path, IEnumerable<CacheType> builds)
            {
                FileNameFormat = "{0,-4}.xml";
                Subfolder = path;
                MapTypes = builds.ToList();
                ModuleTypes = new List<ModuleType>();
            }

            //used for default settings
            public PluginProfile(string path, params ModuleType[] builds)
                : this(path, builds.AsEnumerable()) { }

            public PluginProfile(string path, IEnumerable<ModuleType> builds)
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