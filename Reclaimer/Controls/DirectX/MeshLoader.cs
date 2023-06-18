using Adjutant.Geometry;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using System.Collections.ObjectModel;

using Media3D = System.Windows.Media.Media3D;

namespace Reclaimer.Controls.DirectX
{
    public partial class ModelViewer
    {
        private sealed class MeshLoader
        {
            private readonly Dictionary<int, List<InstancedPermutation>> instanceIds = new();
            private readonly Dictionary<int, GroupModel3D> meshLookup = new();

            public MeshLoader(IGeometryModel model, TextureLoader textureLoader)
            {
                var indexes = model.Regions.SelectMany(r => r.Permutations)
                    .SelectMany(p => Enumerable.Range(p.MeshIndex, p.MeshCount)).Distinct();

                foreach (var i in indexes)
                {
                    var mesh = model.Meshes.ElementAtOrDefault(i);
                    if (mesh == null || mesh.Submeshes.Count == 0)
                        continue;

                    if (mesh.IsInstancing)
                        instanceIds.Add(i, new List<InstancedPermutation>());

                    var mGroup = new GroupModel3D();

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

                            var geom = new MeshGeometry3D
                            {
                                Indices = new IntCollection(indices.Select(j => j - vertStart)),
                                Positions = new Vector3Collection(positions)
                            };

                            if (texcoords != null)
                                geom.TextureCoordinates = new Vector2Collection(texcoords);

                            if (normals != null)
                                geom.Normals = new Vector3Collection(normals);
                            
                            geom.UpdateOctree();

                            var element = mesh.IsInstancing
                                ? new InstancingMeshGeometryModel3D()
                                : new MeshGeometryModel3D();

                            element.Geometry = geom;
                            element.Material = textureLoader[sub.MaterialIndex];

                            mGroup.Children.Add(element);
                        }
                        catch { }
                    }

                    meshLookup.Add(i, mGroup);
                }
            }

            public MeshTag GetMesh(IGeometryPermutation permutation)
            {
                if (instanceIds.TryGetValue(permutation.MeshIndex, out var instances))
                {
                    var source = meshLookup[permutation.MeshIndex];
                    var matrix = GetTransform(permutation).ToMatrix(); // /*SharpDX.Matrix.Scaling(permutation.TransformScale) **/ permutation.Transform.ToMatrix3();
                    var instance = new InstancedPermutation(source, matrix);
                    instances.Add(instance);
                    return new MeshTag(permutation, source, instance);
                }

                var transformGroup = GetTransform(permutation);

                GroupElement3D elementGroup;
                if (transformGroup.Children.Count == 0 && permutation.MeshCount == 1)
                    elementGroup = meshLookup.GetValueOrDefault(permutation.MeshIndex);
                else
                {
                    elementGroup = new GroupModel3D();
                    var parts = Enumerable.Range(permutation.MeshIndex, permutation.MeshCount)
                        .Where(meshLookup.ContainsKey)
                        .Select(i => meshLookup[i]);

                    elementGroup.Children.AddRange(parts);

                    if (transformGroup.Children.Count > 0)
                        elementGroup.Transform = transformGroup;
                }

                if (elementGroup == null || elementGroup.Children.Count == 0)
                    return null;

                return new MeshTag(permutation, elementGroup);
            }

            private static Media3D.Transform3DGroup GetTransform(IGeometryPermutation permutation)
            {
                var transformGroup = new Media3D.Transform3DGroup();

                if (permutation.TransformScale != 1)
                {
                    var tform = new Media3D.ScaleTransform3D(permutation.TransformScale, permutation.TransformScale, permutation.TransformScale);

                    tform.Freeze();
                    transformGroup.Children.Add(tform);
                }

                if (!permutation.Transform.IsIdentity)
                {
                    var transform = new Media3D.MatrixTransform3D(new Media3D.Matrix3D
                    {
                        M11 = permutation.Transform.M11,
                        M12 = permutation.Transform.M12,
                        M13 = permutation.Transform.M13,

                        M21 = permutation.Transform.M21,
                        M22 = permutation.Transform.M22,
                        M23 = permutation.Transform.M23,

                        M31 = permutation.Transform.M31,
                        M32 = permutation.Transform.M32,
                        M33 = permutation.Transform.M33,

                        OffsetX = permutation.Transform.M41,
                        OffsetY = permutation.Transform.M42,
                        OffsetZ = permutation.Transform.M43
                    });

                    transform.Freeze();
                    transformGroup.Children.Add(transform);
                }

                transformGroup.Freeze();
                return transformGroup;
            }

            public sealed class InstancedPermutation
            {
                public Guid Id { get; }
                public GroupModel3D InstanceGroup { get; }
                public SharpDX.Matrix Transform { get; }

                public InstancedPermutation(GroupModel3D instanceGroup, SharpDX.Matrix transform)
                {
                    Id = Guid.NewGuid();
                    InstanceGroup = instanceGroup;
                    Transform = transform;

                    foreach (var geometry in InstanceGroup.Children.OfType<InstancingMeshGeometryModel3D>())
                    {
                        (geometry.InstanceIdentifiers ??= new List<Guid>()).Add(Id);
                        (geometry.Instances ??= new List<SharpDX.Matrix>()).Add(Transform);

                        CycleInstances(geometry);
                    }

                    instanceGroup.InvalidateRender();
                }

                //GeometryModel3D doesnt observe the Instances property and InvalidateRender() doesnt refresh it either...
                //the only time it ever updates is when a new value is assigned
                private static void CycleInstances(GeometryModel3D geometry)
                {
                    var temp = geometry.Instances;
                    geometry.Instances = null;
                    geometry.Instances = temp;
                }

                public void Toggle(bool visible)
                {
                    foreach (var geometry in InstanceGroup.Children.OfType<InstancingMeshGeometryModel3D>())
                    {
                        var index = geometry.InstanceIdentifiers.IndexOf(Id);
                        geometry.Instances[index] = visible ? Transform : SharpDX.Matrix.Scaling(0);
                        CycleInstances(geometry);
                    }

                    InstanceGroup.InvalidateRender();
                }
            }
        }
    }
}
