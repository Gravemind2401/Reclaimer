using Reclaimer.Annotations;
using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo5;
using Reclaimer.Controls.Editors;
using Reclaimer.Drawing;
using Reclaimer.Geometry;
using Reclaimer.Models;
using Reclaimer.Saber3D.Common;
using Reclaimer.Utilities;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

using BlamContentFactory = Reclaimer.Blam.Common.ContentFactory;
using SaberContentFactory = Reclaimer.Saber3D.Common.ContentFactory;

namespace Reclaimer.Plugins
{
    public class BatchExtractPlugin : Plugin
    {
        private const string BlamFileRegex = @"Blam\.(\w+)\.(.*)";
        private const string SaberFileRegex = @"Saber3D\.(\w+)\.(.*)";
        private const string FileKeyWildcard = "*";

        private ConcurrentQueue<IExtractable> extractionQueue = new ConcurrentQueue<IExtractable>();

        private bool isBusy;
        private CancellationTokenSource tokenSource;
        private string lastDataFolder;

        private delegate string GetModelExtension(string formatId);
        private Lazy<GetModelExtension> getModelExtensionFunc;

        private delegate void WriteModelFile(IContentProvider<Scene> provider, string fileName, string formatId);
        private WriteModelFile writeModelFileFunc;

        private delegate bool WriteSoundFile(GameSound sound, string directory, bool overwrite);
        private WriteSoundFile writeSoundFileFunc;

        public override string Name => "Batch Extractor";

        private BatchExtractSettings Settings { get; set; }

        private PluginContextItem ExtractMultipleContextItem => new PluginContextItem("ExtractAll", isBusy ? "Add to extraction queue" : "Extract All", OnContextItemClick);
        private PluginContextItem ExtractSingleContextItem => new PluginContextItem("Extract", isBusy ? "Add to extraction queue" : "Extract", OnContextItemClick);

        public override void Initialise()
        {
            Settings = LoadSettings<BatchExtractSettings>();
            Settings.DataFolder = Settings.DataFolder.Replace(Constants.PluginsFolderToken, Substrate.PluginsDirectory, StringComparison.OrdinalIgnoreCase);
            getModelExtensionFunc = new Lazy<GetModelExtension>(() => Substrate.GetSharedFunction<GetModelExtension>(Constants.SharedFuncGetModelExtension));
        }

        public override void PostInitialise()
        {
            writeModelFileFunc = Substrate.GetSharedFunction<WriteModelFile>(Constants.SharedFuncWriteModelFile);
            writeSoundFileFunc = Substrate.GetSharedFunction<WriteSoundFile>(Constants.SharedFuncWriteSoundFile);
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem("Cancel", "Tools\\Cancel Batch Extract", OnMenuItemClick);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            Match match;
            if ((match = Regex.Match(context.FileTypeKey, BlamFileRegex)).Success)
            {
                if (!ValidateCacheType(match.Groups[1].Value))
                    yield break;

                if (match.Groups[2].Value == FileKeyWildcard && context.File.Any(i => i is TreeItemModel))
                    yield return ExtractMultipleContextItem;
                else
                {
                    if (context.File.Any(IsExtractable))
                        yield return ExtractSingleContextItem;
                }
            }
            else if ((match = Regex.Match(context.FileTypeKey, SaberFileRegex)).Success)
            {
                if (match.Groups[1].Value != "Halo1X")
                    yield break;

                if (match.Groups[2].Value == FileKeyWildcard && context.File.Any(i => i is TreeItemModel))
                    yield return ExtractMultipleContextItem;
                else
                {
                    if (context.File.Any(IsExtractable))
                        yield return ExtractSingleContextItem;
                }
            }
        }

        public override void Suspend()
        {
            Settings.DataFolder = Settings.DataFolder.Replace(Substrate.PluginsDirectory, Constants.PluginsFolderToken, StringComparison.OrdinalIgnoreCase);
            SaveSettings(Settings);
        }

        private static bool ValidateCacheType(string name) => Enum.TryParse(name, out CacheType _) || Enum.TryParse(name, out ModuleType _);

        private bool IsExtractable(object obj) => GetExtractable(obj, Settings.DataFolder) != null;

