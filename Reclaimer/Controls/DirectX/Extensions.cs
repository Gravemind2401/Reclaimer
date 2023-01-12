using Adjutant.Spatial;

using Helix = HelixToolkit.Wpf.SharpDX;
using HelixCore = HelixToolkit.SharpDX.Core;
using Media3D = System.Windows.Media.Media3D;
using Numerics = System.Numerics;

namespace Reclaimer.Controls.DirectX
{
    internal static class Extensions
    {
        public static Numerics.Vector3 ToNumericsVector3(this Media3D.Point3D point)
        {
            return new Numerics.Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Numerics.Vector3 ToNumericsVector3(this Media3D.Vector3D vector)
        {
            return new Numerics.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        public static SharpDX.Matrix ToMatrix3(this Numerics.Matrix4x4 matrix)
        {
            return new SharpDX.Matrix
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,
                M14 = matrix.M14,
                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,
                M24 = matrix.M24,
                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33,
                M34 = matrix.M34,
                M41 = matrix.M41,
                M42 = matrix.M42,
                M43 = matrix.M43,
                M44 = matrix.M44,
            };
        }

        public static SharpDX.BoundingBox GetTotalBounds(this IEnumerable<Helix.Element3D> elements, bool original = false)
        {
            return GetTotalBounds(elements.Select(e => e.SceneNode), original);
        }

        public static SharpDX.BoundingBox GetTotalBounds(this IEnumerable<HelixCore.Model.Scene.SceneNode> nodes, bool original = false)
        {
            var boundsList = new List<SharpDX.BoundingBox>();
            foreach (var node in nodes)
                CollectChildBounds(node, boundsList, original);

            return GetTotalBounds(boundsList);

            static void CollectChildBounds(HelixCore.Model.Scene.SceneNode node, List<SharpDX.BoundingBox> boundsList, bool original)
            {
                if (node.HasBound)
                    boundsList.Add(original ? node.OriginalBounds : node.BoundsWithTransform);
                else if (node.ItemsCount > 0)
                {
                    foreach (var child in node.Items.Where(i => i.Visible))
                        CollectChildBounds(child, boundsList, original);
                }
            }
        }

        private static SharpDX.BoundingBox GetTotalBounds(this IEnumerable<SharpDX.BoundingBox> bounds)
        {
            var boundsList = bounds as IList<SharpDX.BoundingBox> ?? bounds.ToList();

            if (!boundsList.Any())
                return default;

            var min = new SharpDX.Vector3(
                boundsList.Min(b => b.Minimum.X),
                boundsList.Min(b => b.Minimum.Y),
                boundsList.Min(b => b.Minimum.Z)
            );

            var max = new SharpDX.Vector3(
                boundsList.Max(b => b.Maximum.X),
                boundsList.Max(b => b.Maximum.Y),
                boundsList.Max(b => b.Maximum.Z)
            );

            return new SharpDX.BoundingBox(min, max);
        }
    }
}
