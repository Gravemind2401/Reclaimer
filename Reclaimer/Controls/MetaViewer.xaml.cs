using Adjutant.Blam.Common;
using Adjutant.Blam.Halo5;
using Reclaimer.Models;
using Reclaimer.Plugins.MetaViewer;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            get { return (bool)GetValue(ShowInvisiblesProperty); }
            set { SetValue(ShowInvisiblesProperty, value); }
        }

        public static void ShowInvisiblesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MetaViewerPlugin.Settings.ShowInvisibles = e.NewValue as bool? ?? false;
        }
        #endregion

        private object item;
        private string fileName;

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
            TabModel.ToolTip = $"{tag.FullPath}.{tag.ClassCode}";
            TabModel.Header = $"{Utils.GetFileName(tag.FullPath)}.{tag.ClassCode}";

            item = tag;
            fileName = xmlFileName;

            LoadData();
        }

        public void LoadMetadata(ModuleItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.FullPath}.{tag.ClassCode}";
            TabModel.Header = $"{Utils.GetFileName(tag.FullPath)}.{tag.ClassCode}";

            item = tag;
            fileName = xmlFileName;

            LoadData();
        }

        private void LoadData()
        {
            if (item is IIndexItem)
                LoadDataHalo3();
            else if (item is ModuleItem)
                LoadDataHalo5();
        }

        private void LoadDataHalo3()
        {
            var tag = item as IIndexItem;
            Metadata.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            context?.DataSource?.Dispose();
            context = new Plugins.MetaViewer.Halo3.MetaContext(doc, tag.CacheFile, tag);

            foreach (XmlNode n in doc.DocumentElement.ChildNodes)
            {
                try
                {
                    var meta = MetaValueBase.GetMetaValue(n, context, tag.MetaPointer.Address);
                    Metadata.Add(meta);
                }
                catch { }
            }

            context.UpdateBlockIndices();
        }

        private void LoadDataHalo5()
        {
            var tag = item as ModuleItem;
            Metadata.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            var offset = 0;
            using (var tagReader = tag.CreateReader())
            {
                var header = new MetadataHeader(tagReader);
                using (var reader = tagReader.CreateVirtualReader(header.Header.HeaderSize))
                {
                    var rootIndex = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;
                    var mainBlock = header.DataBlocks[rootIndex];

                    foreach (XmlNode n in doc.DocumentElement.ChildNodes)
                    {
                        try
                        {
                            var def = FieldDefinition.GetHalo5Definition(n);
                            var meta = MetaValueBase.GetMetaValue(n, tag, header, mainBlock, reader, mainBlock.Offset, offset);
                            Metadata.Add(meta);
                            offset += def.Size;
                        }
                        catch { break; }
                    }
                }
            }
        }

        private void RecursiveToggle(IEnumerable<MetaValueBase> collection, bool value)
        {
            foreach (var s in collection.OfType<IExpandable>())
            {
                s.IsExpanded = value;
                RecursiveToggle(s.Children, value);
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            RecursiveToggle(Metadata, false);
        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            RecursiveToggle(Metadata, true);
        }

        public void Dispose()
        {
            context?.Dispose();
        }
    }
}
