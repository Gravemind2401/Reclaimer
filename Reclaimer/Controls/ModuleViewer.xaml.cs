using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo5;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Studio.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for ModuleViewer.xaml
    /// </summary>
    public partial class ModuleViewer
    {
        private const int FolderNodeType = 0;
        private const int TagNodeType = 1;
        private const int ResourceNodeType = 2;

        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly MenuItem OpenFromContextItem;
        private readonly MenuItem CopyPathContextItem;
        private readonly Separator ContextSeparator;

        private Module module;
        private TreeItemModel rootNode;

        #region Dependency Properties
        private static readonly DependencyPropertyKey HasGlobalHandlersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasGlobalHandlers), typeof(bool), typeof(ModuleViewer), new PropertyMetadata(false));

        public static readonly DependencyProperty HasGlobalHandlersProperty = HasGlobalHandlersPropertyKey.DependencyProperty;

        public static readonly DependencyProperty HierarchyViewProperty =
            DependencyProperty.Register(nameof(HierarchyView), typeof(bool), typeof(ModuleViewer), new PropertyMetadata(false, HierarchyViewChanged));

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
            var mv = d as ModuleViewer;
            ModuleViewerPlugin.Settings.HierarchyView = mv.HierarchyView;
            mv.BuildTagTree(mv.txtSearch.Text);
        }

        public ModuleViewer()
        {
            InitializeComponent();

            OpenContextItem = new MenuItem { Header = "Open" };
            OpenWithContextItem = new MenuItem { Header = "Open With..." };
            OpenFromContextItem = new MenuItem { Header = "Open From..." };
            CopyPathContextItem = new MenuItem { Header = "Copy Path" };
            ContextSeparator = new Separator();

            TabModel = new TabModel(this, TabItemType.Tool);
            ContextItems = new ObservableCollection<UIElement>();

            DataContext = this;
        }

        public void LoadModule(string fileName)
        {
            module = new Module(fileName);
            rootNode = new TreeItemModel(module.FileName);
            tv.ItemsSource = rootNode.Items;

            TabModel.Header = Utils.GetFileName(module.FileName);
            TabModel.ToolTip = $"Module Viewer - {TabModel.Header}";

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

            HierarchyView = ModuleViewerPlugin.Settings.HierarchyView;
            BuildTagTree(null);
        }

        private void BuildTagTree(string filter)
        {
            if (HierarchyView)
                BuildHierarchyTree(filter);
            else
                BuildClassTree(filter);

            void BuildClassTree(string filter)
            {
                var result = new List<TreeItemModel>();
                var classGroups = module.GetTagClasses()
                    .SelectMany(c => module.GetItemsByClass(c.ClassCode))
                    .Where(i => FilterTag(filter, i))
                    .GroupBy(i => i.ClassName);

                foreach (var g in classGroups.OrderBy(g => g.Key))
                {
                    var classNode = new TreeItemModel { Header = g.Key, ItemType = FolderNodeType };
                    foreach (var i in g.OrderBy(i => i.TagName))
                    {
                        var tagNode = new TreeItemModel
                        {
                            Header = i.TagName,
                            ItemType = TagNodeType,
                            Tag = i
                        };

                        AppendResourceNodes(tagNode, i);
                        classNode.Items.Add(tagNode);
                    }
                    result.Add(classNode);
                }

                rootNode.Items.Reset(result);
            }

            void BuildHierarchyTree(string filter)
            {
                var result = new List<TreeItemModel>();
                var lookup = new Dictionary<string, TreeItemModel>();

                foreach (var tag in module.GetLinkedItems().Where(i => FilterTag(filter, i)).OrderBy(i => i.TagName))
                {
                    var node = MakeNode(result, lookup, Utils.ChangeExtension(tag.TagName, tag.ClassName));
                    node.ItemType = TagNodeType;
                    node.Tag = tag;
                    AppendResourceNodes(node, tag);
                }

                rootNode.Items.Reset(result);
            }

            void AppendResourceNodes(TreeItemModel treeItem, ModuleItem tag)
            {
                //TODO: make a setting to enable/disable this, also one day view the resources in the meta viewer?
                return;

                foreach (var resourceItem in Enumerable.Range(tag.ResourceIndex, tag.ResourceCount).Select(i => tag.Module.Items[tag.Module.Resources[i]]))
                    treeItem.Items.Add(new TreeItemModel { Header = resourceItem.FileName, ItemType = ResourceNodeType, Tag = resourceItem });
            }

            static bool FilterTag(string filter, ModuleItem tag)
            {
                if (tag.GlobalTagId == -1)
                    return false;

                return string.IsNullOrEmpty(filter)
                    || tag.TagName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                    || tag.ClassCode.Equals(filter, StringComparison.OrdinalIgnoreCase)
                    || tag.ClassName.Equals(filter, StringComparison.OrdinalIgnoreCase);
            }

            TreeItemModel MakeNode(IList<TreeItemModel> root, IDictionary<string, TreeItemModel> lookup, string path, bool inner = false)
            {
                if (lookup.TryGetValue(path, out var item))
                    return item;

                var index = path.LastIndexOf('\\');
                var branch = index < 0 ? null : path[..index];
                var leaf = index < 0 ? path : path[(index + 1)..];

                item = new TreeItemModel(leaf) { ItemType = FolderNodeType };
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
        }

        private OpenFileArgs GetFolderArgs(TreeItemModel node) => new OpenFileArgs(node.Header, $"Blam.{module.ModuleType}.*", node);

        private OpenFileArgs GetSelectedArgs()
        {
            var node = tv.SelectedItem as TreeItemModel;
            return node.Tag is ModuleItem moduleItem
                ? GetSelectedArgs(moduleItem)
                : GetFolderArgs(node); //folder
        }

        private OpenFileArgs GetSelectedArgs(ModuleItem item)
        {
            var fileName = $"{item.TagName}.{item.ClassName}";
            var fileKey = $"Blam.{module.ModuleType}.{item.ClassCode}";
            return new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), GetFileFormats(item).ToArray());
        }

        private static IEnumerable<object> GetFileFormats(ModuleItem item)
        {
            yield return item;

            object content;
            try { ContentFactory.TryGetPrimaryContent(item, out content); }
            catch { content = null; }

            if (content != null)
                yield return content;
        }

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in rootNode.Items)
                node.CollapseAll();
        }

        private void btnAddLink_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Halo Module Files|*.module",
                Multiselect = true,
                CheckFileExists = true
            };

            if (!string.IsNullOrEmpty(ModuleViewerPlugin.Settings.ModuleFolder))
                ofd.InitialDirectory = ModuleViewerPlugin.Settings.ModuleFolder;

            if (ofd.ShowDialog() != true)
                return;

            foreach (var fileName in ofd.FileNames)
                module.AddLinkedModule(fileName);

            BuildTagTree(txtSearch.Text);
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

            if ((tv.SelectedItem as TreeItemModel)?.Tag is not ModuleItem)
                return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if ((sender as TreeViewItem)?.DataContext != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            foreach (var item in ContextItems.OfType<MenuItem>().Concat(OpenFromContextItem.Items.OfType<MenuItem>()))
                item.Click -= ContextItem_Click;

            var menu = sender as ContextMenu;
            var node = tv.SelectedItem as TreeItemModel;

            ContextItems.Clear();
            OpenFromContextItem.Items.Clear();

            if (node.Tag is ModuleItem moduleItem)
            {
                ContextItems.Add(OpenContextItem);
                ContextItems.Add(OpenWithContextItem);

                if (moduleItem.GlobalTagId >= 0)
                {
                    var instances = moduleItem.Module.FindAlternateTagInstances(moduleItem.GlobalTagId).ToList();
                    if (instances.Count > 1)
                    {
                        foreach (var instance in instances)
                        {
                            var item = new MenuItem { Header = Utils.GetFileNameWithoutExtension(instance.Module.FileName), Tag = instance };
                            OpenFromContextItem.Items.Add(item);
                        }

                        ContextItems.Add(OpenFromContextItem);
                    }
                }

                ContextItems.Add(CopyPathContextItem);
            }

            var customItems = Substrate.GetContextItems(GetSelectedArgs());

            if (ContextItems.Any() && customItems.Any())
                ContextItems.Add(ContextSeparator);

            foreach (var item in customItems)
                ContextItems.Add(new MenuItem { Header = item.Path, Tag = item });

            foreach (var item in ContextItems.OfType<MenuItem>().Concat(OpenFromContextItem.Items.OfType<MenuItem>()))
                item.Click += ContextItem_Click;

            if (!ContextItems.Any())
                e.Handled = true;
        }

        private void ContextItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item)
                return;

            var args = GetSelectedArgs();
            if (sender == OpenContextItem)
                Substrate.OpenWithDefault(args);
            else if (sender == OpenWithContextItem)
                Substrate.OpenWithPrompt(args);
            else if (OpenFromContextItem.Items.Contains(item))
                Substrate.OpenWithPrompt(GetSelectedArgs(item.Tag as ModuleItem));
            else if (sender == CopyPathContextItem)
            {
                var tag = args.File.OfType<ModuleItem>().First();
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
