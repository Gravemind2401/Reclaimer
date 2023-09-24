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

        public static Media3D.Vector3D ToMediaVector3(this Numerics.Vector3 vector)
        {
            return new Media3D.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static Media3D.Transform3D ToMediaTransform(this Numerics.Matrix4x4 matrix)
        {
            return new Media3D.MatrixTransform3D(new Media3D.Matrix3D
            {
                M11 = matrix.M11,
                M12 = matrix.M12,
                M13 = matrix.M13,

                M21 = matrix.M21,
                M22 = matrix.M22,
                M23 = matrix.M23,

                M31 = matrix.M31,
                M32 = matrix.M32,
                M33 = matrix.M33,

                OffsetX = matrix.M41,
                OffsetY = matrix.M42,
                OffsetZ = matrix.M43
            });
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

        public static SharpDX.BoundingBox GetTotalBounds(this Helix.Element3D element, bool original = false) => GetTotalBounds(Enumerable.Repeat(element, 1), original);

        public static SharpDX.BoundingBox GetTotalBounds(this IEnumerable<Helix.Element3D> elements, bool original = false)
        {
            return GetTotalBounds(elements.Select(e => e.SceneNode), original);
        }

        public static SharpDX.BoundingBox GetTotalBounds(this IEnumerable<HelixCore.Model.Scene.SceneNode> nodes, bool original = false)
        {
            var enumerator = nodes.SelectMany(EnumerateChildBounds).GetEnumerator();
            if (!enumerator.MoveNext())
                return default;

            var current = enumerator.Current;
            var result = current;

            while (enumerator.MoveNext())
            {
                current = enumerator.Current;
                SharpDX.BoundingBox.Merge(ref result, ref current, out result);
            }

            return result;

            IEnumerable<SharpDX.BoundingBox> EnumerateChildBounds(HelixCore.Model.Scene.SceneNode node)
            {
                if (node.HasBound)
                    yield return original ? node.OriginalBounds : node.BoundsWithTransform;
                else if (node.ItemsCount > 0)
                {
                    foreach (var box in node.Items.Where(i => i.Visible).SelectMany(EnumerateChildBounds))
                        yield return box;
                }
            }
        }
    }
}
