using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Plugins.MetaViewer;
using Reclaimer.Utilities;
using Studio.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MetaViewer.xaml
    /// </summary>
    public partial class MetaViewer : IDisposable
    {
        #region Dependency Properties
        public static readonly DependencyProperty ShowInvisiblesProperty =
            DependencyProperty.Register(nameof(ShowInvisibles), typeof(bool), typeof(MetaViewer), new PropertyMetadata(false, ShowInvisiblesChanged));

        public bool ShowInvisibles
        {
            get => (bool)GetValue(ShowInvisiblesProperty);
            set => SetValue(ShowInvisiblesProperty, value);
        }

        public static void ShowInvisiblesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MetaViewerPlugin.Settings.ShowInvisibles = e.NewValue as bool? ?? false;
        }
        #endregion

        private object item;
        private string xmlFileName;

        private Plugins.MetaViewer.Halo3.MetaContext context;

        public TabModel TabModel { get; }
        public ObservableCollection<MetaValueBase> Metadata { get; }

        public MetaViewer()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Document);
            Metadata = new ObservableCollection<MetaValueBase>();
            DataContext = this;
            ShowInvisibles = MetaViewerPlugin.Settings.ShowInvisibles;
        }

        public void LoadMetadata(IIndexItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.TagName}.{tag.ClassCode}";
            TabModel.Header = $"{Utils.GetFileName(tag.TagName)}.{tag.ClassCode}";

            item = tag;
            this.xmlFileName = xmlFileName;

            LoadData();
        }

        public void LoadMetadata(IModuleItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.TagName}.{tag.ClassCode}";
            TabModel.Header = $"{tag.FileName}.{tag.ClassCode}";

            item = tag;
            this.xmlFileName = xmlFileName;

            LoadData();
        }

        private void LoadData()
        {
            if (item is IIndexItem cacheItem)
                LoadDataHalo3(xmlFileName, cacheItem, Metadata, ref context);
            else if (item is IModuleItem moduleItem)
                LoadDataHalo5(xmlFileName, moduleItem, Metadata);
        }

        internal static void LoadDataHalo3(string xmlFileName, IIndexItem tag, IList<MetaValueBase> collection, ref Plugins.MetaViewer.Halo3.MetaContext context)
        {
            collection.Clear();

            var doc = new XmlDocument();
            doc.Load(xmlFileName);

            context?.DataSource?.Dispose();
            context = new Plugins.MetaViewer.Halo3.MetaContext(doc, tag);

            foreach (var n in doc.DocumentElement.GetChildElements())
            {
                try
                {
                    var meta = MetaValueBase.GetMetaValue(n, context, tag.GetBaseAddress());
                    collection.Add(meta);
                }
                catch { }
            }

            context.UpdateBlockIndices();
        }

        internal static void LoadDataHalo5(string xmlFileName, IModuleItem tag, IList<MetaValueBase> collection)
        {
            collection.Clear();

            var doc = new XmlDocument();
            doc.Load(xmlFileName);

            var offset = 0;
            using (var tagReader = tag.CreateReader())
            {
                var header = tag.ReadMetadataHeader(tagReader);
                using (var reader = tagReader.CreateVirtualReader(header.HeaderSize))
                {
                    var rootIndex = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;
                    var mainBlock = header.DataBlocks[rootIndex];

                    foreach (var n in doc.DocumentElement.GetChildElements())
                    {
                        try
                        {
                            var def = FieldDefinition.GetHalo5Definition(tag, n);
                            var meta = MetaValueBase.GetMetaValue(n, tag, header, mainBlock, reader, mainBlock.Offset, offset);
                            collection.Add(meta);
                            offset += def.Size;
                        }
                        catch { break; }
                    }
                }
            }
        }

        private static void RecursiveToggle(IEnumerable<MetaValueBase> collection, bool value)
        {
            foreach (var s in collection.OfType<IExpandable>())
            {
                s.IsExpanded = value;
                RecursiveToggle(s.Children, value);
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e) => LoadData();

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            MetaViewerPlugin.ExportJson(xmlFileName, item);
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e) => RecursiveToggle(Metadata, false);
        private void btnExpandAll_Click(object sender, RoutedEventArgs e) => RecursiveToggle(Metadata, true);

        public void Dispose() => context?.Dispose();

        private void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Parameter is Plugins.MetaViewer.Halo3.TagReferenceValue h3ref && h3ref.SelectedItem is not null)
                {
                    var item = h3ref.SelectedItem.Context;
                    var fileName = $"{item.TagName}.{item.ClassName}";
                    var fileKey = $"Blam.{item.CacheFile.CacheType}.{item.ClassCode}";
                    var args = new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), item);
                    Substrate.OpenWithPlugin(args, typeof(MetaViewerPlugin));
                }
                else if (e.Parameter is Plugins.MetaViewer.Halo5.TagReferenceValue h5ref && h5ref.SelectedItem is not null)
                {
                    var item = h5ref.SelectedItem.Context;
                    var fileName = $"{item.TagName}.{item.ClassName}";
                    var fileKey = $"Blam.{item.Module.ModuleType}.{item.ClassCode}";
                    var args = new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), item);
                    Substrate.OpenWithPlugin(args, typeof(MetaViewerPlugin));
                }
            }
            catch { }
        }
    }
}
