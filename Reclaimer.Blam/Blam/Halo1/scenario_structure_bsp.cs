using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo1
{
    public class scenario_structure_bsp : IRenderGeometry
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public scenario_structure_bsp(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
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

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                var model = new GeometryModel(item.FileName()) { CoordinateSystem = CoordinateSystem.Default };

                var shaderRefs = Lightmaps.SelectMany(m => m.Materials)
                    .Where(m => m.ShaderReference.TagId >= 0)
                    .GroupBy(m => m.ShaderReference.TagId)
                    .Select(g => g.First().ShaderReference)
                    .ToList();

                var shaderIds = shaderRefs.Select(r => r.TagId).ToList();

                model.Materials.AddRange(Halo1Common.GetMaterials(shaderRefs, reader));

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
                        SourceIndex = Lightmaps.IndexOf(section),
                        Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
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
                        IndexFormat = IndexFormat.TriangleList,
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

        public IEnumerable<IBitmap> GetAllBitmaps() => GetBitmaps(Enumerable.Range(0, Lightmaps?.Count ?? 0));

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            var selection = shaderIndexes?.Distinct().Where(i => i >= 0 && i < Lightmaps?.Count).Select(i => Lightmaps[i]);
            if (selection?.Any() != true)
                yield break;

            var complete = new List<int>();
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                foreach (var mat in selection.SelectMany(lm => lm.Materials))
                {
                    var bitmTag = Halo1Common.GetShaderDiffuse(mat.ShaderReference, reader);
                    if (bitmTag == null || complete.Contains(bitmTag.Id))
                        continue;

                    complete.Add(bitmTag.Id);
                    yield return bitmTag.ReadMetadata<bitmap>();
                }
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
        [Offset(0)]
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
