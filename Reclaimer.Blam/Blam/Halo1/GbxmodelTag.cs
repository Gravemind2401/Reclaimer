using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo1
{
    public class GbxModelTag : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public GbxModelTag(IIndexItem item)
            : base(item)
        { }

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

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            const int lod = 0;

            using var reader = Cache.CreateReader(Cache.DefaultAddressTranslator);

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);

            model.Bones.AddRange(Nodes.Select(n => new Bone
            {
                Name = n.Name,
                LocalTransform = Utils.CreateMatrix(n.Position, n.Rotation.Conjugate),
                ParentIndex = n.ParentIndex
            }));

            model.Markers.AddRange(MarkerGroups.Select(g =>
            {
                var marker = new Marker { Name = g.Name };
                marker.Instances.AddRange(g.Markers.Select(m => new MarkerInstance
                {
                    Position = (Vector3)m.Position,
                    Rotation = new Quaternion(m.Rotation.X, m.Rotation.Y, m.Rotation.Z, m.Rotation.W),
                    RegionIndex = m.RegionIndex,
                    PermutationIndex = m.PermutationIndex,
                    BoneIndex = m.NodeIndex
                }));

                return marker;
            }));

            model.Regions.AddRange(Regions.Select(r =>
            {
                var region = new ModelRegion { Name = r.Name };
                region.Permutations.AddRange(r.Permutations.Select(p => new ModelPermutation
                {
                    Name = p.Name,
                    MeshRange = (p.LodIndex(lod), 1)
                }));

                return region;
            }));

            var materials = Halo1Common.GetMaterials(Shaders.Select(s => s.ShaderReference), reader).ToList();

            if (Cache.CacheType == CacheType.Halo1Xbox)
                model.Meshes.AddRange(ReadXboxMeshes(reader, materials));
            else
                model.Meshes.AddRange(ReadPCMeshes(reader, materials));

            return model;
        }

        private IEnumerable<Mesh> ReadXboxMeshes(DependencyReader reader, List<Material> materials)
        {
            if (Cache.TagIndex is not ITagIndexGen1 tagIndex)
                throw new NotSupportedException();

            foreach (var section in Sections)
            {
                var mesh = new Mesh();
                var indices = new List<int>();

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
                        var segment = new MeshSegment
                        {
                            Material = materials.ElementAtOrDefault(submesh.ShaderIndex),
                            IndexStart = indices.Count,
                            IndexLength = submesh.IndexCount + 2
                        };

                        mesh.Segments.Add(segment);

                        reader.Seek(submesh.IndexOffset - tagIndex.Magic, SeekOrigin.Begin);
                        reader.ReadInt32();
                        reader.Seek(reader.ReadInt32() - tagIndex.Magic, SeekOrigin.Begin);

                        var indicesTemp = reader.ReadArray<ushort>(segment.IndexLength);
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

                mesh.IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList);
                mesh.VertexBuffer = vertexBuffer;

                yield return mesh;
            }
        }

        private IEnumerable<Mesh> ReadPCMeshes(DependencyReader reader, List<Material> materials)
        {
            if (Cache.TagIndex is not ITagIndexGen1 tagIndex)
                throw new NotSupportedException();

            const int submeshSize = 132;
            const int vertexSize = 68;

            foreach (var section in Sections)
            {
                var mesh = new Mesh();
                var indices = new List<int>();

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
                    var subIndices = reader.ReadArray<ushort>(submesh.IndexCount + 2).Select(i => i + vertexTally).Unstrip().Reverse().ToList();

                    var segment = new MeshSegment
                    {
                        Material = materials.ElementAtOrDefault(submesh.ShaderIndex),
                        IndexStart = indices.Count,
                        IndexLength = subIndices.Count
                    };

                    mesh.Segments.Add(segment);
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

                        for (var i = vertexTally; i < vertexTally + submesh.VertexCount; i++)
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

                mesh.IndexBuffer = IndexBuffer.FromCollection(indices, IndexFormat.TriangleList);
                mesh.VertexBuffer = vertexBuffer;

                yield return mesh;
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
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(52)]
        public BlockCollection<MarkerBlock> Markers { get; set; }
    }

    [FixedSize(32)]
    public class MarkerBlock
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        //something here

        [Offset(4)]
        public RealVector3 Position { get; set; }

        [Offset(16)]
        public RealVector4 Rotation { get; set; }

        public override string ToString() => Position.ToString();
    }

    [FixedSize(156)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class NodeBlock
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
        public RealVector3 Position { get; set; }

        [Offset(52)]
        public RealVector4 Rotation { get; set; }

        [Offset(68)]
        public float DistanceFromParent { get; set; }
    }

    [FixedSize(76)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(64)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(88)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
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

        internal short[] LodArray => [SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex];

        internal int LodCount => LodArray.Count(i => i >= 0);

        internal short LodIndex(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, LodArray.Length);

            return LodArray.Take(lod + 1)
                .Reverse()
                .FirstOrDefault(i => i >= 0);
        }
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
    [DebuggerDisplay($"{{{nameof(ShaderReference)},nq}}")]
    public class ShaderBlock
    {
        [Offset(0)]
        public TagReference ShaderReference { get; set; }
    }
}
