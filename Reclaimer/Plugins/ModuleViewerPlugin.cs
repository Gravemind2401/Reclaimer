using Microsoft.Win32;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Controls.Editors;
using Reclaimer.Utilities;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Plugins
{
    public class ModuleViewerPlugin : Plugin
    {
        private const string OpenFileKey = "ModuleViewer.OpenModuleFile";
        private const string OpenFolderKey = "ModuleViewer.OpenModuleFolder";
        private const string OpenFilePath = "File\\Open Module";
        private const string OpenFolderPath = "File\\Open Module Folder";
        private const string BrowseFileFilter = "Halo Module Files|*.module";
        private const string ModuleFileExtension = "module";

        private const string ModuleTagKeyRegex = @"Blam\.Halo(?:5\w+|Infinite)\..{2,}";

        internal static ModuleViewerSettings Settings;

        public override string Name => "Module Viewer";

        private PluginContextItem ExtractBinaryContextItem => new PluginContextItem("ExtractBinary", "Extract Tag Binary", OnContextItemClick);

        public override void Initialise() => Settings = LoadSettings<ModuleViewerSettings>();
        public override void Suspend() => SaveSettings(Settings);

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenFileKey, OpenFilePath, OpenFileMenuItemClick);
            yield return new PluginMenuItem(OpenFolderKey, OpenFolderPath, OpenFolderMenuItemClick);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            if (Regex.IsMatch(context.FileTypeKey, ModuleTagKeyRegex))
                yield return ExtractBinaryContextItem;
        }

        private void OpenFileMenuItemClick(string key)
        {
            if (key != OpenFileKey)
                return;

            var ofd = new OpenFileDialog
            {
                Filter = BrowseFileFilter,
                Multiselect = true,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(Settings.ModuleFolder))
                ofd.InitialDirectory = Settings.ModuleFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                OpenPhysicalFile(fileName);
        }

        private void OpenFolderMenuItemClick(string key)
        {
            if (key != OpenFolderKey)
                return;

            var ofd = new System.Windows.Forms.FolderBrowserDialog();

            if (!string.IsNullOrEmpty(Settings.ModuleFolder))
                ofd.InitialDirectory = Settings.ModuleFolder;

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var moduleFiles = FindModuleFiles(ofd.SelectedPath);
            if (moduleFiles.Count == 0)
            {
                Substrate.ShowErrorMessage("No module files were found in the selected directory or its subdirectories.");
                return;
            }

            var primary = moduleFiles[0];
            moduleFiles.RemoveAt(0);
            OpenPhysicalFile(primary, moduleFiles);
        }

        internal static List<string> FindModuleFiles(string directory)
        {
            //limit search depth to 4 - this is enough to select the "deploy" folder and have all modules included
            const int maxRecursionDepth = 4;

            //prioritise "common" modules first, then just the largest module file after that
            return new DirectoryInfo(directory)
                .EnumerateFiles("*.module", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = maxRecursionDepth })
                .OrderByDescending(f => f.Name.StartsWith("common"))
                .ThenByDescending(f => f.Length)
                .Select(f => f.FullName)
                .ToList();
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            var item = context.File.OfType<IModuleItem>().First();

            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = Utils.GetSafeFileName(item.FileName),
                Filter = "Binary Files|*.bin",
                FilterIndex = 1,
                AddExtension = true
            };

            if (sfd.ShowDialog() != true)
                return;

            using (var tagReader = item.CreateReader())
            using (var fs = File.Open(sfd.FileName, FileMode.Create))
                tagReader.BaseStream.CopyTo(fs);
        }

        public override bool SupportsFileExtension(string extension) => extension.ToLowerInvariant() == ModuleFileExtension;

        public override void OpenPhysicalFile(string fileName) => OpenPhysicalFile(fileName, null);

        private void OpenPhysicalFile(string fileName, IList<string> linkedFileNames)
        {
            var tabId = $"{Key}::{fileName}";
            if (Substrate.ShowTabById(tabId))
                return;

            var hasAdditionalFiles = linkedFileNames?.Count > 0;

            if (hasAdditionalFiles)
                LogOutput($"Loading module file (+{linkedFileNames.Count} linked): {fileName}");
            else
                LogOutput($"Loading module file: {fileName}");

            try
            {
                var mv = new Controls.ModuleViewer();
                mv.TabModel.ContentId = tabId;

                if (hasAdditionalFiles)
                    mv.LoadModulesAsLinked(fileName, linkedFileNames);
                else
                    mv.LoadModule(fileName);

                Substrate.AddTool(mv.TabModel, Substrate.GetHostWindow(), Dock.Left, new GridLength(400));
                Substrate.AddRecentFile(fileName);

                if (hasAdditionalFiles)
                    LogOutput($"Loaded module file (+{linkedFileNames.Count} linked): {fileName}");
                else
                    LogOutput($"Loaded module file: {fileName}");
            }
            catch (Exception ex)
            {
                var message = $"Unable to load {Path.GetFileName(fileName)}:{Environment.NewLine}{ex.Message}";
                Substrate.ShowErrorMessage(message);
                Substrate.LogError(message, ex);
            }
        }
    }

    internal class ModuleViewerSettings
    {
        [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
        [DisplayName("Modules Folder")]
        public string ModuleFolder { get; set; }

        [DisplayName("Hierarchy View")]
        public bool HierarchyView { get; set; }

        [DisplayName("Show Tag Resources")]
        public bool ShowTagResources { get; set; }

        [Editor(typeof(BrowseFileEditor), typeof(BrowseFileEditor))]
        [DisplayName("String IDs File")]
        public string StringIdFile { get; set; }
    }
}
