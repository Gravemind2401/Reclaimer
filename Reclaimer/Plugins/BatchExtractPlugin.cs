using Adjutant.Audio;
using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Utilities;
using Reclaimer.Models;
using Reclaimer.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Reclaimer.Plugins
{
    public class BatchExtractPlugin : Plugin
    {
        private const string supportedTags = "bitm,mode,mod2,sbsp,snd!";

        private ConcurrentQueue<IIndexItem> extractionQueue = new ConcurrentQueue<IIndexItem>();

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
                CacheType cacheType;
                if (!Enum.TryParse(match.Groups[1].Value, out cacheType))
                    yield break;

                if (match.Groups[2].Value == "*" && context.File.Any(i => i is TreeItemModel))
                    yield return ExtractMultipleContextItem;
                else
                {
                    var item = context.File.OfType<IIndexItem>().FirstOrDefault();
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

                Settings.DataFolder = dataFolder = fsd.SelectedPath;
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

            extractionQueue = new ConcurrentQueue<IIndexItem>();
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
                var item = context.File.OfType<IIndexItem>().FirstOrDefault();
                if (item != null)
                    extractionQueue.Enqueue(item);
            }

            if (isBusy) return;

            tokenSource = new CancellationTokenSource();
            isBusy = true;

            Task.Run(() =>
            {
                var counter = new ExtractCounter();
                var start = DateTime.Now;

                while (extractionQueue.Count > 0)
                {
                    if (tokenSource.IsCancellationRequested)
                        break;

                    IIndexItem item;
                    if (!extractionQueue.TryDequeue(out item))
                        break;

                    if (item != null)
                    {
                        SetWorkingStatus($"Extracting {item.FullPath}");
                        Extract(item, counter);
                    }
                }

                var span = DateTime.Now - start;
                LogOutput($"Extracted {counter.Extracted} tags in {Math.Round(span.TotalSeconds)} seconds with {counter.Errors} errors.");

                tokenSource.Dispose();
                tokenSource = null;
                isBusy = false;
                ClearWorkingStatus();
            }, tokenSource.Token);
        }

        private void BatchQueue(TreeItemModel node)
        {
            if (node.HasItems)
            {
                foreach (var child in node.Items)
                    BatchQueue(child);
            }
            else extractionQueue.Enqueue(node.Tag as IIndexItem);
        }

        private void Extract(IIndexItem tag, ExtractCounter counter)
        {
            try
            {
                switch (tag.ClassCode)
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
            if (ContentFactory.TryGetBitmapContent(tag, out bitmap) &&SaveImage(bitmap, Settings.DataFolder))
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
                        bitmap.ToDds(i).WriteToDisk(fileName + ext);
                        extracted++;
                    }
                    continue;
                }

                var outputs = new List<Tuple<string, DecompressOptions>>();

                ImageFormat format; ;
                if (Settings.BitmapFormat == BitmapFormat.PNG)
                    format = ImageFormat.Png;
                else if (Settings.BitmapFormat == BitmapFormat.TIF)
                    format = ImageFormat.Tiff;
                else //if (Settings.BitmapFormat == BitmapFormat.JPEG)
                    format = ImageFormat.Jpeg;

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

                    dds.WriteToDisk(param.Item1, format, param.Item2, bitmap.CubeLayout);
                    extracted++;
                }
            }

            return extracted > 0;
        }

        private IEnumerable<Tuple<string, DecompressOptions>> GetParamsIsolateAlpha(string fileName, string extension)
        {
            yield return Tuple.Create($"{fileName}_hue{extension}", DecompressOptions.Bgr24);
            yield return Tuple.Create($"{fileName}_alpha{extension}", DecompressOptions.Bgr24 | DecompressOptions.AlphaChannelOnly);
        }

        private IEnumerable<Tuple<string, DecompressOptions>> GetParamsIsolateAll(string fileName, string extension)
        {
            var bgr24 = DecompressOptions.Bgr24;
            yield return Tuple.Create($"{fileName}_blue{extension}", bgr24 | DecompressOptions.BlueChannelOnly);
            yield return Tuple.Create($"{fileName}_green{extension}", bgr24 | DecompressOptions.GreenChannelOnly);
            yield return Tuple.Create($"{fileName}_red{extension}", bgr24 | DecompressOptions.RedChannelOnly);
            yield return Tuple.Create($"{fileName}_alpha{extension}", bgr24 | DecompressOptions.AlphaChannelOnly);
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
                    baseDirectory = Path.Combine(baseDirectory, tagClass);
                    break;
            }

            baseDirectory = Utils.GetSafeFileName(baseDirectory);
            var parent = Directory.GetParent(baseDirectory);
            if (!parent.Exists)
                parent.Create();

            return baseDirectory;
        }

        private class ExtractCounter
        {
            public int Extracted { get; set; }
            public int Errors { get; set; }
        }

        private class BatchExtractSettings
        {
            public string DataFolder { get; set; }
            public bool PromptForFolder { get; set; }
            public bool OverwriteExisting { get; set; }
            public FolderMode FolderMode { get; set; }
            public BitmapFormat BitmapFormat { get; set; }
            public BitmapMode BitmapMode { get; set; }
            public string ModelFormat { get; set; }

            public BatchExtractSettings()
            {
                DataFolder = ":plugins:\\Batch Extractor";
                PromptForFolder = true;
                OverwriteExisting = true;
                FolderMode = FolderMode.Hierarchy;
                BitmapFormat = BitmapFormat.TIF;
                BitmapMode = BitmapMode.Default;
                ModelFormat = "amf";
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
            JPEG
        }
    }
}
