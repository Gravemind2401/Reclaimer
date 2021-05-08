using Adjutant.Audio;
using Adjutant.Blam.Common;
using Adjutant.Blam.Halo5;
using Adjutant.Geometry;
using Adjutant.Utilities;
using Reclaimer.Controls.Editors;
using Reclaimer.Models;
using Reclaimer.Utilities;
using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Dds;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Reclaimer.Plugins
{
    public class BatchExtractPlugin : Plugin
    {
        private const string supportedTags = "bitm,mode,mod2,sbsp,snd!";

        private ConcurrentQueue<object> extractionQueue = new ConcurrentQueue<object>();

        private bool isBusy;
        private CancellationTokenSource tokenSource;

        private delegate string GetModelExtension(string formatId);
        private Lazy<GetModelExtension> getModelExtensionFunc;

        private delegate void WriteModelFile(IGeometryModel model, string fileName, string formatId);
        private WriteModelFile writeModelFileFunc;

        private delegate bool WriteSoundFile(GameSound sound, string directory, bool overwrite);
        private WriteSoundFile writeSoundFileFunc;

        public override string Name => "Batch Extractor";

        private BatchExtractSettings Settings { get; set; }

        private PluginContextItem ExtractMultipleContextItem
        {
            get
            {
                return new PluginContextItem("ExtractAll", isBusy ? "Add to extraction queue" : "Extract All", OnContextItemClick);
            }
        }
        private PluginContextItem ExtractSingleContextItem
        {
            get
            {
                return new PluginContextItem("Extract", isBusy ? "Add to extraction queue" : "Extract", OnContextItemClick);
            }
        }

        public override void Initialise()
        {
            Settings = LoadSettings<BatchExtractSettings>();
            Settings.DataFolder = Settings.DataFolder.PatternReplace(":plugins:", Substrate.PluginsDirectory);
            getModelExtensionFunc = new Lazy<GetModelExtension>(() => Substrate.GetSharedFunction<GetModelExtension>("Reclaimer.Plugins.ModelViewerPlugin.GetFormatExtension"));
        }

        public override void PostInitialise()
        {
            writeModelFileFunc = Substrate.GetSharedFunction<WriteModelFile>("Reclaimer.Plugins.ModelViewerPlugin.WriteModelFile");
            writeSoundFileFunc = Substrate.GetSharedFunction<WriteSoundFile>("Reclaimer.Plugins.SoundExtractorPlugin.WriteSoundFile");
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem("Cancel", "Tools\\Cancel Batch Extract", OnMenuItemClick);
        }

        public override IEnumerable<PluginContextItem> GetContextItems(OpenFileArgs context)
        {
            Match match;
            if ((match = Regex.Match(context.FileTypeKey, @"Blam\.(\w+)\.(.*)")).Success)
            {
                if (!ValidateCacheType(match.Groups[1].Value))
                    yield break;

                if (match.Groups[2].Value == "*" && context.File.Any(i => i is TreeItemModel))
                    yield return ExtractMultipleContextItem;
                else
                {
                    dynamic item = context.File.FirstOrDefault(f => ValidateTag(f));
                    if (item != null && supportedTags.Split(',').Any(s => item.ClassCode == s))
                        yield return ExtractSingleContextItem;
                }
            }
        }

        public override void Suspend()
        {
            Settings.DataFolder = Settings.DataFolder.PatternReplace(Substrate.PluginsDirectory, ":plugins:");
            SaveSettings(Settings);
        }

        private bool ValidateCacheType(string name)
        {
            CacheType cacheType;
            if (Enum.TryParse(name, out cacheType))
                return true;

            ModuleType moduleType;
            if (Enum.TryParse(name, out moduleType))
                return true;

            return false;
        }

        private bool ValidateTag(object tag) => tag != null && (tag is IIndexItem || tag is ModuleItem);

        [SharedFunction]
        private bool GetDataFolder(out string dataFolder)
        {
            dataFolder = Settings.DataFolder;
            if (Settings.PromptForFolder)
            {
                var fsd = new FolderSelectDialog();
                if (!string.IsNullOrEmpty(Settings.DataFolder))
                    fsd.InitialDirectory = Settings.DataFolder;

                if (!fsd.ShowDialog())
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
                System.Windows.Forms.MessageBox.Show("Nothing in progress");
                return;
            }

            if (!tokenSource.IsCancellationRequested)
                tokenSource.Cancel();

            extractionQueue = new ConcurrentQueue<object>();
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            if (!isBusy)
            {
                string folder;
                if (!GetDataFolder(out folder))
                    return;
            }

            var node = context.File.OfType<TreeItemModel>().FirstOrDefault();
            if (node != null)
                BatchQueue(node);
            else
            {
                var item = context.File.FirstOrDefault(f => ValidateTag(f));
                if (item != null)
                    extractionQueue.Enqueue(item);
            }

            if (isBusy) return;

            tokenSource = new CancellationTokenSource();

            Task.Run(ProcessQueueAsync, tokenSource.Token);
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

                Func<Task> process = async () =>
                {
                    var prefix = Settings.BatchWorkerCount > 1 ? $"[Worker {nextWorkerId++}] " : string.Empty;

                    while (extractionQueue.Count > 0)
                    {
                        if (tokenSource.IsCancellationRequested)
                            break;

                        dynamic item;
                        if (!extractionQueue.TryDequeue(out item))
                            break;

                        if (item != null)
                        {
                            var itemKey = $"{item.FullPath}.{item.ClassName}";
                            using (await locker.WaitAsync(itemKey))
                            {
                                SetWorkingStatus($"{prefix}Extracting {item.FullPath}");
                                Extract(item, counter);
                            }
                        }
                    }
                };

                var processors = Enumerable.Range(0, Settings.BatchWorkerCount).Select(i => Task.Run(process)).ToList();
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

        private void BatchQueue(TreeItemModel node)
        {
            if (node.HasItems)
            {
                foreach (var child in node.Items)
                    BatchQueue(child);
            }
            else if (ValidateTag(node.Tag))
                extractionQueue.Enqueue(node.Tag);
        }

        private void Extract(dynamic tag, ExtractCounter counter)
        {
            try
            {
                switch ((string)tag.ClassCode)
                {
                    case "bitm":
                        if (SaveImage(tag))
                            counter.Extracted++;
                        break;

                    case "mode":
                    case "mod2":
                    case "sbsp":
                        if (writeModelFileFunc != null)
                        {
                            if (SaveModel(tag))
                                counter.Extracted++;
                        }
                        break;

                    case "snd!":
                        if (writeSoundFileFunc != null)
                        {
                            if (SaveSound(tag))
                                counter.Extracted++;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                LogError($"Error extracting {tag.FullPath}", e);
                counter.Errors++;
            }
        }

        #region Images
        private bool SaveImage(IIndexItem tag)
        {
            IBitmap bitmap;
            if (ContentFactory.TryGetBitmapContent(tag, out bitmap) && SaveImage(bitmap, Settings.DataFolder))
            {
                LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
                return true;
            }

            return false;
        }

        private bool SaveImage(ModuleItem tag)
        {
            IBitmap bitmap;
            if (ContentFactory.TryGetBitmapContent(tag, out bitmap) && SaveImage(bitmap, Settings.DataFolder))
            {
                LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
                return true;
            }

            return false;
        }

        [SharedFunction]
        private bool SaveImage(IBitmap bitmap, string baseDir)
        {
            var extracted = 0;
            for (int i = 0; i < bitmap.SubmapCount; i++)
            {
                var fileName = MakePath(bitmap.Class, bitmap.Name, baseDir);
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

                var outputs = new List<Tuple<string, DecompressOptions>>();

                ImageFormat format;
                if (Settings.BitmapFormat == BitmapFormat.PNG)
                    format = ImageFormat.Png;
                else if (Settings.BitmapFormat == BitmapFormat.TIF)
                    format = ImageFormat.Tiff;
                else if (Settings.BitmapFormat == BitmapFormat.JPEG)
                    format = ImageFormat.Jpeg;
                else //if (Settings.BitmapFormat == BitmapFormat.TGA)
                    format = null;

                if (Settings.BitmapMode == BitmapMode.Default)
                    outputs.Add(Tuple.Create(fileName + ext, DecompressOptions.Default));
                else if (Settings.BitmapMode == BitmapMode.Bgr24)
                    outputs.Add(Tuple.Create(fileName + ext, DecompressOptions.Bgr24));
                else if (Settings.BitmapMode == BitmapMode.IsolateAlpha)
                    outputs.AddRange(GetParamsIsolateAlpha(fileName, ext));
                else if (Settings.BitmapMode == BitmapMode.IsolateAll)
                    outputs.AddRange(GetParamsIsolateAll(fileName, ext));
                else if (Settings.BitmapMode == BitmapMode.MixedIsolate)
                    outputs.AddRange(GetParamsMixedIsolate(fileName, ext));

                DdsImage dds = null;
                foreach (var param in outputs)
                {
                    if (!Settings.OverwriteExisting && File.Exists(param.Item1))
                        continue;

                    if (dds == null)
                        dds = bitmap.ToDds(i);

                    if (format != null)
                        dds.WriteToDisk(param.Item1, format, param.Item2, bitmap.CubeLayout);
                    else //if (Settings.BitmapFormat == BitmapFormat.TGA)
                        dds.WriteToTarga(param.Item1, param.Item2, bitmap.CubeLayout);

                    extracted++;
                }
            }

            return extracted > 0;
        }

        private IEnumerable<Tuple<string, DecompressOptions>> GetParamsIsolateAlpha(string fileName, string extension)
        {
            yield return Tuple.Create($"{fileName}_hue{extension}", DecompressOptions.Bgr24);
            yield return Tuple.Create($"{fileName}_alpha{extension}", DecompressOptions.AlphaChannelOnly);
        }

        private IEnumerable<Tuple<string, DecompressOptions>> GetParamsIsolateAll(string fileName, string extension)
        {
            yield return Tuple.Create($"{fileName}_blue{extension}", DecompressOptions.BlueChannelOnly);
            yield return Tuple.Create($"{fileName}_green{extension}", DecompressOptions.GreenChannelOnly);
            yield return Tuple.Create($"{fileName}_red{extension}", DecompressOptions.RedChannelOnly);
            yield return Tuple.Create($"{fileName}_alpha{extension}", DecompressOptions.AlphaChannelOnly);
        }

        private static readonly string[] shouldIsolate = new[] { "([_ ]multi)$", "([_ ]multipurpose)$", "([_ ]cc)$" };
        private IEnumerable<Tuple<string, DecompressOptions>> GetParamsMixedIsolate(string fileName, string extension)
        {
            var imageName = fileName.Split('\\').Last();
            if (imageName.EndsWith("]"))
                imageName = imageName.Substring(0, imageName.LastIndexOf('['));

            if (shouldIsolate.Any(s => Regex.IsMatch(imageName, s, RegexOptions.IgnoreCase)))
                return GetParamsIsolateAll(fileName, extension);
            else return GetParamsIsolateAlpha(fileName, extension);
        }
        #endregion

        #region Models
        private bool SaveModel(IIndexItem tag)
        {
            IRenderGeometry geometry;
            if (ContentFactory.TryGetGeometryContent(tag, out geometry) && SaveModel(geometry, Settings.DataFolder))
            {
                LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
                return true;
            }

            return false;
        }

        private bool SaveModel(ModuleItem tag)
        {
            IRenderGeometry geometry;
            if (ContentFactory.TryGetGeometryContent(tag, out geometry) && SaveModel(geometry, Settings.DataFolder))
            {
                LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
                return true;
            }

            return false;
        }

        [SharedFunction]
        private bool SaveModel(IRenderGeometry geometry, string baseDir)
        {
            var fileName = MakePath(geometry.Class, geometry.Name, baseDir);
            var ext = getModelExtensionFunc?.Value(Settings.ModelFormat);

            if (!Settings.OverwriteExisting && File.Exists($"{fileName}.{ext}"))
                return false;

            writeModelFileFunc?.Invoke(geometry.ReadGeometry(0), fileName, Settings.ModelFormat);
            return true;
        }
        #endregion

        #region Sounds
        private bool SaveSound(IIndexItem tag)
        {
            ISoundContainer container;
            if (ContentFactory.TryGetSoundContent(tag, out container) && SaveSound(container, Settings.DataFolder))
            {
                LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
                return true;
            }

            return false;
        }

        private bool SaveSound(ModuleItem tag)
        {
            return false;
        }

        [SharedFunction]
        private bool SaveSound(ISoundContainer sound, string baseDir)
        {
            var dir = Path.GetDirectoryName(MakePath(sound.Class, sound.Name, baseDir));
            return writeSoundFileFunc?.Invoke(sound.ReadData(), dir, Settings.OverwriteExisting) ?? false;
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

            [Editor(typeof(BrowseFolderEditor), typeof(PropertyValueEditor))]
            [DisplayName("Data Folder")]
            public string DataFolder { get; set; }

            [DisplayName("Data Folder Prompt")]
            public bool PromptForFolder { get; set; }

            [DisplayName("Auto Update Data Folder")]
            public bool AutoDataFolder { get; set; }

            [DisplayName("Overwrite Existing")]
            public bool OverwriteExisting { get; set; }

            [DisplayName("Folder Mode")]
            public FolderMode FolderMode { get; set; }

            [DisplayName("Bitmap Format")]
            public BitmapFormat BitmapFormat { get; set; }

            [DisplayName("Bitmap Mode")]
            public BitmapMode BitmapMode { get; set; }

            [ItemsSource(typeof(ModelFormatItemsSource))]
            [DisplayName("Model Format")]
            public string ModelFormat { get; set; }

            [DisplayName("Batch Worker Count")]
            [Range(minWorkers, maxWorkers)]
            public int BatchWorkerCount { get; set; }

            public BatchExtractSettings()
            {
                DataFolder = ":plugins:\\Batch Extractor";
                PromptForFolder = true;
                OverwriteExisting = true;
                FolderMode = FolderMode.Hierarchy;
                BitmapFormat = BitmapFormat.TIF;
                BitmapMode = BitmapMode.Default;
                ModelFormat = "amf";
                BatchWorkerCount = minWorkers;
            }

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
    }
}
