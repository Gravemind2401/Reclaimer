using Adjutant.Geometry;
using Adjutant.Utilities;
using Microsoft.Win32;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Dds;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : IDisposable
    {
        public delegate void ExportBitmaps(IRenderGeometry geometry);

        private static readonly string[] AllLods = new[] { "Highest", "High", "Medium", "Low", "Lowest", "Potato" };
        private static readonly DiffuseMaterial ErrorMaterial;

        #region Dependency Properties
        private static readonly DependencyPropertyKey AvailableLodsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(AvailableLods), typeof(IEnumerable<string>), typeof(ModelViewer), new PropertyMetadata());

        public static readonly DependencyProperty AvailableLodsProperty = AvailableLodsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedLodProperty =
            DependencyProperty.Register(nameof(SelectedLod), typeof(int), typeof(ModelViewer), new PropertyMetadata(0, SelectedLodChanged));

        public IEnumerable<string> AvailableLods
        {
            get { return (IEnumerable<string>)GetValue(AvailableLodsProperty); }
            private set { SetValue(AvailableLodsPropertyKey, value); }
        }

        public int SelectedLod
        {
            get { return (int)GetValue(SelectedLodProperty); }
            set { SetValue(SelectedLodProperty, value); }
        }

        public static void SelectedLodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModelViewer)d;
            control.SetLod((int)e.NewValue);
        }
        #endregion

        private readonly Model3DGroup modelGroup = new Model3DGroup();
        private readonly ModelVisual3D visual = new ModelVisual3D();

        private IRenderGeometry geometry;
        private IGeometryModel model;

        public TabModel TabModel { get; }
        public ObservableCollection<TreeItemModel> TreeViewItems { get; }

        public Action<string> LogOutput { get; set; }
        public Action<string, Exception> LogError { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action ClearStatus { get; set; }

        static ModelViewer()
        {
            (ErrorMaterial = new DiffuseMaterial(Brushes.Gold)).Freeze();
        }

        public ModelViewer()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Document);
            TreeViewItems = new ObservableCollection<TreeItemModel>();
            DataContext = this;

            visual.Content = modelGroup;
            renderer.AddChild(visual);
        }

        public void LoadGeometry(IRenderGeometry geometry, string fileName)
        {
            TabModel.ToolTip = fileName;
            TabModel.Header = Utils.GetFileName(fileName);
            this.geometry = geometry;

            AvailableLods = AllLods.Take(geometry.LodCount);
            SetLod(0);
        }

        private void SetLod(int index)
        {
            model = geometry.ReadGeometry(index);
            var meshes = GetMeshes(model).ToList();

            TreeViewItems.Clear();
            modelGroup.Children.Clear();
            foreach (var region in model.Regions)
            {
                var regNode = new TreeItemModel { Header = region.Name, IsChecked = true };

                foreach (var perm in region.Permutations)
                {
                    var mesh = meshes[perm.MeshIndex];
                    if (mesh == null)
                        continue;

                    var permNode = new TreeItemModel { Header = perm.Name, IsChecked = true };
                    regNode.Items.Add(permNode);

                    var tGroup = new Transform3DGroup();

                    if (perm.TransformScale != 1)
                    {
                        var tform = new ScaleTransform3D(perm.TransformScale, perm.TransformScale, perm.TransformScale);

                        tform.Freeze();
                        tGroup.Children.Add(tform);
                    }

                    if (!perm.Transform.IsIdentity)
                    {
                        var tform = new MatrixTransform3D(new Matrix3D
                        {
                            M11 = perm.Transform.M11,
                            M12 = perm.Transform.M12,
                            M13 = perm.Transform.M13,

                            M21 = perm.Transform.M21,
                            M22 = perm.Transform.M22,
                            M23 = perm.Transform.M23,

                            M31 = perm.Transform.M31,
                            M32 = perm.Transform.M32,
                            M33 = perm.Transform.M33,

                            OffsetX = perm.Transform.M41,
                            OffsetY = perm.Transform.M42,
                            OffsetZ = perm.Transform.M43
                        });

                        tform.Freeze();
                        tGroup.Children.Add(tform);
                    }

                    Model3DGroup permGroup;
                    if (tGroup.Children.Count == 0 && perm.MeshCount == 1)
                        permGroup = meshes[perm.MeshIndex];
                    else
                    {
                        permGroup = new Model3DGroup();
                        for (int i = 0; i < perm.MeshCount; i++)
                        {
                            if (tGroup.Children.Count > 0)
                            {
                                (permGroup.Transform = tGroup).Freeze();

                                permGroup.Children.Add(meshes[perm.MeshIndex + i]);
                                permGroup.Freeze();
                            }
                            else permGroup.Children.Add(meshes[perm.MeshIndex + i]);
                        }
                    }

                    permNode.Tag = permGroup;
                    modelGroup.Children.Add(permGroup);
                }

                if (regNode.HasItems)
                    TreeViewItems.Add(regNode);
            }

            renderer.ScaleToContent(new[] { modelGroup });
        }

        private IEnumerable<Material> GetMaterials(IGeometryModel model)
        {
            var indexes = model.Meshes.SelectMany(m => m.Submeshes)
                .Select(s => s.MaterialIndex).Distinct().ToArray();

            var bitmapLookup = new Dictionary<int, DiffuseMaterial>();
            for (short i = 0; i < model.Materials.Count; i++)
            {
                if (!indexes.Contains(i))
                {
                    yield return null;
                    continue;
                }

                var mat = model.Materials[i];
                DiffuseMaterial material;

                try
                {
                    var diffuse = mat.Submaterials.First(m => m.Usage == MaterialUsage.Diffuse);
                    if (!bitmapLookup.ContainsKey(diffuse.Bitmap.Id))
                    {
                        var dds = diffuse.Bitmap.ToDds(0);

                        var brush = new ImageBrush(dds.ToBitmapSource(DecompressOptions.Bgr24))
                        {
                            ViewportUnits = BrushMappingMode.Absolute,
                            TileMode = TileMode.Tile,
                            Viewport = new Rect(0, 0, 1f / Math.Abs(diffuse.Tiling.X), 1f / Math.Abs(diffuse.Tiling.Y))
                        };

                        brush.Freeze();
                        material = new DiffuseMaterial(brush);
                        material.Freeze();
                        bitmapLookup[diffuse.Bitmap.Id] = material;
                    }
                    else material = bitmapLookup[diffuse.Bitmap.Id];
                }
                catch
                {
                    material = ErrorMaterial;
                }

                yield return material;
            }
        }

        private IEnumerable<Model3DGroup> GetMeshes(IGeometryModel model)
        {
            var indexes = model.Regions.SelectMany(r => r.Permutations)
                .SelectMany(p => Enumerable.Range(p.MeshIndex, p.MeshCount))
                .Distinct().ToList();

            var materials = GetMaterials(model).ToList();

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];

                if (mesh.Submeshes.Count == 0 || !indexes.Contains(i))
                {
                    yield return null;
                    continue;
                }

                var mGroup = new Model3DGroup();
                var tGroup = new Transform3DGroup();

                var texMatrix = Matrix.Identity;
                if (mesh.BoundsIndex >= 0)
                {
                    var bounds = model.Bounds[mesh.BoundsIndex.Value];
                    texMatrix = new Matrix
                    {
                        M11 = bounds.UBounds.Length,
                        M22 = bounds.VBounds.Length,
                        OffsetX = bounds.UBounds.Min,
                        OffsetY = bounds.VBounds.Min
                    };

                    var transform = new Matrix3D
                    {
                        M11 = bounds.XBounds.Length,
                        M22 = bounds.YBounds.Length,
                        M33 = bounds.ZBounds.Length,
                        OffsetX = bounds.XBounds.Min,
                        OffsetY = bounds.YBounds.Min,
                        OffsetZ = bounds.ZBounds.Min
                    };

                    var tform = new MatrixTransform3D(transform);
                    tform.Freeze();
                    tGroup.Children.Add(tform);
                }

                foreach (var sub in mesh.Submeshes)
                {
                    try
                    {
                        var geom = new MeshGeometry3D();

                        var indices = mesh.Indicies.Skip(sub.IndexStart).Take(sub.IndexLength).ToList();
                        if (mesh.IndexFormat == IndexFormat.TriangleStrip) indices = indices.Unstrip().ToList();

                        var vertStart = indices.Min();
                        var vertLength = indices.Max() - vertStart + 1;

                        var verts = mesh.Vertices.Skip(vertStart).Take(vertLength);
                        var positions = verts.Select(v => new Point3D(v.Position[0].X, v.Position[0].Y, v.Position[0].Z));

                        (geom.Positions = new Point3DCollection(positions)).Freeze();
                        (geom.TriangleIndices = new Int32Collection(indices.Select(j => j - vertStart))).Freeze();

                        if (mesh.Vertices[0].TexCoords.Count > 0)
                        {
                            var texcoords = verts.Select(v => new Point(v.TexCoords[0].X, v.TexCoords[0].Y)).ToArray();
                            if (!texMatrix.IsIdentity) texMatrix.Transform(texcoords);
                            (geom.TextureCoordinates = new PointCollection(texcoords)).Freeze();
                        }

                        if (mesh.Vertices[0].Normal.Count > 0)
                        {
                            var normals = verts.Select(v => new Vector3D(v.Normal[0].X, v.Normal[0].Y, v.Normal[0].Z));
                            (geom.Normals = new Vector3DCollection(normals)).Freeze();
                        }

                        var mat = materials.ElementAtOrDefault(sub.MaterialIndex) ?? ErrorMaterial;
                        var subGroup = new GeometryModel3D(geom, mat) { BackMaterial = mat };
                        subGroup.Freeze();
                        mGroup.Children.Add(subGroup);
                    }
                    catch { }
                }

                (mGroup.Transform = tGroup).Freeze();
                mGroup.Freeze();

                yield return mGroup;
            }
        }

        #region Treeview Events
        private void TreeViewItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as TreeItemModel;
            if (item != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            var mesh = item.Tag as Model3DGroup;
            if (mesh != null)
                renderer.LocateObject(mesh);
        }

        private bool isWorking = false;

        private void TreeViewItem_Checked(object sender, RoutedEventArgs e)
        {
            if (isWorking) return;

            isWorking = true;
            SetState((e.OriginalSource as FrameworkElement).DataContext as TreeItemModel, true);
            isWorking = false;
        }

        private void SetState(TreeItemModel item, bool updateRender)
        {
            if (item.HasItems == false)
            {
                var parent = item.Parent as TreeItemModel;
                var children = parent.Items.Where(i => i.IsVisible);

                if (children.All(i => i.IsChecked == true))
                    parent.IsChecked = true;
                else if (children.All(i => i.IsChecked == false))
                    parent.IsChecked = false;
                else parent.IsChecked = null;

                if (updateRender)
                {
                    var group = item.Tag as Model3DGroup;
                    if (item.IsChecked == true && !modelGroup.Children.Contains(group))
                        modelGroup.Children.Add(group);
                    else
                        modelGroup.Children.Remove(group);
                }
            }
            else
            {
                foreach (var i in item.Items.Where(i => i.IsVisible))
                {
                    var group = i.Tag as Model3DGroup;
                    i.IsChecked = item.IsChecked;

                    if (updateRender)
                    {
                        if (i.IsChecked == true && !modelGroup.Children.Contains(group))
                            modelGroup.Children.Add(group);
                        else
                            modelGroup.Children.Remove(group);
                    }
                }
            }
        }
        #endregion

        #region Toolbar Events
        private void btnCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeViewItems)
                item.IsExpanded = false;
        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeViewItems)
                item.IsExpanded = true;
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeViewItems)
                item.IsChecked = true;
        }

        private void btnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in TreeViewItems)
                item.IsChecked = false;
        }

        private bool PromptFileSave(out string fileName, out string formatId)
        {
            var exportFormats = ModelViewerPlugin.GetExportFormats()
                .Select(f => new
                {
                    FormatId = f,
                    Extension = ModelViewerPlugin.GetFormatExtension(f),
                    Description = ModelViewerPlugin.GetFormatDescription(f)
                }).ToList();

            var filter = string.Join("|", exportFormats.Select(f => $"{f.Description}|*.{f.Extension}"));

            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = model.Name,
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

        private void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            string fileName, formatId;
            if (!PromptFileSave(out fileName, out formatId))
                return;

            ModelViewerPlugin.WriteModelFile(model, fileName, formatId);
        }

        private void btnExportSelected_Click(object sender, RoutedEventArgs e)
        {
            string fileName, formatId;
            if (!PromptFileSave(out fileName, out formatId))
                return;

            var selectedPerms = new List<IGeometryPermutation>();
            foreach (var parent in TreeViewItems.Zip(model.Regions, (a, b) => new { Node = a, Region = b }))
            {
                if (parent.Node.IsChecked == false)
                    continue;

                foreach (var child in parent.Node.Items.Zip(parent.Region.Permutations, (a, b) => new { Node = a, Permutation = b }))
                {
                    if (child.Node.IsChecked == true)
                        selectedPerms.Add(child.Permutation);
                }
            }

            var masked = new MaskedGeometryModel(model, selectedPerms);
            ModelViewerPlugin.WriteModelFile(masked, fileName, formatId);
        }

        private void btnExportBitmaps_Click(object sender, RoutedEventArgs e)
        {
            var export = Substrate.GetSharedFunction<ExportBitmaps>("Reclaimer.Plugins.ModelViewerPlugin.ExportBitmaps");
            export.Invoke(geometry);
        }

        private void txtSearch_SearchChanged(object sender, RoutedEventArgs e)
        {
            foreach (var parent in TreeViewItems)
            {
                foreach (var child in parent.Items)
                {
                    child.Visibility = string.IsNullOrEmpty(txtSearch.Text) || child.Header.ToUpperInvariant().Contains(txtSearch.Text.ToUpperInvariant())
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                parent.Visibility = parent.Items.Any(i => i.IsVisible)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            isWorking = true;
            foreach (var item in TreeViewItems)
                SetState(item.Items.First(), false);
            isWorking = false;
        }
        #endregion

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
            var label = sender as Label;
            if (label == null)
                return;

            var anim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1)
            };

            Clipboard.SetText(label.Content?.ToString());
            label.BeginAnimation(OpacityProperty, anim);
        }

        #region IDisposable
        public void Dispose()
        {
            TreeViewItems.Clear();
            renderer.Dispose();
            GC.Collect();
        }
        #endregion
    }
}
