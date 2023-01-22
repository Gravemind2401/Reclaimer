using Reclaimer.Geometry;
using System.Numerics;

namespace Adjutant.Geometry
{
    public static class Extensions2
    {
        public static Scene ConvertToScene(this IGeometryModel source)
        {
            var model = new Model { Name = source.Name };

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
                    reg.Permutations.Add(new ModelPermutation
                    {
                        Name = p.Name,
                        MeshRange = (p.MeshIndex, p.MeshCount),
                        Transform = p.Transform,
                        UniformScale = p.TransformScale,
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

            return Scene.WrapSingleModel(model);
        }
    }
}
