using Adjutant.Blam.Halo5;
using Reclaimer.Models;
using Reclaimer.Plugins.MetaViewer.Halo5;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MetaViewerHalo5.xaml
    /// </summary>
    public partial class MetaViewerHalo5
    {
        #region Dependency Properties
        public static readonly DependencyProperty ShowInvisiblesProperty =
            DependencyProperty.Register(nameof(ShowInvisibles), typeof(bool), typeof(MetaViewerHalo5), new PropertyMetadata(false, ShowInvisiblesChanged));

        public bool ShowInvisibles
        {
            get { return (bool)GetValue(ShowInvisiblesProperty); }
            set { SetValue(ShowInvisiblesProperty, value); }
        }

        public static void ShowInvisiblesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //MetaViewerPlugin.Settings.ShowInvisibles = e.NewValue as bool? ?? false;
        }
        #endregion

        private ModuleItem tag;
        private string fileName;

        public TabModel TabModel { get; }
        public ObservableCollection<MetaValue> Metadata { get; }

        public MetaViewerHalo5()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Document);
            Metadata = new ObservableCollection<MetaValue>();
            DataContext = this;
            ShowInvisibles = true;// MetaViewerPlugin.Settings.ShowInvisibles;
        }

        public void LoadMetadata(ModuleItem tag, string xmlFileName)
        {
            TabModel.ToolTip = $"{tag.FullPath}.{tag.ClassCode}";
            TabModel.Header = $"{Utils.GetFileName(tag.FullPath)}.{tag.ClassCode}";

            this.tag = tag;
            fileName = xmlFileName;

            LoadData();
        }

        private void LoadData()
        {
            Metadata.Clear();

            var doc = new XmlDocument();
            doc.Load(fileName);

            var offset = 0;
            using (var tagReader = tag.CreateReader())
            {
                var header = new MetadataHeader(tagReader);
                using (var reader = tagReader.CreateVirtualReader(header.Header.HeaderSize))
                {
                    var mainBlock = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;
                    if (header.DataBlocks[mainBlock].Section != 1)
                        System.Diagnostics.Debugger.Break();

                    foreach (XmlNode n in doc.DocumentElement.ChildNodes)
                    {
                        try
                        {
                            var def = FieldDefinition.GetDefinition(n);
                            var meta = MetaValue.GetValue(n, tag, header, reader, header.DataBlocks[mainBlock].Offset, offset);
                            Metadata.Add(meta);
                            offset += def.Size;
                        }
                        catch { break; }
                    }
                }
            }
        }

        private void RecursiveToggle(IEnumerable<MetaValue> collection, bool value)
        {
            foreach (StructureValue s in collection.Where(s => s is StructureValue))
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
    }
}
