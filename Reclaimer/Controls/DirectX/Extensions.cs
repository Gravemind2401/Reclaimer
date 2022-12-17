using System;
using System.Collections.Generic;
using System.Linq;

using Numerics = System.Numerics;
using Media3D = System.Windows.Media.Media3D;
using Helix = HelixToolkit.Wpf.SharpDX;
using HelixCore = HelixToolkit.SharpDX.Core;
using Adjutant.Spatial;

namespace Reclaimer.Controls.DirectX
{
    internal static class Extensions
    {
        public static float Clamp(float value, float min, float max) => Math.Min(Math.Max(min, value), max);

        public static double Clamp(double value, double min, double max) => Math.Min(Math.Max(min, value), max);

        public static Numerics.Vector3 ToNumericsVector3(this Media3D.Point3D point)
        {
            return new Numerics.Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Numerics.Vector3 ToNumericsVector3(this Media3D.Vector3D vector)
        {
            return new Numerics.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        public static SharpDX.Matrix ToMatrix3(this IRealBounds5D bounds)
        {
            return new SharpDX.Matrix
            {
                M11 = bounds.XBounds.Length,
                M22 = bounds.YBounds.Length,
                M33 = bounds.ZBounds.Length,
                M41 = bounds.XBounds.Min,
                M42 = bounds.YBounds.Min,
                M43 = bounds.ZBounds.Min,
                M44 = 1
            };
        }

        public static SharpDX.Matrix ToMatrix2(this IRealBounds5D bounds)
        {
            return new SharpDX.Matrix
            {
                M11 = bounds.UBounds.Length,
                M22 = bounds.VBounds.Length,
                M41 = bounds.UBounds.Min,
                M42 = bounds.VBounds.Min,
                M44 = 1
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
