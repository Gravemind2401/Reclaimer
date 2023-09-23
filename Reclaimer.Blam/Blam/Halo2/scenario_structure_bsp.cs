using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public class scenario_structure_bsp : ContentTagDefinition, IRenderGeometry
    {
        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        [Offset(68)]
        public RealBounds XBounds { get; set; }

        [Offset(76)]
        public RealBounds YBounds { get; set; }

        [Offset(84)]
        public RealBounds ZBounds { get; set; }

        [Offset(172)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(180)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(328)]
        public BlockCollection<BspSectionBlock> Sections { get; set; }

        [Offset(336)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);
            var geoParams = new Halo2GeometryArgs
            {
                Cache = Cache,
                Shaders = Shaders,
                Sections = Clusters.Select(c => new SectionArgs
                {
                    DataPointer = c.DataPointer,
                    DataSize = c.DataSize,
                    VertexCount = c.VertexCount,
                    FaceCount = c.FaceCount,
                    Resources = c.Resources,
                    BaseAddress = c.HeaderSize + 8
                }).Concat(Sections.Select(s => new SectionArgs
                {
                    DataPointer = s.DataPointer,
                    DataSize = s.DataSize,
                    VertexCount = s.VertexCount,
                    FaceCount = s.FaceCount,
                    Resources = s.Resources,
                    IsInstancing = true,
                    BaseAddress = s.HeaderSize + 8
                })).ToList()
            };

            var model = new GeometryModel(Item.FileName) { CoordinateSystem = CoordinateSystem.Default };
            model.Materials.AddRange(Halo2Common.GetMaterials(Shaders));
            model.Meshes.AddRange(Halo2Common.GetMeshes(geoParams));

            var clusterRegion = new GeometryRegion { Name = BlamConstants.SbspClustersGroupName };

            foreach (var section in Clusters.Where(s => s.VertexCount > 0))
            {
                var sectionIndex = Clusters.IndexOf(section);

                var perm = new GeometryPermutation
                {
                    SourceIndex = sectionIndex,
                    Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                    MeshIndex = sectionIndex,
                    MeshCount = 1
                };

                clusterRegion.Permutations.Add(perm);
            }

            if (clusterRegion.Permutations.Count > 0)
                model.Regions.Add(clusterRegion);

            foreach (var section in Sections.Where(s => s.VertexCount > 0))
            {
                var sectionIndex = Sections.IndexOf(section);
                var sectionRegion = new GeometryRegion { Name = Utils.CurrentCulture($"Instances {sectionIndex:D3}") };

                var perms = GeometryInstances
                    .Where(i => i.SectionIndex == sectionIndex)
                    .Select(i => new GeometryPermutation
                    {
                        SourceIndex = GeometryInstances.IndexOf(i),
                        Name = i.Name,
                        Transform = i.Transform,
                        TransformScale = i.TransformScale,
                        MeshIndex = Clusters.Count + sectionIndex,
                        MeshCount = 1
                    }).ToList();

                if (perms.Count > 0)
                {
                    sectionRegion.Permutations.AddRange(perms);
                    model.Regions.Add(sectionRegion);
                }
            }

            return model;
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo2Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo2Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [FixedSize(176)]
    public class ClusterBlock
    {
        [Offset(0)]
        public ushort VertexCount { get; set; }

        [Offset(2)]
        public ushort FaceCount { get; set; }

        [Offset(24)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(40)]
        public DataPointer DataPointer { get; set; }

        [Offset(44)]
        public int DataSize { get; set; }

        [Offset(48)]
        public int HeaderSize { get; set; }

        [Offset(56)]
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(200)]
    public class BspSectionBlock : ClusterBlock
    {

    }

    [FixedSize(88)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public float TransformScale { get; set; }

        [Offset(4)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }

        [Offset(80)]
        public StringId Name { get; set; }
    }
}
