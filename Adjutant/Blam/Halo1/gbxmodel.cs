using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class gbxmodel : IRenderGeometry
    {
        private readonly CacheFile cache;
        private readonly IndexItem item;

        public gbxmodel(CacheFile cache, IndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(0)]
        public ModelFlags Flags { get; set; }

        [Offset(48)]
        public float UScale { get; set; }

        [Offset(52)]
        public float VScale { get; set; }

        [Offset(172)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(184)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(196)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(208)]
        public BlockCollection<ModelSectionBlock> Sections { get; set; }

        [Offset(220)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => Regions.SelectMany(r => r.Permutations).Max(p => p.LodCount);

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            using (var reader = cache.CreateReader(cache.AddressTranslator))
            {
                var model = new GeometryModel(item.FileName) { CoordinateSystem = CoordinateSystem.HaloCE };

                model.Nodes.AddRange(Nodes);
                model.MarkerGroups.AddRange(MarkerGroups);

                var shaderRefs = Shaders.Select(s => s.ShaderReference);
                model.Materials.AddRange(Halo1Common.GetMaterials(shaderRefs, reader));

                foreach (var region in Regions)
                {
                    var gRegion = new GeometryRegion { Name = region.Name };
                    gRegion.Permutations.AddRange(region.Permutations.Select(p =>
                        new GeometryPermutation
                        {
                            Name = p.Name,
                            MeshIndex = p.LodIndex(lod),
                            MeshCount = 1
                        }));

                    model.Regions.Add(gRegion);
                }

                if (cache.CacheType == CacheType.Halo1Xbox)
                    model.Meshes.AddRange(ReadXboxMeshes(reader));
                else model.Meshes.AddRange(ReadPCMeshes(reader));

                return model;
            }
        }

        private IEnumerable<GeometryMesh> ReadXboxMeshes(DependencyReader reader)
        {
            var magic = cache.TagIndex.Magic - (cache.Header.IndexAddress + cache.TagIndex.HeaderSize);

            foreach (var section in Sections)
            {
                var indices = new List<int>();
                var vertices = new List<CompressedVertex>();
                var submeshes = new List<IGeometrySubmesh>();

                foreach (var submesh in section.Submeshes)
                {
                    if (submesh.IndexCount == 0 || submesh.VertexCount == 0)
                        continue;

                    try
                    {
                        var gSubmesh = new GeometrySubmesh
                        {
                            MaterialIndex = submesh.ShaderIndex,
                            IndexStart = indices.Count,
                            IndexLength = submesh.IndexCount + 2
                        };

                        submeshes.Add(gSubmesh);

                        reader.Seek(submesh.IndexOffset - magic, SeekOrigin.Begin);
                        reader.ReadInt32();
                        reader.Seek(reader.ReadInt32() - magic, SeekOrigin.Begin);

                        var indicesTemp = reader.ReadEnumerable<ushort>(gSubmesh.IndexLength).ToList();
                        indices.AddRange(indicesTemp.Select(i => i + vertices.Count));

                        reader.Seek(submesh.VertexOffset - magic, SeekOrigin.Begin);
                        reader.ReadInt32();
                        reader.Seek(reader.ReadInt32() - magic, SeekOrigin.Begin);

                        var vertsTemp = new List<CompressedVertex>();
                        for (int i = 0; i < submesh.VertexCount; i++)
                            vertsTemp.Add(new CompressedVertex(reader));

                        if (UScale != 1 || VScale != 1)
                        {
                            vertsTemp.ForEach((v) =>
                            {
                                var vec = v.TexCoords;
                                vec.X *= UScale;
                                vec.Y *= VScale;
                                v.TexCoords = vec;
                            });
                        }

                        vertices.AddRange(vertsTemp);
                    }
                    catch { }
                }

                yield return new GeometryMesh
                {
                    IndexFormat = IndexFormat.Stripped,
                    VertexWeights = VertexWeights.Skinned,
                    Indicies = indices.ToArray(),
                    Vertices = vertices.ToArray(),
                    Submeshes = submeshes
                };
            }
        }

        private IEnumerable<GeometryMesh> ReadPCMeshes(DependencyReader reader)
        {
            foreach (var section in Sections)
            {
                var indices = new List<int>();
                var vertices = new List<UncompressedVertex>();
                var submeshes = new List<IGeometrySubmesh>();

                foreach (var submesh in section.Submeshes)
                {
                    var gSubmesh = new GeometrySubmesh
                    {
                        MaterialIndex = submesh.ShaderIndex,
                        IndexStart = indices.Count,
                        IndexLength = submesh.IndexCount + 2
                    };

                    submeshes.Add(gSubmesh);

                    reader.Seek(cache.TagIndex.VertexDataOffset + cache.TagIndex.IndexDataOffset + submesh.IndexOffset, SeekOrigin.Begin);
                    indices.AddRange(reader.ReadEnumerable<ushort>(gSubmesh.IndexLength).Select(i => i + vertices.Count));

                    reader.Seek(cache.TagIndex.VertexDataOffset + submesh.VertexOffset, SeekOrigin.Begin);
                    var vertsTemp = reader.ReadEnumerable<UncompressedVertex>(submesh.VertexCount).ToList();

                    if (UScale != 1 || VScale != 1)
                    {
                        vertsTemp.ForEach((v) =>
                        {
                            var vec = v.TexCoords;
                            vec.X *= UScale;
                            vec.Y *= VScale;
                            v.TexCoords = vec;
                        });
                    }

                    //if (Flags.HasFlag(ModelFlags.UseLocalNodes))
                    //{
                    //    var address = section.Submeshes.Pointer.Address;
                    //    address += section.Submeshes.IndexOf(submesh) * 132;
                    //    reader.Seek(address + 107, SeekOrigin.Begin);
                    //    var nodeCount = reader.ReadByte();
                    //    var nodes = reader.ReadEnumerable<byte>(nodeCount).ToArray();

                    //    vertsTemp.ForEach((v) =>
                    //    {
                    //        v.NodeIndex1 = nodes[v.NodeIndex1];
                    //        v.NodeIndex2 = nodes[v.NodeIndex2];
                    //    });
                    //}

                    vertices.AddRange(vertsTemp);
                }

                yield return new GeometryMesh
                {
                    IndexFormat = IndexFormat.Stripped,
                    VertexWeights = VertexWeights.Skinned,
                    Indicies = indices.ToArray(),
                    Vertices = vertices.ToArray(),
                    Submeshes = submeshes
                };
            }
        }

        #endregion
    }

    [Flags]
    public enum ModelFlags : short
    {
        UseLocalNodes = 1
    }

    [FixedSize(64)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(52)]
        public BlockCollection<MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(32)]
    public class MarkerBlock : IGeometryMarker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        //something here

        [Offset(4)]
        public RealVector3D Position { get; set; }

        [Offset(16)]
        public RealVector4D Rotation { get; set; }

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(156)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public short NextSiblingIndex { get; set; }

        [Offset(34)]
        public short FirstChildIndex { get; set; }

        [Offset(36)]
        public short ParentIndex { get; set; }

        //int16 here

        [Offset(40)]
        public RealVector3D Position { get; set; }

        [Offset(52)]
        public RealVector4D Rotation { get; set; }

        [Offset(68)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation.Conjugate;

        #endregion
    }

    [FixedSize(76)]
    public class RegionBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(64)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(88)]
    public class PermutationBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(64)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(66)]
        public short LowSectionIndex { get; set; }

        [Offset(68)]
        public short MediumSectionIndex { get; set; }

        [Offset(70)]
        public short HighSectionIndex { get; set; }

        [Offset(72)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex };

        internal int LodCount => LodArray.Count(i => i >= 0);

        internal short LodIndex(int lod)
        {
            if (lod < 0 || lod > 4)
                throw new ArgumentOutOfRangeException(nameof(lod));

            return LodArray.Take(lod + 1)
                .Reverse()
                .FirstOrDefault(i => i >= 0);
        }

        public override string ToString() => Name;
    }

    [FixedSize(48)]
    public class ModelSectionBlock
    {
        [Offset(36)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }
    }

    [FixedSize(208, MaxVersion = (int)CacheType.Halo1PC)]
    [FixedSize(132, MinVersion = (int)CacheType.Halo1PC)]
    public class SubmeshBlock
    {
        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(72)]
        public int IndexCount { get; set; }

        [Offset(80, MaxVersion = (int)CacheType.Halo1PC)]
        [Offset(76, MinVersion = (int)CacheType.Halo1PC)]
        public int IndexOffset { get; set; }

        [Offset(88)]
        public int VertexCount { get; set; }

        [Offset(100)]
        public int VertexOffset { get; set; }
    }

    [FixedSize(32)]
    public class ShaderBlock
    {
        [Offset(0)]
        public TagReference ShaderReference { get; set; }

        public override string ToString() => ShaderReference.Tag?.FullPath;
    }
}
