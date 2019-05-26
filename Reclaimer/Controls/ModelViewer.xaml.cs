using Adjutant.Geometry;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Studio.Controls;
using System.Collections.ObjectModel;
using System.IO;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : UserControl, ITabContent, IDisposable
    {
        private static readonly DiffuseMaterial ErrorMaterial;

        public ObservableCollection<ExtendedTreeViewItem> TreeViewItems { get; }

        static ModelViewer()
        {
            (ErrorMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gold))).Freeze();
        }

        public ModelViewer()
        {
            InitializeComponent();
            TreeViewItems = new ObservableCollection<ExtendedTreeViewItem>();
            DataContext = this;
        }

        public void LoadGeometry(Adjutant.Utilities.IRenderGeometry geom, string fileName)
        {
            TabToolTip = fileName;
            TabHeader = System.IO.Path.GetFileName(fileName);

            var model = geom.ReadGeometry(0);
            var group = new Model3DGroup();
            var vis = new ModelVisual3D();

            var materials = new List<Material>();
            foreach (var mat in model.Materials)
            {
                try
                {
                    var dds = mat.Diffuse.ToDds(0);

                    materials.Add(new DiffuseMaterial
                    {
                        Brush = new ImageBrush(dds.ToBitmapSource(false))
                        {
                            ViewportUnits = BrushMappingMode.Absolute,
                            TileMode = TileMode.Tile,
                            Viewport = new Rect(0, 0, 1f / Math.Abs(mat.Tiling.X), 1f / Math.Abs(mat.Tiling.Y))
                        }
                    });
                }
                catch
                {
                    materials.Add(ErrorMaterial);
                }
            }

            TreeViewItems.Clear();
            foreach (var region in model.Regions)
            {
                var regNode = new ExtendedTreeViewItem { Header = region.Name };

                foreach (var perm in region.Permutations)
                {
                    var permNode = new ExtendedTreeViewItem { Header = perm.Name };
                    regNode.Items.Add(permNode);

                    var mesh = model.Meshes[perm.MeshIndex];

                    var permGroup = new Model3DGroup();
                    var tGroup = new Transform3DGroup();

                    var texMatrix = Matrix.Identity;
                    if (perm.BoundsIndex >= 0)
                    {
                        var bounds = model.Bounds[perm.BoundsIndex];
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

                    (permGroup.Transform = tGroup).Freeze();

                    foreach (var sub in perm.Submeshes)
                    {
                        try
                        {
                            var mg = new MeshGeometry3D();

                            var indices = mesh.Indicies.Skip(sub.IndexStart).Take(sub.IndexLength).ToArray();
                            if (mesh.IndexFormat == IndexFormat.Stripped) indices = Unstrip(indices).ToArray();

                            var vertStart = indices.Min();
                            var vertLength = indices.Max() - vertStart + 1;

                            var verts = mesh.Vertices.Skip(vertStart).Take(vertLength);
                            var positions = verts.Select(v => new Point3D(v.Position[0].X, v.Position[0].Y, v.Position[0].Z));

                            var texcoords = verts.Select(v => new Point(v.TexCoords[0].X, v.TexCoords[0].Y)).ToArray();
                            if (!texMatrix.IsIdentity) texMatrix.Transform(texcoords);

                            (mg.Positions = new Point3DCollection(positions)).Freeze();
                            (mg.TextureCoordinates = new PointCollection(texcoords)).Freeze();
                            (mg.TriangleIndices = new Int32Collection(indices.Select(i => i - vertStart))).Freeze();

                            if (mesh.Vertices[0].Normal.Count > 0)
                            {
                                var normals = verts.Select(v => new Vector3D(v.Normal[0].X, v.Normal[0].Y, v.Normal[0].Z));
                                (mg.Normals = new Vector3DCollection(normals)).Freeze();
                            }

                            var mat = sub.MaterialIndex >= 0 ? materials[sub.MaterialIndex] : ErrorMaterial;
                            var subGroup = new GeometryModel3D(mg, mat) { BackMaterial = mat };
                            subGroup.Freeze();
                            permGroup.Children.Add(subGroup);
                        }
                        catch { }
                    }

                    permGroup.Freeze();
                    group.Children.Add(permGroup);
                }

                TreeViewItems.Add(regNode);
            }

            vis.Content = group;

            renderer.AddChild(vis);

            renderer.ScaleToContent(new[] { group });
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

        #region ITabContent
        public object TabHeader { get; private set; }

        public object TabToolTip { get; private set; }

        public object TabIcon => null;

        TabItemUsage ITabContent.TabUsage => TabItemUsage.Document;
        #endregion

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