        private static IExtractable GetExtractable(object obj, string outputFolder)
        {
            IExtractable extractable;
            if (obj is IIndexItem)
                extractable = new CacheExtractable(obj as IIndexItem, outputFolder);
            else if (obj is ModuleItem)
                extractable = new ModuleExtractable(obj as ModuleItem, outputFolder);
            else if (obj is IPakItem)
                extractable = new PakExtractable(obj as IPakItem, outputFolder);
            else
                extractable = null;

            if (extractable?.GetContentType() >= 0)
                return extractable;

            return null;
        }

        [SharedFunction]
        private bool GetDataFolder(out string dataFolder)
        {
            dataFolder = Settings.DataFolder;
            if (Settings.PromptForFolder)
            {
                var fsd = new FolderBrowserDialog();
                if (!string.IsNullOrEmpty(dataFolder))
                    fsd.InitialDirectory = dataFolder;

                if (fsd.ShowDialog() != DialogResult.OK)
                    return false;

                dataFolder = fsd.SelectedPath;

                if (Settings.AutoDataFolder)
                    Settings.DataFolder = dataFolder;
            }

            return true;
        }

        private void OnMenuItemClick(string key)
        {
            if (!isBusy)
            {
                MessageBox.Show("Nothing in progress");
                return;
            }

            if (!tokenSource.IsCancellationRequested)
                tokenSource.Cancel();

            extractionQueue = new ConcurrentQueue<IExtractable>();
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            if (!isBusy)
            {
                if (!GetDataFolder(out lastDataFolder))
                    return;
            }

            var node = context.File.OfType<TreeItemModel>().FirstOrDefault();
            if (node != null)
                BatchQueue(node, lastDataFolder);
            else
            {
                var item = context.File.Select(f => GetExtractable(f, lastDataFolder)).FirstOrDefault(e => e != null);
                if (item != null)
                    extractionQueue.Enqueue(item);
            }

            if (isBusy)
                return;

            tokenSource = new CancellationTokenSource();

            Task.Run(ProcessQueueAsync, tokenSource.Token);
        }

        private void BatchQueue(TreeItemModel node, string outputFolder)
        {
            if (node.HasItems)
            {
                foreach (var child in node.Items)
                    BatchQueue(child, outputFolder);
            }
            else
            {
                var item = GetExtractable(node.Tag, outputFolder);
                if (item != null)
                    extractionQueue.Enqueue(item);
            }
        }

        private async Task ProcessQueueAsync()
        {
            isBusy = true;

            try
            {
                var nextWorkerId = 0;
                var locker = new KeyedSemaphore();
                var counter = new ExtractCounter();
                var start = DateTime.Now;

                //note that multiple workers can result in the working status (status bar text)
                //being inaccurate - each tag will only set the status once so if a slow tag
                //sets the status and a fast tag replaces it then the status will not update
                //back to the slow tag after the fast one finishes.

                async Task Process()
                {
                    var prefix = Settings.BatchWorkerCount > 1 ? $"[Worker {nextWorkerId++}] " : string.Empty;

                    while (!extractionQueue.IsEmpty)
                    {
                        if (tokenSource.IsCancellationRequested)
                            break;

                        if (!extractionQueue.TryDequeue(out var item))
                            break;

                        using (await locker.WaitAsync(item.ItemKey))
                        {
                            SetWorkingStatus($"{prefix}Extracting {item.DisplayName}");
                            Extract(item, counter);
                        }
                    }
                }

                var processors = Enumerable.Range(0, Settings.BatchWorkerCount).Select(i => Task.Run(Process)).ToList();
                await Task.WhenAll(processors);

                var span = DateTime.Now - start;
                LogOutput($"Extracted {counter.Extracted} tags in {Math.Round(span.TotalSeconds)} seconds with {counter.Errors} errors.");
            }
            catch (Exception ex)
            {
                Substrate.LogError("Error during batch extraction", ex);
            }
            finally
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            isBusy = false;
            ClearWorkingStatus();
        }

