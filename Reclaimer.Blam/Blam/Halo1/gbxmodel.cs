using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo1
{
    public class gbxmodel : IRenderGeometry
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public gbxmodel(ICacheFile cache, IIndexItem item)
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

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => Regions.SelectMany(r => r.Permutations).Max(p => p.LodCount);

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            using var reader = cache.CreateReader(cache.DefaultAddressTranslator);

            var model = new GeometryModel(item.FileName) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);

            var shaderRefs = Shaders.Select(s => s.ShaderReference);
            model.Materials.AddRange(Halo1Common.GetMaterials(shaderRefs, reader));

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { SourceIndex = Regions.IndexOf(region), Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Select(p =>
                    new GeometryPermutation
                    {
                        SourceIndex = region.Permutations.IndexOf(p),
                        Name = p.Name,
                        MeshIndex = p.LodIndex(lod),
                        MeshCount = 1
                    }));

                model.Regions.Add(gRegion);
            }

            if (cache.CacheType == CacheType.Halo1Xbox)
                model.Meshes.AddRange(ReadXboxMeshes(reader));
            else
                model.Meshes.AddRange(ReadPCMeshes(reader));

            return model;
        }

        private IEnumerable<GeometryMesh> ReadXboxMeshes(DependencyReader reader)
        {
            if (cache.TagIndex is not ITagIndexGen1 tagIndex)
                throw new NotSupportedException();

            foreach (var section in Sections)
            {
                var indices = new List<int>();
                var submeshes = new List<IGeometrySubmesh>();

                var vertexBuffer = new VertexBuffer();
                var positions = new List<IVector>();
                var normals = new List<IVector>();
                var binormals = new List<IVector>();
                var tangents = new List<IVector>();
                var texCoords = new List<IVector>();
                var boneIndices = new List<IVector>();
                var boneWeights = new List<IVector>();

                vertexBuffer.PositionChannels.Add(positions);
                vertexBuffer.NormalChannels.Add(normals);
                vertexBuffer.BinormalChannels.Add(binormals);
                vertexBuffer.TangentChannels.Add(tangents);
                vertexBuffer.TextureCoordinateChannels.Add(texCoords);
                vertexBuffer.BlendIndexChannels.Add(boneIndices);
                vertexBuffer.BlendWeightChannels.Add(boneWeights);

                var vertexTally = 0;
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

                        reader.Seek(submesh.IndexOffset - tagIndex.Magic, SeekOrigin.Begin);
                        reader.ReadInt32();
                        reader.Seek(reader.ReadInt32() - tagIndex.Magic, SeekOrigin.Begin);

                        var indicesTemp = reader.ReadArray<ushort>(gSubmesh.IndexLength);
                        indices.AddRange(indicesTemp.Select(i => i + vertexTally));

                        reader.Seek(submesh.VertexOffset - tagIndex.Magic, SeekOrigin.Begin);
                        reader.ReadInt32();
                        reader.Seek(reader.ReadInt32() - tagIndex.Magic, SeekOrigin.Begin);

                        for (var i = 0; i < submesh.VertexCount; i++)
                        {
                            positions.Add(new RealVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                            normals.Add(new HenDN3(reader.ReadUInt32()));
                            binormals.Add(new HenDN3(reader.ReadUInt32()));
                            tangents.Add(new HenDN3(reader.ReadUInt32()));
                            texCoords.Add(new RealVector2(reader.ReadInt16() / (float)short.MaxValue, reader.ReadInt16() / (float)short.MaxValue));
                            boneIndices.Add(new UShort2((ushort)(reader.ReadByte() / 3), (ushort)(reader.ReadByte() / 3)));

                            var node0Weight = reader.ReadUInt16() / (float)short.MaxValue;
                            boneWeights.Add(new RealVector2(node0Weight, 1 - node0Weight));
                        }

                        //if (Flags.HasFlag(ModelFlags.UseLocalNodes))
                        //{
                        //    var address = section.Submeshes.Pointer.Address;
                        //    address += section.Submeshes.IndexOf(submesh) * 208;
                        //    reader.Seek(address + 107, SeekOrigin.Begin);
                        //    var nodeCount = reader.ReadByte();
                        //    var nodes = reader.ReadEnumerable<byte>(nodeCount).ToArray();

                        //    vertsTemp.ForEach((v) =>
                        //    {
                        //        v.NodeIndex1 = nodes[v.NodeIndex1];
                        //        v.NodeIndex2 = nodes[v.NodeIndex2];
                        //    });
                        //}

                        vertexTally += submesh.VertexCount;
                    }
                    catch { }
                }

                if (UScale != 1 || VScale != 1)
                {
                    for (var i = 0; i < texCoords.Count; i++)
                    {
                        var v = texCoords[i];
                        texCoords[i] = new RealVector2
                        {
                            X = v.X * UScale,
                            Y = v.Y * VScale
                        };
                    };
                }

                yield return new GeometryMesh
                {
                    IndexFormat = IndexFormat.TriangleStrip,
                    VertexWeights = VertexWeights.Skinned,
                    IndexBuffer = IndexBuffer.FromCollection(indices),
                    VertexBuffer = vertexBuffer,
                    Submeshes = submeshes
                };
            }
        }

        private IEnumerable<GeometryMesh> ReadPCMeshes(DependencyReader reader)
        {
            if (cache.TagIndex is not ITagIndexGen1 tagIndex)
                throw new NotSupportedException();

            const int submeshSize = 132;
            const int vertexSize = 68;

            foreach (var section in Sections)
            {
                var indices = new List<int>();
                var submeshes = new List<IGeometrySubmesh>();

                var vertexCount = section.Submeshes.Sum(s => s.VertexCount);
                var vertexData = new byte[vertexSize * vertexCount];

                var vertexBuffer = new VertexBuffer();
                var texBuffer = new VectorBuffer<RealVector2>(vertexData, vertexCount, vertexSize, 48);
                var boneBuffer = new VectorBuffer<UShort2>(vertexData, vertexCount, vertexSize, 56);

                vertexBuffer.PositionChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 0));
                vertexBuffer.NormalChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 12));
                vertexBuffer.BinormalChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 24));
                vertexBuffer.TangentChannels.Add(new VectorBuffer<RealVector3>(vertexData, vertexCount, vertexSize, 36));
                vertexBuffer.TextureCoordinateChannels.Add(texBuffer);
                vertexBuffer.BlendIndexChannels.Add(boneBuffer);
                vertexBuffer.BlendWeightChannels.Add(new VectorBuffer<RealVector2>(vertexData, vertexCount, vertexSize, 60));

                var vertexTally = 0;
                foreach (var submesh in section.Submeshes)
                {
                    reader.Seek(tagIndex.VertexDataOffset + tagIndex.IndexDataOffset + submesh.IndexOffset, SeekOrigin.Begin);
                    var subIndices = reader.ReadEnumerable<ushort>(submesh.IndexCount + 2).Select(i => i + vertexTally).Unstrip().Reverse().ToList();

                    var gSubmesh = new GeometrySubmesh
                    {
                        MaterialIndex = submesh.ShaderIndex,
                        IndexStart = indices.Count,
                        IndexLength = subIndices.Count
                    };

                    submeshes.Add(gSubmesh);
                    indices.AddRange(subIndices);

                    reader.Seek(tagIndex.VertexDataOffset + submesh.VertexOffset, SeekOrigin.Begin);
                    reader.ReadBytes(vertexSize * submesh.VertexCount).CopyTo(vertexData, vertexTally * vertexSize);

                    if (Flags.HasFlag(ModelFlags.UseLocalNodes))
                    {
                        var address = section.Submeshes.Pointer.Address;
                        address += section.Submeshes.IndexOf(submesh) * submeshSize;
                        reader.Seek(address + 107, SeekOrigin.Begin);
                        var nodeCount = reader.ReadByte();
                        var nodes = reader.ReadBytes(nodeCount);

                        for (var i = vertexTally; i < submesh.VertexCount; i++)
                        {
                            var v = boneBuffer[i];
                            boneBuffer[i] = new UShort2
                            {
                                X = nodes[v.X],
                                Y = nodes[v.Y]
                            };
                        }
                    }

                    vertexTally += submesh.VertexCount;
                }

                if (UScale != 1 || VScale != 1)
                {
                    for (var i = 0; i < texBuffer.Count; i++)
                    {
                        var v = texBuffer[i];
                        texBuffer[i] = new RealVector2
                        {
                            X = v.X * UScale,
                            Y = v.Y * VScale
                        };
                    };
                }

                yield return new GeometryMesh
                {
                    IndexFormat = IndexFormat.TriangleList,
                    VertexWeights = VertexWeights.Skinned,
                    IndexBuffer = IndexBuffer.FromCollection(indices),
                    VertexBuffer = vertexBuffer,
                    Submeshes = submeshes
                };
            }
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => GetBitmaps(Enumerable.Range(0, Shaders?.Count ?? 0));

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes)
        {
            var selection = shaderIndexes?.Distinct().Where(i => i >= 0 && i < Shaders?.Count).Select(i => Shaders[i]);
            if (selection?.Any() != true)
                yield break;

            var complete = new List<int>();
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                foreach (var s in selection)
                {
                    var bitmTag = Halo1Common.GetShaderDiffuse(s.ShaderReference, reader);
                    if (bitmTag == null || complete.Contains(bitmTag.Id))
                        continue;

                    complete.Add(bitmTag.Id);
                    yield return bitmTag.ReadMetadata<bitmap>();
                }
            }
        }

        #endregion
    }

    [Flags]
    public enum ModelFlags : short
    {
        UseLocalNodes = 2
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

        Matrix4x4 IGeometryNode.OffsetTransform => Matrix4x4.Identity;

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
