using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using Reclaimer.Geometry;
using System.Collections.ObjectModel;

namespace Reclaimer.Controls.DirectX
{
    public partial class ModelViewer
    {
        private sealed class MeshLoader
        {
            private readonly Dictionary<int, GroupModel3D> meshLookup = new();
            private readonly Dictionary<int, GroupModel3D> instanceLookup = new();

            public MeshLoader(Model model, TextureLoader textureLoader)
            {
                var indexes = model.Regions.SelectMany(r => r.Permutations)
                    .SelectMany(p => p.MeshIndices.Select(i => (i, p.IsInstanced)))
                    .Distinct();

                foreach (var (i, isInstance) in indexes)
                {
                    var mesh = model.Meshes.ElementAtOrDefault(i);
                    if (mesh == null || mesh.Segments.Count == 0)
                        continue;

                    var mGroup = new GroupModel3D();

                    var boundsMatrix = mesh.PositionBounds.CreateExpansionMatrix().ToMatrix3();
                    var texMatrix = mesh.TextureBounds.CreateExpansionMatrix().ToMatrix3();

                    foreach (var sub in mesh.Segments)
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

                            if (normals == null)
                                geom.Normals = geom.CalculateNormals(); //display in a faceted style just so you can see the edges properly in the viewer
                            else
                                geom.Normals = new Vector3Collection(normals);

                            geom.UpdateOctree();

                            var element = isInstance
                                ? new InstancingMeshGeometryModel3D()
                                : new MeshGeometryModel3D();

                            element.Geometry = geom;
                            element.Material = textureLoader[sub.Material?.Id];

                            mGroup.Children.Add(element);
                        }
                        catch { }
                    }

                    if (isInstance)
                        instanceLookup.Add(i, mGroup);
                    else
                        meshLookup.Add(i, mGroup);
                }
            }

            public MeshTag GetMesh(ModelPermutation permutation)
            {
                if (!permutation.MeshIndices.Any())
                    return null;

                if (permutation.IsInstanced)
                {
                    if (!instanceLookup.TryGetValue(permutation.MeshRange.Index, out var source))
                        return null;

                    var instance = new InstancedPermutation(source, permutation.GetFinalTransform().ToMatrix3());
                    return new MeshTag(permutation, source, instance);
                }

                var transform = permutation.GetFinalTransform().ToMediaTransform();
                transform.Freeze();

                var elementGroup = new GroupModel3D { Transform = transform };
                elementGroup.Children.AddRange(
                    permutation.MeshIndices
                        .Where(meshLookup.ContainsKey)
                        .Select(i => meshLookup[i])
                        .Where(m => m.Parent == null)
                );

                if (elementGroup.Children.Count == 0)
                    return null;

                return new MeshTag(permutation, elementGroup);
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