        private void Extract(IExtractable item, ExtractCounter counter)
        {
            try
            {
                switch (item.GetContentType())
                {
                    case 0:
                        if (SaveImage(item))
                            counter.Extracted++;
                        break;

                    case 1:
                        if (writeModelFileFunc != null && SaveModel(item))
                            counter.Extracted++;
                        break;

                    case 2:
                        if (writeSoundFileFunc != null && SaveSound(item))
                            counter.Extracted++;
                        break;
                }
            }
            catch (Exception e)
            {
                LogError($"Error extracting {item.DisplayName}", e);
                counter.Errors++;
            }
        }

        #region Images
        private bool SaveImage(IExtractable item)
        {
            if (item.GetBitmapContent(out var provider) && SaveImage(provider, item.Destination))
            {
                LogOutput($"Extracted {item.DisplayName}");
                return true;
            }

            return false;
        }

        [SharedFunction]
        private bool SaveImage(IContentProvider<IBitmap> provider, string baseDir)
        {
            var bitmap = provider.GetContent();
            var extracted = 0;
            for (var i = 0; i < bitmap.SubmapCount; i++)
            {
                var fileName = MakePath(provider.Class, provider.Name, baseDir);
                var ext = "." + Settings.BitmapFormat.ToString().ToLower();

                if (bitmap.SubmapCount > 1)
                    fileName += $"[{i}]";

                if (Settings.BitmapFormat == BitmapFormat.DDS)
                {
                    if (Settings.OverwriteExisting || !File.Exists(fileName + ext))
                    {
                        var rawDds = bitmap.ToDds(i);
                        rawDds.WriteToDxgi(fileName + ext);
                        extracted++;
                    }
                    continue;
                }

                var format = Settings.BitmapFormat switch
                {
                    BitmapFormat.PNG => ImageFormat.Png,
                    BitmapFormat.TIF => ImageFormat.Tiff,
                    BitmapFormat.JPEG => ImageFormat.Jpeg,
                    BitmapFormat.TGA => null,
                    _ => null
                };

                var outputs = new List<(string FileName, DecompressOptions Options)>();

                if (Settings.BitmapMode == BitmapMode.Default)
                    outputs.Add((fileName + ext, DecompressOptions.Default));
                else if (Settings.BitmapMode == BitmapMode.Bgr24)
                    outputs.Add((fileName + ext, DecompressOptions.Bgr24));
                else if (Settings.BitmapMode == BitmapMode.IsolateAlpha)
                    outputs.AddRange(GetParamsIsolateAlpha(fileName, ext));
                else if (Settings.BitmapMode == BitmapMode.IsolateAll)
                    outputs.AddRange(GetParamsIsolateAll(fileName, ext));
                else if (Settings.BitmapMode == BitmapMode.MixedIsolate)
                    outputs.AddRange(GetParamsMixedIsolate(fileName, ext));

                DdsImage dds = null;
                foreach (var (f, o) in outputs)
                {
                    if (!Settings.OverwriteExisting && File.Exists(f))
                        continue;

                    dds ??= bitmap.ToDds(i);

                    var args = new DdsOutputArgs(o, bitmap.CubeLayout);
                    if (format != null)
                        dds.WriteToDisk(f, format, args);
                    else //if (Settings.BitmapFormat == BitmapFormat.TGA)
                        dds.WriteToTarga(f, args);

                    extracted++;
                }
            }

            return extracted > 0;
        }

        private static IEnumerable<(string FileName, DecompressOptions Options)> GetParamsIsolateAlpha(string fileName, string extension)
        {
            yield return ($"{fileName}_hue{extension}", DecompressOptions.Bgr24);
            yield return ($"{fileName}_alpha{extension}", DecompressOptions.AlphaChannelOnly);
        }

        private static IEnumerable<(string FileName, DecompressOptions Options)> GetParamsIsolateAll(string fileName, string extension)
        {
            yield return ($"{fileName}_blue{extension}", DecompressOptions.BlueChannelOnly);
            yield return ($"{fileName}_green{extension}", DecompressOptions.GreenChannelOnly);
            yield return ($"{fileName}_red{extension}", DecompressOptions.RedChannelOnly);
            yield return ($"{fileName}_alpha{extension}", DecompressOptions.AlphaChannelOnly);
        }

