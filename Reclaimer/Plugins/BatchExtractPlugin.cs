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
        private const string supportedTags = "bitm,mode,mod2,sbsp";

        private readonly ConcurrentQueue<IIndexItem> extractionQueue = new ConcurrentQueue<IIndexItem>();

        private bool isBusy;
        private CancellationTokenSource tokenSource;

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

        private void OnMenuItemClick(string key)
        {
            if (!isBusy)
            {
                System.Windows.Forms.MessageBox.Show("Nothing in progress");
                return;
            }

            if (!tokenSource.IsCancellationRequested)
                tokenSource.Cancel();
        }

        private void OnContextItemClick(string key, OpenFileArgs context)
        {
            var folder = Settings.DataFolder;
            if (Settings.PromptForFolder && !isBusy)
            {
                var fsd = new FolderSelectDialog();
                if (!string.IsNullOrEmpty(Settings.DataFolder))
                    fsd.InitialDirectory = Settings.DataFolder;

                if (!fsd.ShowDialog())
                    return;

                Settings.DataFolder = folder = fsd.SelectedPath;
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

            Substrate.ShowOutput();
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
                        Extract(item, counter);
                }

                var span = DateTime.Now - start;
                LogOutput($"Extracted {counter.Extracted} tags in {Math.Round(span.TotalSeconds)} seconds with {counter.Errors} errors.");

                tokenSource.Dispose();
                tokenSource = null;
                isBusy = false;
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
                        SaveImage(tag);
                        counter.Extracted++;
                        break;

                    case "mode":
                    case "mod2":
                    case "sbsp":
                        SaveModel(tag);
                        counter.Extracted++;
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
        private void SaveImage(IIndexItem tag)
        {
            IBitmap bitmap;
            switch (tag.CacheFile.CacheType)
            {
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                    bitmap = tag.ReadMetadata<Adjutant.Blam.Halo1.bitmap>();
                    break;

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    bitmap = tag.ReadMetadata<Adjutant.Blam.Halo2.bitmap>();
                    break;

                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.Halo3ODST:
                    bitmap = tag.ReadMetadata<Adjutant.Blam.Halo3.bitmap>();
                    break;

                default: return;
            }

            SaveImage(bitmap, Settings.DataFolder);

            LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
        }

        private void SaveImage(IBitmap bitmap, string baseDir)
        {
            for (int i = 0; i < bitmap.SubmapCount; i++)
            {
                var dds = bitmap.ToDds(i);
                var fileName = MakePath(bitmap.Class, bitmap.Name, baseDir);
                var ext = "." + Settings.BitmapFormat.ToString().ToLower();

                if (bitmap.SubmapCount > 1)
                    fileName += $"[{i}]";

                if (Settings.BitmapFormat == BitmapFormat.DDS)
                {
                    dds.WriteToDisk(fileName + ext);
                    continue;
                }

                var format = Settings.BitmapFormat == BitmapFormat.PNG ? ImageFormat.Png : ImageFormat.Tiff;
                if (Settings.BitmapMode == BitmapMode.Default)
                    dds.WriteToDisk(fileName + ext, format);
                else if (Settings.BitmapMode == BitmapMode.Bgr24)
                    dds.WriteToDisk(fileName + ext, format, DecompressOptions.Bgr24);
                else if (Settings.BitmapMode == BitmapMode.IsolateAlpha)
                    WriteImageIsolateAlpha(dds, fileName, ext, format, bitmap.CubeLayout);
                else if (Settings.BitmapMode == BitmapMode.IsolateAll)
                    WriteImageIsolateAll(dds, fileName, ext, format, bitmap.CubeLayout);
                else if (Settings.BitmapMode == BitmapMode.MixedIsolate)
                    WriteImageMixedIsolate(dds, fileName, ext, format, bitmap.CubeLayout);
            }
        }

        private void WriteImageIsolateAlpha(DdsImage image, string fileName, string extension, ImageFormat format, CubemapLayout layout)
        {
            image.WriteToDisk($"{fileName}_hue{extension}", format, DecompressOptions.Bgr24);
            image.WriteToDisk($"{fileName}_alpha{extension}", format, DecompressOptions.Bgr24 | DecompressOptions.AlphaChannelOnly);
        }

        private void WriteImageIsolateAll(DdsImage image, string fileName, string extension, ImageFormat format, CubemapLayout layout)
        {
            var options = DecompressOptions.Bgr24;
            image.WriteToDisk($"{fileName}_blue{extension}", format, options | DecompressOptions.BlueChannelOnly);
            image.WriteToDisk($"{fileName}_green{extension}", format, options | DecompressOptions.GreenChannelOnly);
            image.WriteToDisk($"{fileName}_red{extension}", format, options | DecompressOptions.RedChannelOnly);
            image.WriteToDisk($"{fileName}_alpha{extension}", format, options | DecompressOptions.AlphaChannelOnly);
        }

        private static readonly string[] shouldIsolate = new[] { "([_ ]multi)$", "([_ ]multipurpose)$", "([_ ]cc)$" };
        private void WriteImageMixedIsolate(DdsImage image, string fileName, string extension, ImageFormat format, CubemapLayout layout)
        {
            var imageName = fileName.Split('\\').Last();
            if (imageName.EndsWith("]"))
                imageName = imageName.Substring(0, imageName.LastIndexOf('['));

            if (shouldIsolate.Any(s => Regex.IsMatch(imageName, s, RegexOptions.IgnoreCase)))
                WriteImageIsolateAll(image, fileName, extension, format, layout);
            else WriteImageIsolateAlpha(image, fileName, extension, format, layout);
        }
        #endregion

        private void SaveModel(IIndexItem tag)
        {
            IRenderGeometry geometry;
            switch (tag.CacheFile.CacheType)
            {
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                    geometry = tag.ClassCode == "sbsp" ? (IRenderGeometry)tag.ReadMetadata<Adjutant.Blam.Halo1.scenario_structure_bsp>() : tag.ReadMetadata<Adjutant.Blam.Halo1.gbxmodel>();
                    break;

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    geometry = tag.ClassCode == "sbsp" ? (IRenderGeometry)tag.ReadMetadata<Adjutant.Blam.Halo2.scenario_structure_bsp>() : tag.ReadMetadata<Adjutant.Blam.Halo2.render_model>();
                    break;

                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.Halo3ODST:
                    geometry = tag.ClassCode == "sbsp" ? (IRenderGeometry)tag.ReadMetadata<Adjutant.Blam.Halo3.scenario_structure_bsp>() : tag.ReadMetadata<Adjutant.Blam.Halo3.render_model>();
                    break;

                default: return;
            }

            SaveModel(geometry, Settings.DataFolder);

            LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
        }

        private void SaveModel(IRenderGeometry geometry, string baseDir)
        {
            var fileName = MakePath(geometry.Class, geometry.Name, baseDir) + ".amf";
            geometry.ReadGeometry(0).WriteAMF(fileName);
        }

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
            public ModelFormat ModelFormat { get; set; }

            public BatchExtractSettings()
            {
                DataFolder = ":plugins:\\Batch Extractor";
                PromptForFolder = true;
                OverwriteExisting = true;
                FolderMode = FolderMode.Hierarchy;
                BitmapFormat = BitmapFormat.TIF;
                BitmapMode = BitmapMode.Default;
                ModelFormat = ModelFormat.AMF;
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
            PNG
        }

        private enum ModelFormat
        {
            AMF
        }
    }
}
