using Adjutant.Blam.Common;
using Reclaimer.Plugins.MetaViewer;
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
    /// Interaction logic for MetaViewer.xaml
    /// </summary>
    public partial class MetaViewer : DocumentItem
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

        public ObservableCollection<MetaValue> Metadata { get; }

        public MetaViewer()
        {
            InitializeComponent();
            Metadata = new ObservableCollection<MetaValue>();
            DataContext = this;
            ShowInvisibles = MetaViewerPlugin.Settings.ShowInvisibles;
        }

        public void LoadMetadata(IIndexItem tag, XmlDocument definition)
        {
            TabToolTip = $"{tag.FullPath}.{tag.ClassCode}";
            TabHeader = $"{Path.GetFileName(tag.FullPath)}.{tag.ClassCode}";

            foreach (XmlNode n in definition.DocumentElement.ChildNodes)
            {
                try
                {
                    var meta = MetaValue.GetValue(n, tag.CacheFile, tag.MetaPointer.Address);
                    Metadata.Add(meta);
                }
                catch { }
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