        private static readonly string[] shouldIsolate = new[] { "([_ ]multi)$", "([_ ]multipurpose)$", "([_ ]cc)$" };
        private static IEnumerable<(string FileName, DecompressOptions Options)> GetParamsMixedIsolate(string fileName, string extension)
        {
            var imageName = fileName.Split('\\').Last();
            if (imageName.EndsWith("]"))
                imageName = imageName[..imageName.LastIndexOf('[')];

            return shouldIsolate.Any(s => Regex.IsMatch(imageName, s, RegexOptions.IgnoreCase))
                ? GetParamsIsolateAll(fileName, extension)
                : GetParamsIsolateAlpha(fileName, extension);
        }
        #endregion

        #region Models
        private bool SaveModel(IExtractable item)
        {
            if (item.GetGeometryContent(out var provider) && SaveModel(provider, item.Destination))
            {
                LogOutput($"Extracted {item.DisplayName}");
                return true;
            }

            return false;
        }

        [SharedFunction]
        private bool SaveModel(IContentProvider<Scene> provider, string baseDir)
        {
            var fileName = MakePath(provider.Class, provider.Name, baseDir);
            var ext = getModelExtensionFunc?.Value(Settings.ModelFormat);

            if (!Settings.OverwriteExisting && File.Exists($"{fileName}.{ext}"))
                return false;

            writeModelFileFunc?.Invoke(provider, fileName, Settings.ModelFormat);
            return true;
        }
        #endregion

        #region Sounds
        private bool SaveSound(IExtractable item)
        {
            if (item.GetSoundContent(out var provider) && SaveSound(provider, item.Destination))
            {
                LogOutput($"Extracted {item.DisplayName}");
                return true;
            }

            return false;
        }

        [SharedFunction]
        private bool SaveSound(IContentProvider<GameSound> provider, string baseDir)
        {
            var dir = Path.GetDirectoryName(MakePath(provider.Class, provider.Name, baseDir));
            return writeSoundFileFunc?.Invoke(provider.GetContent(), dir, Settings.OverwriteExisting) ?? false;
        }
        #endregion

        private string MakePath(string tagClass, string tagPath, string baseDirectory)
        {
            switch (Settings.FolderMode)
            {
                case FolderMode.Hierarchy:
                    baseDirectory = Path.Combine(baseDirectory, tagPath);
                    break;

                case FolderMode.Hybrid:
                    baseDirectory = Path.Combine(baseDirectory, tagClass, tagPath);
                    break;

                case FolderMode.TagClass:
                    baseDirectory = Path.Combine(baseDirectory, tagClass, Utils.GetFileName(tagPath));
                    break;
            }

            baseDirectory = Utils.GetSafeFileName(baseDirectory);
            var parent = Directory.GetParent(baseDirectory);
            if (!parent.Exists)
                parent.Create();

            return baseDirectory;
        }

        private sealed class ExtractCounter
        {
            public volatile int Extracted;
            public volatile int Errors;
        }

        private sealed class BatchExtractSettings : IPluginSettings
        {
            private const int minWorkers = 1;
            private const int maxWorkers = 10;

            [Editor(typeof(BrowseFolderEditor), typeof(BrowseFolderEditor))]
            [DisplayName("Data Folder")]
            [DefaultValue(":plugins:\\Batch Extractor")]
            public string DataFolder { get; set; }

            [DisplayName("Data Folder Prompt")]
            [DefaultValue(true)]
            public bool PromptForFolder { get; set; }

            [DisplayName("Auto Update Data Folder")]
            [DefaultValue(false)]
            public bool AutoDataFolder { get; set; }

            [DisplayName("Overwrite Existing")]
            [DefaultValue(true)]
            public bool OverwriteExisting { get; set; }

            [DisplayName("Folder Mode")]
            [DefaultValue(FolderMode.Hierarchy)]
            public FolderMode FolderMode { get; set; }

            [DisplayName("Bitmap Format")]
            [DefaultValue(BitmapFormat.TIF)]
            public BitmapFormat BitmapFormat { get; set; }

            [DisplayName("Bitmap Mode")]
            [DefaultValue(BitmapMode.Default)]
            public BitmapMode BitmapMode { get; set; }

            [ItemsSource(typeof(ModelFormatItemsSource))]
            [DisplayName("Model Format")]
            [DefaultValue("amf")]
            public string ModelFormat { get; set; }

