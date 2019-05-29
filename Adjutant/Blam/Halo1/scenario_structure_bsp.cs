using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class scenario_structure_bsp : IRenderGeometry
    {
        private readonly CacheFile cache;

        public scenario_structure_bsp(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(224)]
        public RealBounds XBounds { get; set; }

        [Offset(232)]
        public RealBounds YBounds { get; set; }

        [Offset(240)]
        public RealBounds ZBounds { get; set; }

        [Offset(272)]
        public int SurfaceCount { get; set; }

        [Offset(276)]
        public Pointer SurfacePointer { get; set; }

        [Offset(284)]
        public BlockCollection<LightmapBlock> Lightmaps { get; set; }

        [Offset(600)]
        public BlockCollection<BspMarkerBlock> Markers { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            using (var reader = cache.CreateReader(cache.AddressTranslator))
            {
                var model = new GeometryModel { CoordinateSystem = CoordinateSystem.HaloCE };

                var group = new GeometryMarkerGroup();
                group.Markers.AddRange(Markers);
                model.MarkerGroups.Add(group);

                #region Add Shaders

                var shaderIds = Lightmaps.SelectMany(m => m.Materials)
                    .Where(m => m.ShaderReference.TagId >= 0)
                    .Select(m => m.ShaderReference.TagId)
                    .Distinct().ToList();

                var shaderTags = shaderIds.Select(i => cache.TagIndex[i]);
                foreach (var shaderTag in shaderTags)
                {
                    var bitmTag = shaderTag.GetShaderDiffuse(reader);

                    if (bitmTag == null)
                    {
                        model.Materials.Add(null);
                        continue;
                    }

                    var mat = new GeometryMaterial
                    {
                        Name = bitmTag.FileName,
                        Diffuse = bitmTag.ReadMetadata<bitmap>(),
                        Tiling = new RealVector2D(1, 1)
                    };

                    model.Materials.Add(mat);
                } 

                #endregion

                reader.Seek(SurfacePointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<ushort>(SurfaceCount * 3).ToArray();

                var gRegion = new GeometryRegion { Name = "Clusters" };

                int sectionIndex = 0;
                foreach (var section in Lightmaps)
                {
                    if (section.Materials.Count == 0)
                        continue;

                    var localIndices = new List<int>();
                    var vertices = new List<WorldVertex>();
                    var submeshes = new List<IGeometrySubmesh>();

                    var gPermutation = new GeometryPermutation
                    {
                        Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                        NodeIndex = byte.MaxValue,
                        Transform = Matrix4x4.Identity,
                        TransformScale = 1,
                        BoundsIndex = -1,
                        MeshIndex = sectionIndex,
                        MeshCount = 1
                    };

                    foreach (var submesh in section.Materials)
                    {
                        reader.Seek(submesh.VertexPointer.Address, SeekOrigin.Begin);

                        submeshes.Add(new GeometrySubmesh
                        {
                            MaterialIndex = (short)shaderIds.IndexOf(submesh.ShaderReference.TagId),
                            IndexStart = localIndices.Count,
                            IndexLength = submesh.SurfaceCount * 3
                        });

                        localIndices.AddRange(
                            indices.Skip(submesh.SurfaceIndex * 3)
                                   .Take(submesh.SurfaceCount * 3)
                                   .Select(i => i + vertices.Count)
                        );

                        var vertsTemp = reader.ReadEnumerable<WorldVertex>(submesh.VertexCount).ToList();
                        vertices.AddRange(vertsTemp);
                    }

                    gRegion.Permutations.Add(gPermutation);

                    model.Meshes.Add(new GeometryMesh
                    {
                        IndexFormat = IndexFormat.Triangles,
                        VertexWeights = VertexWeights.None,
                        Indicies = localIndices.ToArray(),
                        Vertices = vertices.ToArray(),
                        Submeshes = submeshes
                    });

                    sectionIndex++;
                }

                model.Regions.Add(gRegion);

                return model;
            }
        }

        #endregion
    }

    [FixedSize(32)]
    public class LightmapBlock
    {
        [Offset(20)]
        public BlockCollection<MaterialBlock> Materials { get; set; }
    }

    [FixedSize(256)]
    public class MaterialBlock
    {
        [Offset(12)]
        public TagReference ShaderReference { get; set; }

        [Offset(20)]
        public int SurfaceIndex { get; set; }

        [Offset(24)]
        public int SurfaceCount { get; set; }

        [Offset(180)]
        public int VertexCount { get; set; }

        [Offset(228)]
        public Pointer VertexPointer { get; set; }

        public override string ToString() => ShaderReference.ToString();
    }

    [FixedSize(104)]
    public class ClusterBlock
    {
        [Offset(52)]
        public BlockCollection<SubclusterBlock> Subclusters { get; set; }

        [Offset(68)]
        public BlockCollection<int> SurfaceIndices { get; set; }
    }

    [FixedSize(36)]
    public class SubclusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(24)]
        public BlockCollection<int> SurfaceIndices { get; set; }
    }

    [FixedSize(60)]
    public class BspMarkerBlock : IGeometryMarker
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public RealVector4D Rotation { get; set; }

        [Offset(48)]
        public RealVector3D Position { get; set; }

        #region IGeometryMarker

        byte IGeometryMarker.RegionIndex => byte.MaxValue;

        byte IGeometryMarker.PermutationIndex => byte.MaxValue;

        byte IGeometryMarker.NodeIndex => byte.MaxValue;

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }
}
