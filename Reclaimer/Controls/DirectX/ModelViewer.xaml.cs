using Adjutant.Geometry;
using Microsoft.Win32;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using Helix = HelixToolkit.Wpf.SharpDX;
using HelixCore = HelixToolkit.SharpDX.Core;
using Media3D = System.Windows.Media.Media3D;

namespace Reclaimer.Controls.DirectX
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : IDisposable
    {
        private delegate void ExportBitmaps(IRenderGeometry geometry);
        private delegate void ExportSelectedBitmaps(IRenderGeometry geometry, IEnumerable<int> shaderIndexes);

        private static readonly string[] AllLods = new[] { "Highest", "High", "Medium", "Low", "Lowest", "Potato" };
        private static readonly Helix.Material ErrorMaterial = Helix.DiffuseMaterials.Gold;

        #region Dependency Properties
        private static readonly DependencyPropertyKey AvailableLodsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(AvailableLods), typeof(IEnumerable<string>), typeof(ModelViewer), new PropertyMetadata());

        public static readonly DependencyProperty AvailableLodsProperty = AvailableLodsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedLodProperty =
            DependencyProperty.Register(nameof(SelectedLod), typeof(int), typeof(ModelViewer), new PropertyMetadata(0, SelectedLodChanged));

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

        public static void SelectedLodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModelViewer)d;
            control.SetLod((int)e.NewValue);
        }
        #endregion

        private readonly Helix.GroupModel3D modelGroup = new Helix.GroupModel3D { IsHitTestVisible = false };

        private IRenderGeometry geometry;
        private IGeometryModel model;

        public TabModel TabModel { get; }
        public ObservableCollection<TreeItemModel> TreeViewItems { get; }

        public Action<string> LogOutput { get; set; }
        public Action<string, Exception> LogError { get; set; }
        public Action<string> SetStatus { get; set; }
        public Action ClearStatus { get; set; }

        public ModelViewer()
        {
            TabModel = new TabModel(this, TabItemType.Document);
            TreeViewItems = new ObservableCollection<TreeItemModel>();
            InitializeComponent();

            renderer.AddChild(modelGroup);
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
            TreeViewItems.Clear();
            ClearChildren();

            model = geometry.ReadGeometry(index);
            var materials = GetMaterials(model).ToList();

            foreach (var region in model.Regions)
            {
                var regNode = new TreeItemModel { Header = region.Name, IsChecked = true };

                foreach (var perm in region.Permutations)
                {
                    if (perm.MeshCount <= 0)
                        continue;

                    var meshes = GetMeshes(model, perm, materials).ToList();
                    if (!meshes.Any())
                        continue;

                    var permNode = new TreeItemModel { Header = perm.Name, IsChecked = true };
                    regNode.Items.Add(permNode);

                    var tGroup = new Media3D.Transform3DGroup();

                    if (perm.TransformScale != 1)
                    {
                        var tform = new Media3D.ScaleTransform3D(perm.TransformScale, perm.TransformScale, perm.TransformScale);

                        tform.Freeze();
                        tGroup.Children.Add(tform);
                    }

                    if (!perm.Transform.IsIdentity)
                    {
                        var tform = new Media3D.MatrixTransform3D(new Media3D.Matrix3D
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

                    Helix.GroupElement3D permGroup;
                    if (tGroup.Children.Count == 0 && meshes.Count == 1)
                        permGroup = meshes[0];
                    else
                    {
                        permGroup = new Helix.GroupModel3D();
                        for (var i = 0; i < meshes.Count; i++)
                            permGroup.Children.Add(meshes[i]);

                        if (tGroup.Children.Count > 0)
                            (permGroup.Transform = tGroup).Freeze();
                    }

                    permNode.Tag = new MeshTag(permGroup, perm);
                    modelGroup.Children.Add(permGroup);
                }

                if (regNode.HasItems)
                    TreeViewItems.Add(regNode);
            }
        }

        private static IEnumerable<Helix.Material> GetMaterials(IGeometryModel model)
        {
            var indexes = model.Meshes.SelectMany(m => m.Submeshes)
                .Select(s => s.MaterialIndex).Distinct().ToArray();

            var bitmapLookup = new Dictionary<int, Helix.Material>();
            for (short i = 0; i < model.Materials.Count; i++)
            {
                if (!indexes.Contains(i))
                {
                    yield return null;
                    continue;
                }

                var mat = model.Materials[i];
                Helix.Material material;

                try
                {
                    var diffuse = mat.Submaterials.First(m => m.Usage == MaterialUsage.Diffuse);
                    if (!bitmapLookup.ContainsKey(diffuse.Bitmap.Id))
                    {
                        var args = new DdsOutputArgs(DecompressOptions.Bgr24);
                        var stream = new System.IO.MemoryStream();
                        diffuse.Bitmap.ToDds(0).WriteToStream(stream, System.Drawing.Imaging.ImageFormat.Png, args);
                        var diffuseTexture = new HelixCore.TextureModel(stream);

                        material = new Helix.DiffuseMaterial
                        {
                            DiffuseMap = diffuseTexture
                        };

                        material.Freeze();
                        bitmapLookup[diffuse.Bitmap.Id] = material;
                    }
                    else
                        material = bitmapLookup[diffuse.Bitmap.Id];
                }
                catch
                {
                    material = ErrorMaterial;
                }

                yield return material;
            }
        }

        private static IEnumerable<Helix.GroupElement3D> GetMeshes(IGeometryModel model, IGeometryPermutation permutation, List<Helix.Material> materials)
        {
            for (var i = 0; i < permutation.MeshCount; i++)
            {
                var mesh = model.Meshes[permutation.MeshIndex + i];
                if (mesh.Submeshes.Count == 0)
                    continue;

                var mGroup = new Helix.GroupModel3D();

                var texMatrix = SharpDX.Matrix.Identity;
                var boundsMatrix = SharpDX.Matrix.Identity;

                if (mesh.BoundsIndex >= 0)
                {
                    var bounds = model.Bounds[mesh.BoundsIndex.Value];
                    boundsMatrix = bounds.ToMatrix3();
                    texMatrix = bounds.ToMatrix2();
                }

                foreach (var sub in mesh.Submeshes)
                {
                    try
                    {
                        var indices = mesh.GetTriangleIndicies(sub);

                        var vertStart = indices.Min();
                        var vertLength = indices.Max() - vertStart + 1;

                        var positions = mesh.GetPositions(vertStart, vertLength).Select(v => new SharpDX.Vector3(v.X, v.Y, v.Z));
                        var texcoords = mesh.GetTexCoords(vertStart, vertLength)?.Select(v => new SharpDX.Vector2(v.X, v.Y));
                        var normals = mesh.GetNormals(vertStart, vertLength)?.Select(v => new SharpDX.Vector3(v.X, v.Y, v.Z));

                        if (!boundsMatrix.IsIdentity)
                            positions = positions.Select(v => SharpDX.Vector3.TransformCoordinate(v, boundsMatrix));

                        if (!texMatrix.IsIdentity)
                            texcoords = texcoords?.Select(v => SharpDX.Vector2.TransformCoordinate(v, texMatrix));

                        var geom = new HelixCore.MeshGeometry3D
                        {
                            Indices = new HelixCore.IntCollection(indices.Select(j => j - vertStart)),
                            Positions = new HelixCore.Vector3Collection(positions)
                        };

                        if (texcoords != null)
                            geom.TextureCoordinates = new HelixCore.Vector2Collection(texcoords);

                        if (normals != null)
                            geom.Normals = new HelixCore.Vector3Collection(normals);

                        geom.UpdateOctree();

                        mGroup.Children.Add(new Helix.MeshGeometryModel3D
                        {
                            Geometry = geom,
                            Material = materials.ElementAtOrDefault(sub.MaterialIndex) ?? ErrorMaterial
                        });
                    }
                    catch { }
                }

                yield return mGroup;
            }
        }

        private IEnumerable<IGeometryPermutation> GetSelectedPermutations()
        {
            return TreeViewItems.Where(i => i.IsChecked != false)
                .SelectMany(i => i.Items.Where(ii => ii.IsChecked == true))
                .Select(i => (i.Tag as MeshTag).Permutation);
        }

        #region Treeview Events
        private void TreeViewItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as TreeItemModel;
            if (item != tv.SelectedItem)
                return; //because this event bubbles to the parent node

            var mesh = (item.Tag as MeshTag)?.Mesh;
            if (mesh != null)
                renderer.LocateObject(mesh);
        }

        private bool isWorking = false;

        private void TreeViewItem_Checked(object sender, RoutedEventArgs e)
        {
            if (isWorking)
                return;

            isWorking = true;
            SetState((e.OriginalSource as FrameworkElement).DataContext as TreeItemModel, true);
            isWorking = false;
        }

        private void SetState(TreeItemModel item, bool updateRender)
        {
            if (item.HasItems == false) //permutation
            {
                var parent = item.Parent;
                var children = parent.Items.Where(i => i.IsVisible);

                if (children.All(i => i.IsChecked == true))
                    parent.IsChecked = true;
                else if (children.All(i => i.IsChecked == false))
                    parent.IsChecked = false;
                else
                    parent.IsChecked = null;

                if (updateRender)
                {
                    var group = (item.Tag as MeshTag).Mesh;
                    if (item.IsChecked == true && !modelGroup.Children.Contains(group))
                        modelGroup.Children.Add(group);
                    else if (item.IsChecked == false)
                        modelGroup.Children.Remove(group);
                }
            }
            else //region
            {
                foreach (var i in item.Items.Where(i => i.IsVisible))
                {
                    var group = (i.Tag as MeshTag).Mesh;
                    i.IsChecked = item.IsChecked;

                    if (updateRender)
                    {
                        if (i.IsChecked == true && !modelGroup.Children.Contains(group))
                            modelGroup.Children.Add(group);
                        else if (i.IsChecked == false)
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

        #region Export Functions
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
            if (!PromptFileSave(out var fileName, out var formatId))
                return;

            ModelViewerPlugin.WriteModelFile(model, fileName, formatId);
        }

        private void btnExportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptFileSave(out var fileName, out var formatId))
                return;

            var masked = new MaskedGeometryModel(model, GetSelectedPermutations());
            ModelViewerPlugin.WriteModelFile(masked, fileName, formatId);
        }

        private void btnExportBitmaps_Click(object sender, RoutedEventArgs e)
        {
            var export = Substrate.GetSharedFunction<ExportBitmaps>(Constants.SharedFuncExportBitmaps);
            export.Invoke(geometry);
        }

        private void btnExportSelectedBitmaps_Click(object sender, RoutedEventArgs e)
        {
            var export = Substrate.GetSharedFunction<ExportSelectedBitmaps>(Constants.SharedFuncExportSelectedBitmaps);
            var matIndices = GetSelectedPermutations()
                .SelectMany(p => Enumerable.Range(p.MeshIndex, p.MeshCount))
                .Select(i => model.Meshes.ElementAtOrDefault(i))
                .Where(m => m != null)
                .SelectMany(m => m.Submeshes.Select(s => (int)s.MaterialIndex))
                .Distinct();

            export.Invoke(geometry, matIndices);
        }
        #endregion
        #endregion

        #region Control Events
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
            ClearChildren();
            modelGroup.Dispose();
            renderer.Dispose();
            geometry = null;
            model = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        private sealed class MeshTag
        {
            public Helix.GroupElement3D Mesh { get; }
            public IGeometryPermutation Permutation { get; }

            public MeshTag(Helix.GroupElement3D mesh, IGeometryPermutation permutation)
            {
                Mesh = mesh;
                Permutation = permutation;
            }
        }
    }
}
