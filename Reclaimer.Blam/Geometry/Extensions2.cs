using Reclaimer.Geometry;
using System.Numerics;

namespace Adjutant.Geometry
{
    public static class Extensions2
    {
        public static Model ConvertToScene(this IGeometryModel source)
        {
            var scene = new Scene();
            var group = new SceneGroup();

            var obj = new SceneObject();
            var model = obj.Model = new Model { Name = source.Name };

            foreach (var b in source.Nodes)
            {
                model.Bones.Add(new Bone
                {
                    Name = b.Name,
                    ParentIndex = b.ParentIndex,
                    FirstChildIndex = b.FirstChildIndex,
                    NextSiblingIndex = b.NextSiblingIndex,
                    Position = new Vector3(b.Position.X, b.Position.Y, b.Position.Z),
                    Rotation = new Quaternion(b.Rotation.X, b.Rotation.Y, b.Rotation.Z, b.Rotation.W),
                    Transform = b.OffsetTransform
                });
            }

            foreach (var g in source.MarkerGroups)
            {
                var marker = new Marker { Name = g.Name };
                model.Markers.Add(marker);

                foreach (var m in g.Markers)
                {
                    marker.Instances.Add(new MarkerInstance
                    {
                        RegionIndex = m.RegionIndex,
                        PermutationIndex = m.PermutationIndex,
                        BoneIndex = m.NodeIndex,
                        Position = new Vector3(m.Position.X, m.Position.Y, m.Position.Z),
                        Rotation = new Quaternion(m.Rotation.X, m.Rotation.Y, m.Rotation.Z, m.Rotation.W)
                    });
                }
            }

            var materials = source.Materials.Select((m, index) =>
            {
                if (m == null)
                    return null;

                var mat = new Material { Id = index, Name = m.Name, Flags = (int)m.Flags };

                foreach (var s in m.Submaterials)
                {
                    mat.TextureMappings.Add(new TextureMapping
                    {
                        Usage = (int)s.Usage,
                        Tiling = new Vector2(s.Tiling.X, s.Tiling.Y),
                        Texture = new Texture
                        {
                            Id = s.Bitmap.Id,
                            Name = s.Bitmap.Name,
                            GetDds = () => s.Bitmap.ToDds(0)
                        }
                    });
                }

                foreach (var t in m.TintColours)
                {
                    mat.Tints.Add(new MaterialTint
                    {
                        Usage = (int)t.Usage,
                        Color = System.Drawing.Color.FromArgb(t.A, t.R, t.G, t.B)
                    });
                }

                return mat;
            }).ToList();

            foreach (var r in source.Regions)
            {
                var reg = new ModelRegion { Name = r.Name };
                model.Regions.Add(reg);

                foreach (var p in r.Permutations)
                {
                    //not sure why but p.Transform doesnt work as-is for instance transforms in Helix
                    //seems that decomposing and recomposing results in a different matrix which does work
                    Matrix4x4.Decompose(p.Transform, out var scale, out var rotation, out var translation);

                    var t1 = Matrix4x4.CreateScale(p.TransformScale) * p.Transform;
                    var t2 = Matrix4x4.CreateScale(p.TransformScale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
                    var t3 = Matrix4x4.CreateScale(p.TransformScale) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);

                    reg.Permutations.Add(new ModelPermutation
                    {
                        Name = p.Name,
                        MeshRange = (p.MeshIndex, p.MeshCount),
                        Transform = t3,
                        IsInstanced = source.Meshes[p.MeshIndex].IsInstancing
                    });
                }
            }

            foreach (var m in source.Meshes)
            {
                var mesh = new Mesh
                {
                    IndexBuffer = m.IndexBuffer,
                    VertexBuffer = m.VertexBuffer,
                    BoneIndex = m.NodeIndex
                };
                model.Meshes.Add(mesh);

                if (m.BoundsIndex.HasValue)
                {
                    var bounds = source.Bounds[m.BoundsIndex.Value];
                    mesh.PositionBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                    mesh.TextureBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
                }

                foreach (var s in m.Submeshes)
                {
                    mesh.Segments.Add(new MeshSegment
                    {
                        Material = materials.ElementAtOrDefault(s.MaterialIndex),
                        IndexStart = s.IndexStart,
                        IndexLength = s.IndexLength
                    });
                }
            }

            scene.ObjectGroups.Add(group);
            group.ChildObjects.Add(obj);

            return model;
        }
    }
}
