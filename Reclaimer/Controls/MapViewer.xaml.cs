using Adjutant.Blam.Common;
using Reclaimer.Plugins;
using Reclaimer.Utils;
using Reclaimer.Windows;
using Studio.Controls;
using System;
using System.Collections.Generic;
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
        private ICacheFile cache;

        public static readonly DependencyProperty HierarchyViewProperty =
            DependencyProperty.Register(nameof(HierarchyView), typeof(bool), typeof(MapViewer), new PropertyMetadata(false, HierarchyViewChanged));

        public bool HierarchyView
        {
            get { return (bool)GetValue(HierarchyViewProperty); }
            set { SetValue(HierarchyViewProperty, value); }
        }

        public static void HierarchyViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mv = d as MapViewer;
            Plugins.MapViewerPlugin.Settings.HierarchyView = mv.HierarchyView;
            mv.BuildTagTree(mv.txtSearch.Text);
        }

        public MapViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadMap(string fileName)
        {
            cache = CacheFactory.ReadCacheFile(fileName);

            TabHeader = Path.GetFileName(cache.FileName);
            TabToolTip = $"Map Viewer - {TabHeader}";

            HierarchyView = Plugins.MapViewerPlugin.Settings.HierarchyView;
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
                foreach (var i in g.OrderBy(i => i.FileName))
                {
                    node.Children.Add(new TreeNode
                    {
                        Header = i.FileName,
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

            foreach (var tag in cache.TagIndex.Where(i => FilterTag(filter, i)).OrderBy(i => i.FileName))
            {
                var node = MakeNode(result, lookup, $"{tag.FileName}.{tag.ClassName}");
                node.Tag = tag;
            }

            tv.ItemsSource = result;
        }

        private bool FilterTag(string filter, IIndexItem tag)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            if (tag.FileName.ToUpper().Contains(filter.ToUpper()))
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

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            BuildTagTree(txtSearch.Text);
        }

        private void tv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithDefault(args);
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithDefault(args);
        }

        private void menuOpenWith_Click(object sender, RoutedEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithPrompt(args);
        }
        #endregion
    }
}
