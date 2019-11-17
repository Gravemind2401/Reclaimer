using Adjutant.Saber3D.Common;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Reclaimer.Windows;
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

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MapViewer.xaml
    /// </summary>
    public partial class PakViewer : UserControl
    {
        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly Separator ContextSeparator;

        private IPakFile pak;

        public TabModel TabModel { get; }
        public ObservableCollection<UIElement> ContextItems { get; }

        public PakViewer()
        {
            InitializeComponent();

            OpenContextItem = new MenuItem { Header = "Open" };
            OpenWithContextItem = new MenuItem { Header = "Open With..." };
            ContextSeparator = new Separator();

            TabModel = new TabModel(this, TabItemType.Tool);
            ContextItems = new ObservableCollection<UIElement>();

            DataContext = this;
        }

        public void LoadPak(string fileName)
        {
            pak = new Adjutant.Saber3D.Halo1X.PakFile(fileName);

            TabModel.Header = Utils.GetFileName(pak.FileName);
            TabModel.ToolTip = $"Pak Viewer - {TabModel.Header}";

            BuildItemTree(null);
        }

        private void BuildItemTree(string filter)
        {
            var result = new List<TreeItemModel>();
            var itemGroups = pak.Items
                .Where(i => FilterTag(filter, i))
                .GroupBy(i => i.ItemType.ToString());

            foreach (var g in itemGroups.OrderBy(g => g.Key))
            {
                var node = new TreeItemModel { Header = g.Key };
                foreach (var item in g.OrderBy(i => i.Name))
                {
                    node.Items.Add(new TreeItemModel
                    {
                        Header = item.Name,
                        Tag = item
                    });
                }
                result.Add(node);
            }

            tv.ItemsSource = result;
        }

        private bool FilterTag(string filter, IPakItem item)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            if (item.Name.ToUpper() == filter.ToUpper())
                return true;

            return false;
        }

        private void RecursiveCollapseNode(TreeItemModel node)
        {
            foreach (var n in node.Items)
                RecursiveCollapseNode(n);
            node.IsExpanded = false;
        }

        private OpenFileArgs GetSelectedArgs()
        {
            var node = tv.SelectedItem as TreeItemModel;
            if (node.HasItems) //folder
                return new OpenFileArgs(node.Header, $"Saber3D.Halo1X.*", node);

            var item = node.Tag as IPakItem;
            var fileName = $"{item.Name}.{item.ItemType}";
            var fileKey = $"Saber3D.Halo1X.{item.ItemType}";
            return new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), GetFileFormats(item).ToArray());
        }

        private IEnumerable<object> GetFileFormats(IPakItem item)
        {
            yield return item;

            if (item.ItemType == PakItemType.Textures)
                yield return new Adjutant.Saber3D.Halo1X.Texture((Adjutant.Saber3D.Halo1X.PakItem)item);
        }

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            var nodes = tv.ItemsSource as List<TreeItemModel>;

            foreach (var node in nodes)
                RecursiveCollapseNode(node);
        }

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            BuildItemTree(txtSearch.Text);
        }

        private void TreeItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TreeViewItem).IsSelected = true;
        }

        private void TreeItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if ((sender as TreeViewItem)?.DataContext != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            var item = (tv.SelectedItem as TreeItemModel)?.Tag as IPakItem;
            if (item == null) return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (MenuItem item in ContextItems.Where(i => i is MenuItem))
                item.Click -= ContextItem_Click;

            var menu = (sender as ContextMenu);
            var node = tv.SelectedItem as TreeItemModel;

            ContextItems.Clear();
            if (node.Tag is IPakItem)
            {
                ContextItems.Add(OpenContextItem);
                ContextItems.Add(OpenWithContextItem);
            }

            var customItems = Substrate.GetContextItems(GetSelectedArgs());

            if (ContextItems.Any() && customItems.Any())
                ContextItems.Add(ContextSeparator);

            foreach (var item in customItems)
                ContextItems.Add(new MenuItem { Header = item.Path, Tag = item });

            foreach (MenuItem item in ContextItems.Where(i => i is MenuItem))
                item.Click += ContextItem_Click;

            if (!ContextItems.Any())
                e.Handled = true;
        }

        private void ContextItem_Click(object sender, RoutedEventArgs e)
        {
            var args = GetSelectedArgs();
            if (sender == OpenContextItem)
                Substrate.OpenWithDefault(args);
            else if (sender == OpenWithContextItem)
                Substrate.OpenWithPrompt(args);
            else ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(args);
        }
        #endregion
    }
}
