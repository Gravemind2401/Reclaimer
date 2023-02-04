using Reclaimer.Blam.Common;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MapViewer.xaml
    /// </summary>
    public partial class MapViewer
    {
        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly MenuItem CopyPathContextItem;
        private readonly Separator ContextSeparator;

        private ICacheFile cache;
        private TreeItemModel rootNode;

        #region Dependency Properties
        private static readonly DependencyPropertyKey HasGlobalHandlersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasGlobalHandlers), typeof(bool), typeof(MapViewer), new PropertyMetadata(false));

        public static readonly DependencyProperty HasGlobalHandlersProperty = HasGlobalHandlersPropertyKey.DependencyProperty;

        public static readonly DependencyProperty HierarchyViewProperty =
            DependencyProperty.Register(nameof(HierarchyView), typeof(bool), typeof(MapViewer), new PropertyMetadata(false, HierarchyViewChanged));

        public bool HasGlobalHandlers
        {
            get => (bool)GetValue(HasGlobalHandlersProperty);
            private set => SetValue(HasGlobalHandlersPropertyKey, value);
        }

        public bool HierarchyView
        {
            get => (bool)GetValue(HierarchyViewProperty);
            set => SetValue(HierarchyViewProperty, value);
        }
        #endregion

        public TabModel TabModel { get; }
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
            CopyPathContextItem = new MenuItem { Header = "Copy Path" };
            ContextSeparator = new Separator();

            TabModel = new TabModel(this, TabItemType.Tool);
            ContextItems = new ObservableCollection<UIElement>();

            DataContext = this;
        }

        public void LoadMap(string fileName)
        {
            cache = CacheFactory.ReadCacheFile(fileName);
            rootNode = new TreeItemModel(cache.FileName);
            tv.ItemsSource = rootNode.Items;

            TabModel.Header = Utils.GetFileName(cache.FileName);
            TabModel.ToolTip = $"Map Viewer - {TabModel.Header}";

            foreach (var item in globalMenuButton.MenuItems.OfType<MenuItem>())
                item.Click -= GlobalContextItem_Click;

            globalMenuButton.MenuItems.Clear();

            var globalHandlers = Substrate.GetContextItems(GetFolderArgs(rootNode));
            HasGlobalHandlers = globalHandlers.Any();

            if (HasGlobalHandlers)
            {
                foreach (var item in globalHandlers)
                    globalMenuButton.MenuItems.Add(new MenuItem { Header = item.Path, Tag = item });

                foreach (var item in globalMenuButton.MenuItems.OfType<MenuItem>())
                    item.Click += GlobalContextItem_Click;
            }

            HierarchyView = MapViewerPlugin.Settings.HierarchyView;
            BuildTagTree(null);
        }

        private void BuildTagTree(string filter)
        {
            if (HierarchyView)
                BuildHierarchyTree(filter);
            else
                BuildClassTree(filter);
        }

        private void BuildClassTree(string filter)
        {
            var result = new List<TreeItemModel>();
            var classGroups = cache.TagIndex
                .Where(i => FilterTag(filter, i))
                .GroupBy(i => i.ClassName);

            foreach (var g in classGroups.OrderBy(g => g.Key))
            {
                var node = new TreeItemModel { Header = g.Key };
                foreach (var i in g.OrderBy(i => i.TagName))
                {
                    node.Items.Add(new TreeItemModel
                    {
                        Header = i.TagName,
                        Tag = i
                    });
                }
                result.Add(node);
            }

            rootNode.Items.Reset(result);
        }

        private void BuildHierarchyTree(string filter)
        {
            var result = new List<TreeItemModel>();
            var lookup = new Dictionary<string, TreeItemModel>();

            foreach (var tag in cache.TagIndex.Where(i => FilterTag(filter, i)).OrderBy(i => i.TagName))
            {
                var node = MakeNode(result, lookup, $"{tag.TagName}.{tag.ClassName}");
                node.Tag = tag;
            }

            rootNode.Items.Reset(result);
        }

        private static bool FilterTag(string filter, IIndexItem tag)
        {
            return string.IsNullOrEmpty(filter) || tag.TagName.ToUpper().Contains(filter.ToUpper()) || tag.ClassCode.ToUpper() == filter.ToUpper() || tag.ClassName.ToUpper() == filter.ToUpper();
        }

        private TreeItemModel MakeNode(IList<TreeItemModel> root, IDictionary<string, TreeItemModel> lookup, string path, bool inner = false)
        {
            if (lookup.TryGetValue(path, out var item))
                return item;

            var index = path.LastIndexOf('\\');
            var branch = index < 0 ? null : path[..index];
            var leaf = index < 0 ? path : path[(index + 1)..];

            item = new TreeItemModel(leaf);
            lookup.Add(path, item);

            if (branch == null)
            {
                if (inner)
                    root.Insert(root.LastIndexWhere(n => n.HasItems) + 1, item);
                else
                    root.Add(item);

                return item;
            }

            var parent = MakeNode(root, lookup, branch, true);

            if (inner)
                parent.Items.Insert(parent.Items.LastIndexWhere(n => n.HasItems) + 1, item);
            else
                parent.Items.Add(item);

            return item;
        }

        private void RecursiveCollapseNode(TreeItemModel node)
        {
            foreach (var n in node.Items)
                RecursiveCollapseNode(n);
            node.IsExpanded = false;
        }

        private OpenFileArgs GetFolderArgs(TreeItemModel node) => new OpenFileArgs(node.Header, $"Blam.{cache.CacheType}.*", node);

        private OpenFileArgs GetSelectedArgs()
        {
            var node = tv.SelectedItem as TreeItemModel;
            if (node.HasItems) //folder
                return GetFolderArgs(node);

            var item = node.Tag as IIndexItem;
            var fileName = $"{item.TagName}.{item.ClassName}";
            var fileKey = $"Blam.{cache.CacheType}.{item.ClassCode}";
            return new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), GetFileFormats(item).ToArray());
        }

        private static IEnumerable<object> GetFileFormats(IIndexItem item)
        {
            yield return item;

            if (ContentFactory.TryGetPrimaryContent(item, out var content))
                yield return content;
        }

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in rootNode.Items)
                RecursiveCollapseNode(node);
        }

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            BuildTagTree(txtSearch.Text);
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

            if ((tv.SelectedItem as TreeItemModel)?.Tag is not IIndexItem)
                return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if ((sender as TreeViewItem)?.DataContext != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            foreach (var item in ContextItems.OfType<MenuItem>())
                item.Click -= ContextItem_Click;

            var menu = sender as ContextMenu;
            var node = tv.SelectedItem as TreeItemModel;

            ContextItems.Clear();
            if (node.Tag is IIndexItem)
            {
                ContextItems.Add(OpenContextItem);
                ContextItems.Add(OpenWithContextItem);
                ContextItems.Add(CopyPathContextItem);
            }

            var customItems = Substrate.GetContextItems(GetSelectedArgs());

            if (ContextItems.Any() && customItems.Any())
                ContextItems.Add(ContextSeparator);

            foreach (var item in customItems)
                ContextItems.Add(new MenuItem { Header = item.Path, Tag = item });

            foreach (var item in ContextItems.OfType<MenuItem>())
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
            else if (sender == CopyPathContextItem)
            {
                var tag = args.File.OfType<IIndexItem>().First();
                Clipboard.SetText($"{tag.TagName}.{tag.ClassName}");
            }
            else
                ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(args);
        }

        private void GlobalContextItem_Click(object sender, RoutedEventArgs e)
        {
            ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(GetFolderArgs(rootNode));
        }
        #endregion
    }
}
