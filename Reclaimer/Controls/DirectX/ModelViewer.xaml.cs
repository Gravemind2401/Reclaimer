﻿using HelixToolkit.Wpf.SharpDX;
using Microsoft.Win32;
using Reclaimer.Geometry;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Studio.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Reclaimer.Controls.DirectX
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : IDisposable
    {
        private delegate void WriteModelFile(IContentProvider<Scene> provider, string fileName, string formatId);
        private delegate void ExportBitmaps(IContentProvider<Scene> provider, bool filtered, bool async);

        private static readonly string[] AllLods = new[] { "Highest", "High", "Medium", "Low", "Lowest", "Potato" };

        #region Dependency Properties
        private static readonly DependencyPropertyKey AvailableLodsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(AvailableLods), typeof(IEnumerable<string>), typeof(ModelViewer), new PropertyMetadata());

        public static readonly DependencyProperty AvailableLodsProperty = AvailableLodsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedLodProperty =
            DependencyProperty.Register(nameof(SelectedLod), typeof(int), typeof(ModelViewer), new PropertyMetadata(0, SelectedLodChanged));

        public static readonly DependencyProperty TreeTabsVisibilityProperty =
            DependencyProperty.Register(nameof(TreeTabsVisibility), typeof(Visibility), typeof(ModelViewer), new PropertyMetadata(Visibility.Collapsed));

        public IEnumerable<string> AvailableLods
        {
            get => (IEnumerable<string>)GetValue(AvailableLodsProperty);
            private set => SetValue(AvailableLodsPropertyKey, value);
        }

        public int SelectedLod
        {
            get => (int)GetValue(SelectedLodProperty);
            set => SetValue(SelectedLodProperty, value);
        }

        public Visibility TreeTabsVisibility
        {
            get => (Visibility)GetValue(TreeTabsVisibilityProperty);
            set => SetValue(TreeTabsVisibilityProperty, value);
        }

        //TODO: LODs
        public static void SelectedLodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var control = (ModelViewer)d;
            //control.SetLod((int)e.NewValue);
        }
        #endregion

        private readonly GroupModel3D modelGroup = new GroupModel3D { IsHitTestVisible = false };
        private readonly Dictionary<TreeItemModel, TreeItemModel> treeItemsMap = new();

        private TreeView currentTreeView => treeTabs.SelectedContent as TreeView;
        private ObservableCollection<TreeItemModel> currentTreeViewItems => currentTreeView.ItemsSource as ObservableCollection<TreeItemModel>;

        private ICachedContentProvider<Scene> sceneProvider;
        private TextureLoader textureLoader;
        private MeshLoaderFactory meshLoaderFactory;

        public TabModel TabModel { get; }
        public ObservableCollection<TreeItemModel> TreeViewItems { get; } = new();
        public ObservableCollection<TreeItemModel> PermutationViewItems { get; } = new();

        public Action<string> LogOutput { get; set; }
        public Action<string, Exception> LogError { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action ClearStatus { get; set; }

        public ModelViewer()
        {
            TabModel = new TabModel(this, TabItemType.Document);
            InitializeComponent();

            renderer.AddChild(modelGroup);
        }

        public void LoadGeometry(IContentProvider<Scene> provider)
        {
            sceneProvider = provider.AsCached();
            TabModel.ToolTip = provider.Name;
            TabModel.Header = Utils.GetFileName(provider.Name);

            AvailableLods = AllLods.Take(1); // model.LodCount);

            TreeViewItems.Clear();
            PermutationViewItems.Clear();
            treeItemsMap.Clear();
            ClearChildren();

            var scene = sceneProvider.Content;
            textureLoader = new TextureLoader(scene);
            meshLoaderFactory = new MeshLoaderFactory(textureLoader);

            renderer.SetCoordinateSystem(scene.CoordinateSystem);

            if (scene.ChildGroups.Count == 0 && scene.ChildObjects.Count == 1 && scene.ChildObjects[0].Object is Model m)
            {
                //load in single-model mode - displays permutation tree instead of object tree
                LoadGeometry(m);
                return;
            }

            var isFirst = true;
            var sceneBounds = default(SharpDX.BoundingBox);

            AppendSceneGroups(scene.ChildGroups, TreeViewItems);
            renderer.SetDefaultBounds(sceneBounds);
            return;

            void AppendSceneGroups(List<SceneGroup> groups, ObservableCollection<TreeItemModel> destination)
            {
                foreach (var group in groups.OrderBy(g => g.Name))
                {
                    var groupNode = new TreeItemModel { Header = group.Name, IsChecked = true, Tag = new MeshTag(group) };

                    AppendSceneGroups(group.ChildGroups, groupNode.Items);

                    foreach (var placement in group.ChildObjects.OrderBy(o => o.Name))
                    {
                        if (placement.Object is not Model model)
                            continue;

                        var objNode = new TreeItemModel { Header = model.Name, IsChecked = true };
                        var meshLoader = meshLoaderFactory.CreateMeshLoader(model);
                        var objGroup = new GroupModel3D();

                        if (placement != null && !placement.Transform.IsIdentity)
                        {
                            objGroup.Transform = placement.Transform.ToMediaTransform();
                            objGroup.Transform.Freeze();
                        }

                        foreach (var region in model.Regions)
                        {
                            foreach (var perm in region.Permutations)
                            {
                                if (!perm.MeshIndices.Any())
                                    continue;

                                var tag = meshLoader.GetMesh(perm);
                                if (tag == null)
                                    continue;

                                if (!objGroup.Children.Contains(tag.Mesh))
                                    objGroup.Children.Add(tag.Mesh);
                            }
                        }

                        if (!objGroup.Children.Any())
                            continue;

                        modelGroup.Children.Add(objGroup);

                        if (model.Flags.HasFlag(SceneFlags.PrimaryFocus))
                        {
                            var objBounds = objGroup.GetTotalBounds();
                            if (!isFirst)
                                SharpDX.BoundingBox.Merge(ref sceneBounds, ref objBounds, out sceneBounds);
                            else
                            {
                                isFirst = false;
                                sceneBounds = objBounds;
                            }
                        }

                        var objTag = new MeshTag(placement, objGroup);
                        objNode.Tag = objTag;
                        groupNode.Items.Add(objNode);
                    }

                    if (groupNode.HasItems)
                        destination.Add(groupNode);
                }
            }
        }

        public void LoadGeometry(Model model)
        {
            var meshLoader = meshLoaderFactory.CreateMeshLoader(model);

            foreach (var region in model.Regions)
            {
                var regNode = new TreeItemModel { Header = region.Name, IsChecked = true, Tag = new MeshTag(region) };

                foreach (var perm in region.Permutations)
                {
                    if (!perm.MeshIndices.Any())
                        continue;

                    var tag = meshLoader.GetMesh(perm);
                    if (tag == null)
                        continue;

                    var permNode = new TreeItemModel { Header = perm.Name, IsChecked = true, Tag = tag };
                    regNode.Items.Add(permNode);

                    if (!modelGroup.Children.Contains(tag.Mesh))
                        modelGroup.Children.Add(tag.Mesh);
                }

                if (regNode.HasItems)
                    TreeViewItems.Add(regNode);
            }

            var permutationGroups = TreeViewItems
                .SelectMany(r => r.Items, (r, p) => (RegionItem: r, PermutationItem: p))
                .GroupBy(x => x.PermutationItem.Header)
                .ToList();

            //only populate permutation view if there are permutations that appear in multiple regions
            //and there are no more than {maxNodes} top-level regions or {maxNodes} unique permutations (to try to exclude bsps)
            const int maxNodes = 55;
            var usePermutations = TreeViewItems.Count <= maxNodes
                && permutationGroups.Count <= maxNodes
                && permutationGroups.Where(g => g.Skip(1).Any()).Any();

            TreeTabsVisibility = usePermutations ? Visibility.Visible : Visibility.Collapsed;

            if (usePermutations)
            {
                foreach (var g in permutationGroups)
                {
                    var permNode = new TreeItemModel { Header = g.Key, IsChecked = true };

                    foreach (var (srcRegionNode, srcPermNode) in g)
                    {
                        var regNode = new TreeItemModel { Header = srcRegionNode.Header, IsChecked = true };
                        permNode.Items.Add(regNode);

                        treeItemsMap.Add(regNode, srcPermNode);
                        treeItemsMap.Add(srcPermNode, regNode);
                    }

                    PermutationViewItems.Add(permNode);
                }
            }
        }

        #region Treeview Events
        private void TreeViewItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as TreeItemModel;
            if (item != currentTreeView.SelectedItem)
                return; //because this event bubbles to the parent node

            if (item.Tag == null && treeItemsMap.TryGetValue(item, out var otherItem))
                item = otherItem;

            if (item.Tag is not MeshTag t)
                return;

            if (t.Instance != null)
            {
                var bounds = Enumerable.Repeat(t.Mesh, 1).GetTotalBounds(true);
                var newMin = SharpDX.Vector3.TransformCoordinate(bounds.Minimum, t.Instance.Transform);
                var newMax = SharpDX.Vector3.TransformCoordinate(bounds.Maximum, t.Instance.Transform);
                var box = new SharpDX.BoundingBox(newMin, newMax);
                renderer.ZoomToBounds(box);
            }
            else if (t.Mesh != null)
                renderer.LocateObject(t.Mesh);
        }

        private bool isWorking = false;

        private void TreeViewItem_Checked(object sender, RoutedEventArgs e)
        {
            if (isWorking)
                return;

            isWorking = true;

            var item = (e.OriginalSource as FrameworkElement).DataContext as TreeItemModel;
            OnStateChanged(item, true, true);

            isWorking = false;
        }

        private void OnStateChanged(TreeItemModel item, bool updateRender, bool refreshParent)
        {
            foreach (var child in item.EnumerateHierarchy(i => i.IsVisible))
            {
                child.IsChecked = item.IsChecked;
                if (updateRender && child.Tag is MeshTag tag)
                    tag.SetVisible(child.IsChecked.GetValueOrDefault());

                if (treeItemsMap.TryGetValue(child, out var otherItem) && otherItem.IsChecked != child.IsChecked)
                {
                    otherItem.IsChecked = child.IsChecked;
                    OnStateChanged(otherItem, updateRender, refreshParent);
                }
            }

            if (refreshParent)
                RefreshState(item.Parent);

            static void RefreshState(TreeItemModel item)
            {
                if (item == null || !item.HasItems)
                    return;

                var prev = item.IsChecked;
                item.IsChecked = item.Items
                    .Where(i => i.IsVisible)
                    .Select(i => i.IsChecked)
                    .Aggregate(item.Items[0].IsChecked, (a, b) => a == null || a != b ? null : a);

                //if this node changed state then the parent needs to refresh state too
                if (item.IsChecked != prev)
                    RefreshState(item.Parent);
            }
        }
        #endregion

        #region Toolbar Events
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e) => ExpandAll(false);
        private void btnExpandAll_Click(object sender, RoutedEventArgs e) => ExpandAll(true);
        private void btnSelectAll_Click(object sender, RoutedEventArgs e) => CheckAll(true);
        private void btnSelectNone_Click(object sender, RoutedEventArgs e) => CheckAll(false);

        private void ExpandAll(bool expanded)
        {
            foreach (var item in currentTreeViewItems)
                item.ExpandAll(expanded);
        }

        private void CheckAll(bool checkState)
        {
            isWorking = true;
            foreach (var item in currentTreeViewItems.Where(i => i.IsVisible))
            {
                item.IsChecked = checkState;
                OnStateChanged(item, true, PermutationViewItems.Any());
            }
            isWorking = false;
        }

        #region Export Functions
        private void btnExportAll_Click(object sender, RoutedEventArgs e) => ExportScene(false);
        private void btnExportSelected_Click(object sender, RoutedEventArgs e) => ExportScene(true);

        private bool PromptFileSave(out string fileName, out string formatId)
        {
            var exportFormats = ModelViewerPlugin.GetExportFormats()
                .Select(f => new
                {
                    FormatId = f,
                    Extension = ModelViewerPlugin.GetFormatExtension(f),
                    Description = ModelViewerPlugin.GetFormatDescription(f)
                }).ToList();

            //multi-model exports only supported with RMF
            if (sceneProvider.Content.EnumerateModels().Skip(1).Any())
                exportFormats.RemoveAll(f => f.FormatId != FormatId.RMF);

            var filter = string.Join("|", exportFormats.Select(f => $"{f.Description}|*.{f.Extension}"));

            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = Utils.GetFileName(string.IsNullOrWhiteSpace(sceneProvider.Content.Name) ? sceneProvider.Name : sceneProvider.Content.Name),
                Filter = filter,
                FilterIndex = 1 + exportFormats.TakeWhile(f => f.FormatId != ModelViewerPlugin.Settings.DefaultSaveFormat).Count(),
                AddExtension = true
            };

            if (sfd.ShowDialog() != true)
            {
                fileName = formatId = null;
                return false;
            }

            fileName = sfd.FileName;
            formatId = exportFormats[sfd.FilterIndex - 1].FormatId;
            ModelViewerPlugin.Settings.DefaultSaveFormat = formatId;
            return true;
        }

        private void RefreshExportFlags(bool filtered)
        {
            foreach (var item in TreeViewItems.SelectMany(i => i.EnumerateHierarchy()))
                SetSelected((item.Tag as MeshTag)?.Context, item.IsChecked.GetValueOrDefault(true) || !filtered);

            static void SetSelected(object context, bool selected)
            {
                if (context is ModelPermutation permutation)
                    permutation.Export = selected;
                else if (context is ModelRegion region)
                    region.Export = selected;
                else if (context is SceneObject obj)
                    obj.Export = selected;
                else if (context is SceneGroup group)
                    group.Export = selected;
            }
        }

        private void ExportScene(bool filtered)
        {
            if (!PromptFileSave(out var fileName, out var formatId))
                return;

            RefreshExportFlags(filtered);

            try
            {
                var export = Substrate.GetSharedFunction<WriteModelFile>(Constants.SharedFuncWriteModelFile);
                export.Invoke(sceneProvider, fileName, formatId);
            }
            catch (Exception ex)
            {
                LogOutput(ex.ToString());
            }
        }

        private void btnExportBitmaps_Click(object sender, RoutedEventArgs e)
        {
            var export = Substrate.GetSharedFunction<ExportBitmaps>(Constants.SharedFuncExportBitmaps);
            export.Invoke(sceneProvider, false, true);
        }

        private void btnExportSelectedBitmaps_Click(object sender, RoutedEventArgs e)
        {
            RefreshExportFlags(true);
            var export = Substrate.GetSharedFunction<ExportBitmaps>(Constants.SharedFuncExportBitmaps);
            export.Invoke(sceneProvider, true, true);
        }
        #endregion
        #endregion

        #region Control Events
        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            ApplySearch(txtSearch.Text, currentTreeViewItems);
        }

        private void treeTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                return;

            var previousTreeView = e.RemovedItems.OfType<TabItem>().FirstOrDefault()?.Content as TreeView;
            if (previousTreeView?.ItemsSource is not IEnumerable<TreeItemModel> previousItems)
                return;

            ApplySearch(null, previousItems);
            ApplySearch(txtSearch.Text, currentTreeViewItems);
        }

        private void ApplySearch(string searchTerm, IEnumerable<TreeItemModel> treeItems)
        {
            foreach (var parent in treeItems)
            {
                foreach (var child in parent.Items)
                {
                    child.Visibility = string.IsNullOrEmpty(searchTerm) || child.Header.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant())
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                parent.Visibility = parent.Items.Any(i => i.IsVisible)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            //refresh parent item check states to reflect visible children only
            isWorking = true;
            foreach (var item in treeItems)
                OnStateChanged(item.Items.First(), false, true);
            isWorking = false;
        }

        private void btnToggleDetails_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle.IsChecked == true)
            {
                toggle.Tag = SplitPanel.GetDesiredSize(splitPanel.Children[0]);
                SplitPanel.SetDesiredSize(splitPanel.Children[0], new GridLength(0));
                splitPanel.SplitterThickness = 0;
            }
            else
            {
                SplitPanel.SetDesiredSize(splitPanel.Children[0], (GridLength)toggle.Tag);
                splitPanel.SplitterThickness = 3;
            }
        }

        private void PosLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Label label)
                return;

            var anim = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1)
            };

            Clipboard.SetText(label.Content?.ToString());
            label.BeginAnimation(OpacityProperty, anim);
        }

        private void ClearChildren()
        {
            foreach (var element in modelGroup.Children.ToList())
            {
                modelGroup.Children.Remove(element);
                element.Dispose();
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            TreeViewItems.Clear();
            PermutationViewItems.Clear();
            treeItemsMap.Clear();
            ClearChildren();
            modelGroup.Dispose();
            renderer.Dispose();
            meshLoaderFactory.Dispose();
            textureLoader.Dispose();
            DataContext = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        private sealed class MeshTag
        {
            public object Context { get; }
            public GroupElement3D Mesh { get; }
            public MeshLoader.InstancedPermutation Instance { get; }

            public MeshTag(object context)
            {
                Context = context;
            }

            public MeshTag(object context, GroupElement3D mesh)
                : this(context)
            {
                Mesh = mesh;
            }

            public MeshTag(object context, GroupElement3D mesh, MeshLoader.InstancedPermutation instance)
                : this(context, mesh)
            {
                Instance = instance;
            }

            public void SetVisible(bool visible)
            {
                if (Instance != null)
                    Instance.Toggle(visible);
                else if (Mesh != null)
                    Mesh.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
