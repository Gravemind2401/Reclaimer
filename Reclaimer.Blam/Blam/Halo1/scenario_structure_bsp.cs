using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Globalization;
using System.IO;

namespace Reclaimer.Blam.Halo1
{
    public class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        [Offset(8)]
        public int MccVertexDataAddress { get; set; }

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

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            using var reader = Cache.CreateReader(Cache.DefaultAddressTranslator);

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);

            var shaderRefs = Lightmaps.SelectMany(m => m.Materials)
                .Where(m => m.ShaderReference.TagId >= 0)
                .GroupBy(m => m.ShaderReference.TagId)
                .Select(g => g.First().ShaderReference)
                .ToList();

            var shaderIds = shaderRefs.Select(r => r.TagId).ToList();
            var materials = Halo1Common.GetMaterials(shaderRefs, reader).ToList();

            reader.Seek(SurfacePointer.Address, SeekOrigin.Begin);
            var indices = reader.ReadArray<ushort>(SurfaceCount * 3);

            //bsp surfaces seem to be wound backwards in h1?
            for (var i = 0; i < SurfaceCount; i++)
                Array.Reverse(indices, i * 3, 3);

            var region = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };

            const int vertexSize = 56;

            var sectionIndex = 0;
            foreach (var section in Lightmaps)
            {
                if (section.Materials.Count == 0)
                    continue;

                var vertexCount = section.Materials.Sum(s => s.VertexBuffer1Count);
                var vertexData = new byte[vertexSize * vertexCount];

                var vertexBuffer = new VertexBuffer();
                vertexBuffer.PositionChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 0));
                vertexBuffer.NormalChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 12));
                vertexBuffer.BinormalChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 24));
                vertexBuffer.TangentChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 36));
                vertexBuffer.TextureCoordinateChannels.Add(new VectorBuffer<RealVector2>(vertexData, vertexCount, vertexSize, 48));

                var mesh = new Mesh();
                var localIndices = new List<int>();

                var permutation = new ModelPermutation
                {
                    Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                    MeshRange = (sectionIndex, 1)
                };

                var vertexTally = 0;
                foreach (var submesh in section.Materials)
                {
                    var vertexBufferAddress = Cache.Metadata.IsMcc
                        ? MccVertexDataAddress + submesh.VertexBuffer1Offset
                        : submesh.VertexBuffer1Pointer.Address;

                    reader.Seek(vertexBufferAddress, SeekOrigin.Begin);
                    reader.ReadBytes(vertexSize * submesh.VertexBuffer1Count).CopyTo(vertexData, vertexTally * vertexSize);

                    mesh.Segments.Add(new MeshSegment
                    {
                        Material = materials.ElementAtOrDefault(shaderIds.IndexOf(submesh.ShaderReference.TagId)),
                        IndexStart = localIndices.Count,
                        IndexLength = submesh.SurfaceCount * 3
                    });

                    localIndices.AddRange(
                        indices.Skip(submesh.SurfaceIndex * 3)
                               .Take(submesh.SurfaceCount * 3)
                               .Select(i => i + vertexTally)
                    );

                    vertexTally += submesh.VertexBuffer1Count;
                }

                region.Permutations.Add(permutation);

                mesh.IndexBuffer = IndexBuffer.FromCollection(localIndices, IndexFormat.TriangleList);
                mesh.VertexBuffer = vertexBuffer;
                model.Meshes.Add(mesh);

                sectionIndex++;
            }

            model.Regions.Add(region);

            return model;
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
    [DebuggerDisplay($"{{{nameof(ShaderReference)},nq}}")]
    public class MaterialBlock
    {
        [Offset(0)]
        public TagReference ShaderReference { get; set; }

        [Offset(20)]
        public int SurfaceIndex { get; set; }

        [Offset(24)]
        public int SurfaceCount { get; set; }

        [Offset(176)]
        public short VertexBuffer1Type { get; set; }

        [Offset(180)]
        public int VertexBuffer1Count { get; set; }

        [Offset(184)]
        public int VertexBuffer1Offset { get; set; }

        [Offset(196)]
        public short VertexBuffer2Type { get; set; }

        [Offset(200)]
        public int VertexBuffer2Count { get; set; }

        [Offset(204)]
        public int VertexBuffer2Offset { get; set; }

        [Offset(212)]
        public int VertexIndexPointer { get; set; }

        [Offset(216)]
        public DataPointer VertexBuffer1Pointer { get; set; }

        [Offset(236)]
        public DataPointer VertexBuffer2Pointer { get; set; }
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
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class BspMarkerBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public RealVector4 Rotation { get; set; }

        [Offset(48)]
        public RealVector3 Position { get; set; }
    }
}
