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
    public partial class ModuleViewerPlugin : Plugin
    {
        internal sealed record TagBinaryExport(IModuleItem Item, string RelativePath);
        private delegate int QueueModuleContent(IReadOnlyCollection<IModuleItem> items, string outputFolder);

        private const string OpenFileKey = "ModuleViewer.OpenModuleFile";
        private const string OpenFolderKey = "ModuleViewer.OpenModuleFolder";
        private const string OpenFilePath = "File\\Open Module";
        private const string OpenFolderPath = "File\\Open Module Folder";
        private const string BrowseFileFilter = "Halo Module Files|*.module";
        private const string ModuleFileExtension = "module";

        [GeneratedRegex(@"Blam\.Halo(?:5\w+|Infinite)\..{2,}")]
        private static partial Regex RxModuleFilePattern();

        internal static ModuleViewerSettings Settings;

        public override string Name => "Module Viewer";

        private PluginContextItem ExtractBinaryContextItem => new PluginContextItem("ExtractBinary", "Extract Tag Binary", OnContextItemClick);
        private PluginContextItem ExtractHalo5AudioContextItem => new PluginContextItem("ExtractHalo5Audio", "Extract Halo 5 Audio", OnContextItemClick);

        public override void Initialise()
        {
            Settings = LoadSettings<ModuleViewerSettings>();
            if (string.IsNullOrWhiteSpace(Settings.VgmstreamPath))
            {
                var bundledVgmstream = Path.Combine(AppContext.BaseDirectory, "vgmstream-cli.exe");
                if (File.Exists(bundledVgmstream))
                    Settings.VgmstreamPath = bundledVgmstream;
            }
        }
        public override void Suspend() => SaveSettings(Settings);

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem(OpenFileKey, OpenFilePath, OpenFileMenuItemClick);
            yield return new PluginMenuItem(OpenFolderKey, OpenFolderPath, OpenFolderMenuItemClick);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            if (!RxModuleFilePattern().IsMatch(context.FileTypeKey))
                yield break;

            var itemCount = context.File.OfType<IModuleItem>().Count();
            if (itemCount == 1)
            {
                yield return ExtractBinaryContextItem;
                if (context.File.OfType<Reclaimer.Blam.Halo5.ModuleItem>().Any(item => item.ClassCode is "snd!" or "sbnk" or "lsnd" or "musc"))
                    yield return ExtractHalo5AudioContextItem;
            }
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
            var items = context.File.OfType<IModuleItem>().Distinct().ToList();
            var item = items.First();

            if (key == "ExtractHalo5Audio")
            {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select a folder for the extracted Halo 5 audio",
                    UseDescriptionForTitle = true
                };
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                var result = Halo5AudioExporter.Export(items, dialog.SelectedPath, Settings.VgmstreamPath, Settings.Halo5AudioLanguage);
                foreach (var failure in result.Failures)
                    Substrate.LogError(failure, new InvalidOperationException(failure));
                MessageBox.Show(
                    $"Processed {result.Banks} banks and {result.Media} media sources from {result.Tags} Halo 5 audio tags to:{Environment.NewLine}{dialog.SelectedPath}"
                        + (result.Failures.Count == 0 ? string.Empty : $"{Environment.NewLine}{Environment.NewLine}Failed: {result.Failures.Count}. See the output log for details."),
                    Application.Current.MainWindow.Title,
                    MessageBoxButton.OK,
                    result.Failures.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
                return;
            }

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

        internal static void ExtractSelectedBinaries(IReadOnlyCollection<TagBinaryExport> items, bool includeSupportedContent = false)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a folder for the extracted tag binaries",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var reservedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failures = new List<(IModuleItem Item, Exception Error)>();
            var successCount = 0;

            foreach (var export in items)
            {
                var item = export.Item;
                string outputPath = null;
                var outputCreated = false;
                try
                {
                    outputPath = GetAvailableBinaryPath(dialog.SelectedPath, export.RelativePath, reservedPaths);
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    using var tagReader = item.CreateReader();
                    using var fs = File.Open(outputPath, FileMode.CreateNew, FileAccess.Write);
                    outputCreated = true;
                    tagReader.BaseStream.CopyTo(fs);
                    successCount++;
                }
                catch (Exception ex)
                {
                    if (outputCreated)
                    {
                        try { File.Delete(outputPath); }
                        catch (Exception cleanupError) { Substrate.LogError($"Unable to remove incomplete tag binary '{outputPath}'.", cleanupError); }
                    }

                    failures.Add((item, ex));
                    Substrate.LogError($"Unable to extract tag binary '{item.FileName}'.", ex);
                }
            }

            var summary = $"Extracted {successCount} of {items.Count} tag binaries to:{Environment.NewLine}{dialog.SelectedPath}";
            if (failures.Count > 0)
                summary += $"{Environment.NewLine}{Environment.NewLine}Failed: {failures.Count}. See the output log for details.";

            if (includeSupportedContent)
            {
                var queueContent = Substrate.GetSharedFunction<QueueModuleContent>(Constants.SharedFuncQueueModuleContent);
                var queuedCount = queueContent?.Invoke(items.Select(export => export.Item).Distinct().ToList(), dialog.SelectedPath) ?? 0;
                summary += $"{Environment.NewLine}{Environment.NewLine}Queued {queuedCount} supported image/geometry/sound items for normal extraction.";

                var audioResult = Halo5AudioExporter.Export(items.Select(export => export.Item), dialog.SelectedPath, Settings.VgmstreamPath, Settings.Halo5AudioLanguage);
                if (audioResult.Tags > 0)
                {
                    summary += $"{Environment.NewLine}Halo 5 audio: processed {audioResult.Banks} banks and {audioResult.Media} media sources from {audioResult.Tags} tags.";
                    if (audioResult.Failures.Count > 0)
                    {
                        summary += $" {audioResult.Failures.Count} failed; see the output log.";
                        foreach (var failure in audioResult.Failures)
                            Substrate.LogError(failure, new InvalidOperationException(failure));
                    }
                }
            }

            MessageBox.Show(
                summary,
                Application.Current.MainWindow.Title,
                MessageBoxButton.OK,
                failures.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        internal static string GetAvailableBinaryPath(string outputFolder, string relativePath, ISet<string> reservedPaths)
        {
            var segments = (relativePath ?? string.Empty)
                .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
                .Select(GetSafePathSegment)
                .ToList();

            var safeName = segments.Count == 0 ? "tag" : segments[^1];
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "tag";

            var relativeFolder = segments.Count <= 1
                ? string.Empty
                : Path.Combine(segments.Take(segments.Count - 1).ToArray());
            var baseName = Path.GetFileNameWithoutExtension(safeName);
            var destinationFolder = Path.Combine(outputFolder, relativeFolder);
            var candidate = Path.Combine(destinationFolder, $"{baseName}.bin");
            var suffix = 2;

            while (File.Exists(candidate) || !reservedPaths.Add(candidate))
                candidate = Path.Combine(destinationFolder, $"{baseName} ({suffix++}).bin");

            return candidate;
        }

        private static string GetSafePathSegment(string segment)
        {
            var safeSegment = Utils.GetSafeFileName(segment);
            return string.IsNullOrWhiteSpace(safeSegment) || safeSegment is "." or ".."
                ? "_"
                : safeSegment;
        }

        public override bool SupportsFileExtension(string extension) => extension.ToLowerInvariant() == ModuleFileExtension;

        public override void OpenPhysicalFile(string fileName) => OpenPhysicalFile(fileName, null);

        private void OpenPhysicalFile(string fileName, List<string> linkedFileNames)
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

        [Editor(typeof(BrowseFileEditor), typeof(BrowseFileEditor))]
        [DisplayName("vgmstream CLI Path")]
        [Description("Optional path to vgmstream-cli.exe. Halo 5 TXTP exports are converted to WAV when configured.")]
        public string VgmstreamPath { get; set; }

        [DisplayName("Halo 5 Audio Language")]
        [Description("Language package folder used for localized Halo 5 voice audio, for example English(US).")]
        public string Halo5AudioLanguage { get; set; } = "English(US)";
    }
}
