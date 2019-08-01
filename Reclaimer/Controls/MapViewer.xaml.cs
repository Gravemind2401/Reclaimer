using Adjutant.Blam.Common;
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
    public partial class MapViewer : UtilityItem
    {
        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly Separator ContextSeparator;

        private ICacheFile cache;

        public static readonly DependencyProperty HierarchyViewProperty =
            DependencyProperty.Register(nameof(HierarchyView), typeof(bool), typeof(MapViewer), new PropertyMetadata(false, HierarchyViewChanged));

        public bool HierarchyView
        {
            get { return (bool)GetValue(HierarchyViewProperty); }
            set { SetValue(HierarchyViewProperty, value); }
        }

        public ObservableCollection<UIElement> ContextItems { get; }

        public static void HierarchyViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mv = d as MapViewer;
            MapViewerPlugin.Settings.HierarchyView = mv.HierarchyView;
            mv.BuildTagTree(mv.txtSearch.Text);
        }

        public MapViewer()
        {
            InitializeComponent();

            OpenContextItem = new MenuItem { Header = "Open" };
            OpenWithContextItem = new MenuItem { Header = "Open With..." };
            ContextSeparator = new Separator();

            ContextItems = new ObservableCollection<UIElement>();

            DataContext = this;
        }

        public void LoadMap(string fileName)
        {
            cache = CacheFactory.ReadCacheFile(fileName);

            TabHeader = Utils.GetFileName(cache.FileName);
            TabToolTip = $"Map Viewer - {TabHeader}";

            HierarchyView = MapViewerPlugin.Settings.HierarchyView;
            BuildTagTree(null);
        }

        private void BuildTagTree(string filter)
        {
            if (HierarchyView)
                BuildHierarchyTree(filter);
            else BuildClassTree(filter);
        }

        private void BuildClassTree(string filter)
        {
            var result = new List<TreeNode>();
            var classGroups = cache.TagIndex
                .Where(i => FilterTag(filter, i))
                .GroupBy(i => i.ClassName);

            foreach (var g in classGroups.OrderBy(g => g.Key))
            {
                var node = new TreeNode { Header = g.Key };
                foreach (var i in g.OrderBy(i => i.FullPath))
                {
                    node.Children.Add(new TreeNode
                    {
                        Header = i.FullPath,
                        Tag = i
                    });
                }
                result.Add(node);
            }

            tv.ItemsSource = result;
        }

        private void BuildHierarchyTree(string filter)
        {
            var result = new List<TreeNode>();
            var lookup = new Dictionary<string, TreeNode>();

            foreach (var tag in cache.TagIndex.Where(i => FilterTag(filter, i)).OrderBy(i => i.FullPath))
            {
                var node = MakeNode(result, lookup, $"{tag.FullPath}.{tag.ClassName}");
                node.Tag = tag;
            }

            tv.ItemsSource = result;
        }

        private bool FilterTag(string filter, IIndexItem tag)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            if (tag.FullPath.ToUpper().Contains(filter.ToUpper()))
                return true;

            if (tag.ClassCode.ToUpper() == filter.ToUpper())
                return true;

            if (tag.ClassName.ToUpper() == filter.ToUpper())
                return true;

            return false;
        }

        private TreeNode MakeNode(IList<TreeNode> root, IDictionary<string, TreeNode> lookup, string path, bool inner = false)
        {
            if (lookup.ContainsKey(path))
                return lookup[path];

            var index = path.LastIndexOf('\\');
            var branch = index < 0 ? null : path.Substring(0, index);
            var leaf = index < 0 ? path : path.Substring(index + 1);

            var item = new TreeNode(leaf);
            lookup.Add(path, item);

            if (branch == null)
            {
                if (inner)
                    root.Insert(root.LastIndexWhere(n => n.HasChildren) + 1, item);
                else root.Add(item);

                return item;
            }

            var parent = MakeNode(root, lookup, branch, true);

            if (inner)
                parent.Children.Insert(parent.Children.LastIndexWhere(n => n.HasChildren) + 1, item);
            else parent.Children.Add(item);

            return item;
        }

        private void RecursiveCollapseNode(TreeNode node)
        {
            foreach (var n in node.Children)
                RecursiveCollapseNode(n);
        }

        private OpenFileArgs GetSelectedArgs()
        {
            var node = tv.SelectedItem as TreeNode;
            if (node.HasChildren) //folder
                return new OpenFileArgs(node.Header, node, $"{cache.CacheType}.*");

            var item = node.Tag as IIndexItem;
            var fileName = $"{Utils.GetFileName(item.FullPath)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            return new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
        }

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            BuildTagTree(txtSearch.Text);
        }

        private void TreeItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if ((sender as TreeViewItem)?.DataContext != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (MenuItem item in ContextItems.Where(i => i is MenuItem))
                item.Click -= ContextItem_Click;

            var menu = (sender as ContextMenu);
            var node = tv.SelectedItem as TreeNode;

            ContextItems.Clear();
            if (node.Tag is IIndexItem)
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
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

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
