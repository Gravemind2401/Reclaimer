using Adjutant.Geometry;
using Adjutant.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Dds;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : ControlBase, ITabContent, IDisposable
    {
        private static readonly DiffuseMaterial ErrorMaterial;

        #region Dependency Properties
        public static readonly DependencyProperty SelectedLodProperty =
            DependencyProperty.Register(nameof(SelectedLod), typeof(int), typeof(ModelViewer), new PropertyMetadata(0, SelectedIndexPropertyChanged));

        public int SelectedLod
        {
            get { return (int)GetValue(SelectedLodProperty); }
            set { SetValue(SelectedLodProperty, value); }
        }

        public static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModelViewer)d;
            control.SetLod((int)e.NewValue);
        } 
        #endregion

        TabItemUsage ITabContent.TabUsage => TabItemUsage.Document;

        private readonly Model3DGroup modelGroup = new Model3DGroup();
        private readonly ModelVisual3D visual = new ModelVisual3D();

        private IRenderGeometry geometry;

        public IEnumerable<int> Indexes { get; private set; }
        public ObservableCollection<ExtendedTreeViewItem> TreeViewItems { get; }

        static ModelViewer()
        {
            (ErrorMaterial = new DiffuseMaterial(Brushes.Gold)).Freeze();
        }

        public ModelViewer()
        {
            InitializeComponent();
            TreeViewItems = new ObservableCollection<ExtendedTreeViewItem>();
            DataContext = this;

            visual.Content = modelGroup;
            renderer.AddChild(visual);
        }

        public void LoadGeometry(IRenderGeometry geometry, string fileName)
        {
            TabToolTip = fileName;
            TabHeader = Path.GetFileName(fileName);
            this.geometry = geometry;

            Indexes = Enumerable.Range(0, geometry.LodCount);
            SetLod(0);
            RaisePropertyChanged(nameof(Indexes));
        }

        private void SetLod(int index)
        {
            var model = geometry.ReadGeometry(index);
            var meshes = GetMeshes(model).ToList();

            TreeViewItems.Clear();
            modelGroup.Children.Clear();
            foreach (var region in model.Regions)
            {
                var regNode = new ExtendedTreeViewItem { Header = region.Name, IsChecked = true };

                foreach (var perm in region.Permutations)
                {
                    var mesh = meshes[perm.MeshIndex];
                    if (mesh == null)
                        continue;

                    var permNode = new ExtendedTreeViewItem { Header = perm.Name, IsChecked = true };
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
                    if (tGroup.Children.Count > 0)
                    {
                        permGroup = new Model3DGroup();
                        (permGroup.Transform = tGroup).Freeze();

                        permGroup.Children.Add(meshes[perm.MeshIndex]);
                        permGroup.Freeze();
                    }
                    else permGroup = meshes[perm.MeshIndex];

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
                    var dds = mat.Diffuse.ToDds(0);

                    var brush = new ImageBrush(dds.ToBitmapSource(DecompressOptions.Bgr24))
                    {
                        ViewportUnits = BrushMappingMode.Absolute,
                        TileMode = TileMode.Tile,
                        Viewport = new Rect(0, 0, 1f / Math.Abs(mat.Tiling.X), 1f / Math.Abs(mat.Tiling.Y))
                    };

                    brush.Freeze();
                    material = new DiffuseMaterial(brush);
                    material.Freeze();
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
                .Select(p => p.MeshIndex).Distinct().ToArray();

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
                    var bounds = model.Bounds[mesh.BoundsIndex];
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

                        var indices = mesh.Indicies.Skip(sub.IndexStart).Take(sub.IndexLength).ToArray();
                        if (mesh.IndexFormat == IndexFormat.Stripped) indices = Unstrip(indices).ToArray();

                        var vertStart = indices.Min();
                        var vertLength = indices.Max() - vertStart + 1;

                        var verts = mesh.Vertices.Skip(vertStart).Take(vertLength);
                        var positions = verts.Select(v => new Point3D(v.Position[0].X, v.Position[0].Y, v.Position[0].Z));

                        var texcoords = verts.Select(v => new Point(v.TexCoords[0].X, v.TexCoords[0].Y)).ToArray();
                        if (!texMatrix.IsIdentity) texMatrix.Transform(texcoords);

                        (geom.Positions = new Point3DCollection(positions)).Freeze();
                        (geom.TextureCoordinates = new PointCollection(texcoords)).Freeze();
                        (geom.TriangleIndices = new Int32Collection(indices.Select(j => j - vertStart))).Freeze();

                        if (mesh.Vertices[0].Normal.Count > 0)
                        {
                            var normals = verts.Select(v => new Vector3D(v.Normal[0].X, v.Normal[0].Y, v.Normal[0].Z));
                            (geom.Normals = new Vector3DCollection(normals)).Freeze();
                        }

                        var mat = sub.MaterialIndex >= 0 ? materials[sub.MaterialIndex] : ErrorMaterial;
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

        private IEnumerable<int> Unstrip(IEnumerable<int> strip)
        {
            var arr = strip.ToArray();

            for (int n = 0; n < arr.Length - 2; n++)
            {
                int i1 = arr[n + 0];
                int i2 = arr[n + 1];
                int i3 = arr[n + 2];

                if ((i1 != i2) && (i1 != i3) && (i2 != i3))
                {
                    yield return i1;

                    if (n % 2 == 0)
                    {
                        yield return i2;
                        yield return i3;
                    }
                    else
                    {
                        yield return i3;
                        yield return i2;
                    }
                }
            }
        }

        #region Treeview Events
        private void ExtendedTreeView_ItemDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as ExtendedTreeViewItem;
            var mesh = item.Tag as Model3DGroup;
            if (mesh != null)
                renderer.LocateObject(mesh);
        }

        private bool isWorking = false;

        private void ExtendedTreeView_ItemChecked(object sender, RoutedEventArgs e)
        {
            if (isWorking) return;

            isWorking = true;
            SetState(e.OriginalSource as ExtendedTreeViewItem);
            isWorking = false;
        }

        private void SetState(ExtendedTreeViewItem item)
        {
            if (item.HasItems == false)
            {
                var parent = item.Parent as ExtendedTreeViewItem;
                var children = parent.Items.Cast<ExtendedTreeViewItem>();

                if (children.All(i => i.IsChecked == true))
                    parent.IsChecked = true;
                else if (children.All(i => i.IsChecked == false))
                    parent.IsChecked = false;
                else parent.IsChecked = null;

                var group = item.Tag as Model3DGroup;
                if (item.IsChecked == true)
                    modelGroup.Children.Add(group);
                else
                    modelGroup.Children.Remove(group);
            }
            else
            {
                foreach (ExtendedTreeViewItem i in item.Items)
                {
                    var group = i.Tag as Model3DGroup;
                    i.IsChecked = item.IsChecked;
                    if (i.IsChecked == true)
                        modelGroup.Children.Add(group);
                    else
                        modelGroup.Children.Remove(group);
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
        #endregion

        private void btnToggleDetails_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;
            if (toggle.IsChecked == true)
            {
                toggle.Tag = splitPanel.Panel1Size;
                splitPanel.Panel1Size = splitPanel.SplitterSize = new GridLength(0);
            }
            else
            {
                splitPanel.Panel1Size = (GridLength)toggle.Tag;
                splitPanel.SplitterSize = new GridLength(5);
            }
        }

        #region IDisposable
        public void Dispose()
        {
            TreeViewItems.Clear();
            renderer.ClearChildren();
            GC.Collect();
        }
        #endregion
    }
}
