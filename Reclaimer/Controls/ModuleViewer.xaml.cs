using Adjutant.Blam.Common;
using Adjutant.Blam.Halo5;
using Adjutant.Utilities;
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
    /// Interaction logic for ModuleViewer.xaml
    /// </summary>
    public partial class ModuleViewer
    {
        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly MenuItem OpenFromContextItem;
        private readonly Separator ContextSeparator;

        private Module module;

        public static readonly DependencyProperty HierarchyViewProperty =
            DependencyProperty.Register(nameof(HierarchyView), typeof(bool), typeof(ModuleViewer), new PropertyMetadata(false, HierarchyViewChanged));

        public bool HierarchyView
        {
            get { return (bool)GetValue(HierarchyViewProperty); }
            set { SetValue(HierarchyViewProperty, value); }
        }

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
            ContextSeparator = new Separator();

            TabModel = new TabModel(this, TabItemType.Tool);
            ContextItems = new ObservableCollection<UIElement>();

            DataContext = this;
        }

        public void LoadModule(string fileName)
        {
            module = new Module(fileName);

            TabModel.Header = Utils.GetFileName(module.FileName);
            TabModel.ToolTip = $"Module Viewer - {TabModel.Header}";

            HierarchyView = ModuleViewerPlugin.Settings.HierarchyView;
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
            var result = new List<TreeItemModel>();
            var classGroups = module.GetTagClasses()
                .SelectMany(c => module.GetItemsByClass(c.ClassCode))
                .Where(i => FilterTag(filter, i))
                .GroupBy(i => i.ClassName);

            foreach (var g in classGroups.OrderBy(g => g.Key))
            {
                var node = new TreeItemModel { Header = g.Key };
                foreach (var i in g.OrderBy(i => i.FullPath))
                {
                    node.Items.Add(new TreeItemModel
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
            var result = new List<TreeItemModel>();
            var lookup = new Dictionary<string, TreeItemModel>();

            foreach (var tag in module.Items.Where(i => FilterTag(filter, i)).OrderBy(i => i.FullPath))
            {
                var node = MakeNode(result, lookup, $"{tag.FullPath}.{tag.ClassName}");
                node.Tag = tag;
            }

            tv.ItemsSource = result;
        }

        private bool FilterTag(string filter, ModuleItem tag)
        {
            if (tag.GlobalTagId == -1)
                return false;

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

        private TreeItemModel MakeNode(IList<TreeItemModel> root, IDictionary<string, TreeItemModel> lookup, string path, bool inner = false)
        {
            if (lookup.ContainsKey(path))
                return lookup[path];

            var index = path.LastIndexOf('\\');
            var branch = index < 0 ? null : path.Substring(0, index);
            var leaf = index < 0 ? path : path.Substring(index + 1);

            var item = new TreeItemModel(leaf);
            lookup.Add(path, item);

            if (branch == null)
            {
                if (inner)
                    root.Insert(root.LastIndexWhere(n => n.HasItems) + 1, item);
                else root.Add(item);

                return item;
            }

            var parent = MakeNode(root, lookup, branch, true);

            if (inner)
                parent.Items.Insert(parent.Items.LastIndexWhere(n => n.HasItems) + 1, item);
            else parent.Items.Add(item);

            return item;
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
                return new OpenFileArgs(node.Header, $"Blam.{module.ModuleType}.*", node);

            return GetSelectedArgs(node.Tag as ModuleItem);
        }

        private OpenFileArgs GetSelectedArgs(ModuleItem item)
        {
            var fileName = $"{item.FullPath}.{item.ClassName}";
            var fileKey = $"Blam.{module.ModuleType}.{item.ClassCode}";
            return new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), GetFileFormats(item).ToArray());
        }

        private IEnumerable<object> GetFileFormats(ModuleItem item)
        {
            yield return item;

            object meta = null;
            try
            {
                if (item.ClassCode == "bitm")
                    meta = item.ReadMetadata<bitmap>();
                else if (item.ClassCode == "mode")
                    meta = item.ReadMetadata<render_model>();
            }
            catch { }

            if (meta != null)
                yield return meta;
        }

        #region Event Handlers
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            var nodes = tv.ItemsSource as List<TreeItemModel>;

            foreach (var node in nodes)
                RecursiveCollapseNode(node);
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

            txtSearch.Clear();
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

            var item = (tv.SelectedItem as TreeItemModel)?.Tag as ModuleItem;
            if (item == null) return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (var item in ContextItems.OfType<MenuItem>().Concat(OpenFromContextItem.Items.OfType<MenuItem>()))
                item.Click -= ContextItem_Click;

            var menu = (sender as ContextMenu);
            var node = tv.SelectedItem as TreeItemModel;

            ContextItems.Clear();
            OpenFromContextItem.Items.Clear();

            var moduleItem = node.Tag as ModuleItem;
            if (moduleItem != null)
            {
                ContextItems.Add(OpenContextItem);
                ContextItems.Add(OpenWithContextItem);

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
            var item = sender as MenuItem;
            if (item == null)
                return;

            var args = GetSelectedArgs();
            if (sender == OpenContextItem)
                Substrate.OpenWithDefault(args);
            else if (sender == OpenWithContextItem)
                Substrate.OpenWithPrompt(args);
            else if (OpenFromContextItem.Items.Contains(item))
                Substrate.OpenWithPrompt(GetSelectedArgs(item.Tag as ModuleItem));
            else ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(args);
        }
        #endregion
    }
}
