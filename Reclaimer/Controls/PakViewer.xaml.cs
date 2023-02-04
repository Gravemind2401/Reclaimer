using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Saber3D.Common;
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
    public partial class PakViewer : UserControl
    {
        private readonly MenuItem OpenContextItem;
        private readonly MenuItem OpenWithContextItem;
        private readonly Separator ContextSeparator;

        private IPakFile pak;
        private TreeItemModel rootNode;

        #region Dependency Properties
        private static readonly DependencyPropertyKey HasGlobalHandlersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasGlobalHandlers), typeof(bool), typeof(PakViewer), new PropertyMetadata(false));

        public static readonly DependencyProperty HasGlobalHandlersProperty = HasGlobalHandlersPropertyKey.DependencyProperty;

        public bool HasGlobalHandlers
        {
            get => (bool)GetValue(HasGlobalHandlersProperty);
            private set => SetValue(HasGlobalHandlersPropertyKey, value);
        }
        #endregion

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
            pak = new Reclaimer.Saber3D.Halo1X.PakFile(fileName);
            rootNode = new TreeItemModel(pak.FileName);
            tv.ItemsSource = rootNode.Items;

            TabModel.Header = Utils.GetFileName(pak.FileName);
            TabModel.ToolTip = $"Pak Viewer - {TabModel.Header}";

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

            rootNode.Items.Reset(result);
        }

        private static bool FilterTag(string filter, IPakItem item)
        {
            return string.IsNullOrEmpty(filter) || item.Name.ToUpper() == filter.ToUpper();
        }

        private void RecursiveCollapseNode(TreeItemModel node)
        {
            foreach (var n in node.Items)
                RecursiveCollapseNode(n);
            node.IsExpanded = false;
        }

        private static OpenFileArgs GetFolderArgs(TreeItemModel node) => new OpenFileArgs(node.Header, $"Saber3D.Halo1X.*", node);

        private OpenFileArgs GetSelectedArgs()
        {
            var node = tv.SelectedItem as TreeItemModel;
            if (node.HasItems) //folder
                return GetFolderArgs(node);

            var item = node.Tag as IPakItem;
            var fileName = $"{item.Name}.{item.ItemType}";
            var fileKey = $"Saber3D.Halo1X.{item.ItemType}";
            return new OpenFileArgs(fileName, fileKey, Substrate.GetHostWindow(this), GetFileFormats(item).ToArray());
        }

        private static IEnumerable<object> GetFileFormats(IPakItem item)
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

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e) => BuildItemTree(txtSearch.Text);

        private void TreeItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => (sender as TreeViewItem).IsSelected = true;

        private void TreeItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if ((sender as TreeViewItem)?.DataContext != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            if ((tv.SelectedItem as TreeItemModel)?.Tag is not IPakItem)
                return;

            Substrate.OpenWithDefault(GetSelectedArgs());
        }

        private void TreeItemContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (var item in ContextItems.OfType<MenuItem>())
                item.Click -= ContextItem_Click;

            var menu = sender as ContextMenu;
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
            else
                ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(args);
        }

        private void GlobalContextItem_Click(object sender, RoutedEventArgs e) => ((sender as MenuItem)?.Tag as PluginContextItem)?.ExecuteHandler(GetFolderArgs(rootNode));
        #endregion
    }
}
