using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Utilities;
using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Reclaimer.Plugins
{
    public class BatchExtractPlugin : Plugin
    {
        private bool isBusy;
        private CancellationTokenSource tokenSource;

        public override string Name => "Batch Extractor";

        private BatchExtractSettings Settings { get; set; }

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
            CacheType cacheType;
            var split = context.FileTypeKey.Split('.');
            if (split.Length == 2 && !Enum.TryParse(split[0], out cacheType))
                yield break;

            if (context.File is TreeNode && split[1] == "*")
                yield return new PluginContextItem("ExtractAll", "Extract All", OnContextItemClick);
            else yield return new PluginContextItem("Extract", "Extract", OnContextItemClick);
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
            if (isBusy)
            {
                System.Windows.Forms.MessageBox.Show("Already in progress");
                return;
            }

            var folder = Settings.DataFolder;
            if (Settings.PromptForFolder)
            {
                var fsd = new FolderSelectDialog();
                if (!fsd.ShowDialog())
                    return;

                Settings.DataFolder = folder = fsd.SelectedPath;
            }

            tokenSource = new CancellationTokenSource();
            isBusy = true;

            Task.Run(() =>
            {
                var counter = new ExtractCounter();
                var start = DateTime.Now;

                if (context.File is TreeNode)
                    BatchExtract(context.File as TreeNode, counter);
                else Extract(context as IIndexItem, counter);

                var span = DateTime.Now - start;
                LogOutput($"Extracted {counter.Extracted} tags in {Math.Round(span.TotalSeconds)} seconds with {counter.Errors} errors.");

                tokenSource.Dispose();
                tokenSource = null;
                isBusy = false;
            }, tokenSource.Token);
        }

        private void BatchExtract(TreeNode node, ExtractCounter counter)
        {
            if (tokenSource.IsCancellationRequested)
                return;

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                    BatchExtract(child, counter);
            }
            else
            {
                var tag = node.Tag as IIndexItem;
                if (tag == null) return;
                Extract(node.Tag as IIndexItem, counter);
            }
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

            for (int i = 0; i < bitmap.SubmapCount; i++)
            {
                var dds = bitmap.ToDds(i);
                var fileName = MakePath(tag, Settings.DataFolder) + (bitmap.SubmapCount > 1 ? $"[{i}]" : string.Empty) + "." + Settings.BitmapFormat.ToString().ToLower();

                if (Settings.BitmapFormat == BitmapFormat.DDS)
                {
                    dds.WriteToDisk(fileName);
                    break;
                }

                var format = Settings.BitmapFormat == BitmapFormat.PNG ? ImageFormat.Png : ImageFormat.Tiff;
                var options = Settings.BitmapMode == BitmapMode.NoAlpha ? DecompressOptions.Bgr24 : DecompressOptions.Default;
                dds.WriteToDisk(fileName, format, options | DecompressOptions.UnwrapCubemap);
            }

            LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
        }

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

            var fileName = MakePath(tag, Settings.DataFolder) + ".amf";
            geometry.ReadGeometry(0).WriteAMF(fileName);
            LogOutput($"Extracted {tag.FullPath}.{tag.ClassName}");
        }

        private string MakePath(IIndexItem tag, string baseDirectory)
        {
            switch (Settings.FolderMode)
            {
                case FolderMode.Hierarchy:
                    baseDirectory = Path.Combine(baseDirectory, tag.FullPath);
                    break;

                case FolderMode.Hybrid:
                    baseDirectory = Path.Combine(baseDirectory, tag.ClassName, tag.FullPath);
                    break;

                case FolderMode.TagClass:
                    baseDirectory = Path.Combine(baseDirectory, tag.ClassName);
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
                BitmapMode = BitmapMode.Standard;
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
            Standard,
            NoAlpha,
            IsolateChannels
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