            [DisplayName("Batch Worker Count")]
            [DefaultValue(minWorkers), Range(minWorkers, maxWorkers)]
            public int BatchWorkerCount { get; set; }

            void IPluginSettings.ApplyDefaultValues(bool newInstance)
            {
                if (BatchWorkerCount < minWorkers || BatchWorkerCount > maxWorkers)
                    BatchWorkerCount = minWorkers;
            }
        }

        private enum FolderMode
        {
            Hierarchy,
            TagClass,
            Hybrid
        }

        private enum BitmapMode
        {
            Default,
            Bgr24,
            IsolateAlpha,
            IsolateAll,
            MixedIsolate
        }

        private enum BitmapFormat
        {
            DDS,
            TIF,
            PNG,
            JPEG,
            TGA
        }

        #region Extractables
        private interface IExtractable
        {
            string ItemKey { get; }
            string DisplayName { get; }
            string Destination { get; }
            int GetContentType();
            bool GetBitmapContent(out IContentProvider<IBitmap> provider);
            bool GetGeometryContent(out IContentProvider<Scene> provider);
            bool GetSoundContent(out IContentProvider<GameSound> provider);
        }

        private sealed class CacheExtractable : IExtractable
        {
            private readonly IIndexItem item;

            public string ItemKey => $"{item.TagName}.{item.ClassName}";

            public string DisplayName => ItemKey;

            public string Destination { get; }

            public CacheExtractable(IIndexItem item, string destination)
            {
                this.item = item;
                Destination = destination;
            }

            public int GetContentType()
            {
                return item.ClassCode switch
                {
                    "bitm" => 0,
                    "mode" or "mod2" or "sbsp" => 1,
                    "snd!" => 2,
                    _ => -1
                };
            }

            //TODO
            public bool GetBitmapContent(out IContentProvider<IBitmap> provider) => BlamContentFactory.TryGetBitmapContent(item, out provider);
            public bool GetGeometryContent(out IContentProvider<Scene> provider) => BlamContentFactory.TryGetGeometryContent(item, out provider);
            public bool GetSoundContent(out IContentProvider<GameSound> provider) => BlamContentFactory.TryGetSoundContent(item, out provider);
        }

        private sealed class ModuleExtractable : IExtractable
        {
            private readonly ModuleItem item;

            public string ItemKey => $"{item.TagName}.{item.ClassName}";

            public string DisplayName => ItemKey;

            public string Destination { get; }

            public ModuleExtractable(ModuleItem item, string destination)
            {
                this.item = item;
                Destination = destination;
            }

            public int GetContentType()
            {
                return item.ClassCode switch
                {
                    "bitm" => 0,
                    "mode" or "sbsp" or "stlm" or "scnr" or "pmdf" => 1,
                    //"snd!" => return 2,
                    _ => -1
                };
            }

            public bool GetBitmapContent(out IContentProvider<IBitmap> provider) => BlamContentFactory.TryGetBitmapContent(item, out provider);
            public bool GetGeometryContent(out IContentProvider<Scene> provider) => BlamContentFactory.TryGetGeometryContent(item, out provider);
            public bool GetSoundContent(out IContentProvider<GameSound> provider)
            {
                provider = null;
                return false;
            }
        }

        private sealed class PakExtractable : IExtractable
        {
            private readonly IPakItem item;

            public string ItemKey => $"{item.ItemType}\\{item.Name}";

            public string DisplayName => ItemKey;

            public string Destination { get; }

            public PakExtractable(IPakItem item, string destination)
            {
                this.item = item;
                Destination = destination;
            }

            public int GetContentType()
            {
                return item.ItemType switch
                {
                    PakItemType.Textures => 0,
                    _ => -1
                };
            }

            public bool GetBitmapContent(out IContentProvider<IBitmap> provider) => SaberContentFactory.TryGetBitmapContent(item, out provider);

            public bool GetGeometryContent(out IContentProvider<Scene> provider)
            {
                provider = null;
                return false;
            }

            public bool GetSoundContent(out IContentProvider<GameSound> provider)
            {
                provider = null;
                return false;
            }
        }
        #endregion
    }
}
