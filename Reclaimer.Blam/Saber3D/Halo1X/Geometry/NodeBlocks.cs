using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public enum ObjectType
    {
        Branch = 0,

        RootNode,
        Bone,

        StandardMesh,
        DeferredSkinMesh,
        DeferredSceneMesh,

        SkinCompound,
        SharingObj
    }

    [DataBlock(0xF000)]
    public class NodeGraphBlock0xF000 : CollectionDataBlock
    {
        public ObjectType ObjectType
        {
            get
            {
                if (IsRootNode)
                    return ObjectType.RootNode;

                if (BoneIndex >= 0)
                    return ObjectType.Bone;

                if (MeshDataSource?.SourceMeshId > 0)
                    return ObjectType.DeferredSceneMesh;

                if (SubmeshData?.Submeshes.Any(s => s.UnknownMeshDetails?.UnknownEnum == 19) == true)
                    return ObjectType.SkinCompound;

                if (SubmeshData?.Submeshes.Any(s => s.CompoundSourceId.HasValue) == true)
                    return ObjectType.DeferredSkinMesh;

                if (MeshId.HasValue && Owner.NodeGraph.AllDescendants.Any(n => n.MeshDataSource?.SourceMeshId == MeshId.Value))
                    return ObjectType.SharingObj;

                if (Positions?.Count > 0)
                    return ObjectType.StandardMesh;

                return ObjectType.Branch;
            }
        }

        public List<NodeGraphBlock0xF000> AllDescendants { get; } = new List<NodeGraphBlock0xF000>();

        public bool IsRootNode => ChildBlocks[0] is CountBlock0x2C01;
        public int? MeshId => Mesh?.Id;
        public string MeshName => Mesh?.Name;

        public int DescendantCount => GetOptionalChild<CountBlock0x2C01>()?.Value ?? default;
        public MeshBlock0xB903 Mesh => GetOptionalChild<MeshBlock0xB903>();
        public MeshFlagsBlock0x2E01 MeshFlags => GetOptionalChild<MeshFlagsBlock0x2E01>();
        public VertexPositionListBlock Positions => GetOptionalChild<VertexPositionListBlock>();
        public VertexDataListBlock VertexData => GetOptionalChild<VertexDataListBlock>();
        public FaceListBlock Faces => GetOptionalChild<FaceListBlock>();
        public BoundsBlock0x1D01 Bounds => GetOptionalChild<BoundsBlock0x1D01>();
        public Matrix4x4? Transform => GetOptionalChild<MatrixBlock0xF900>()?.Value;
        public int? BoneIndex => GetOptionalChild<BoneIndexBlock>()?.Value;
        public string UnknownString0xFD00 => GetOptionalChild<UnknownBlock0xFD00>()?.UnknownString;
        public string UnknownString0x1501 => GetOptionalChild<StringBlock0x1501>()?.Value;
        public SubmeshBlock0x0701 SubmeshData => GetOptionalChild<SubmeshBlock0x0701>();
        public BlendDataBlock BlendData => GetOptionalChild<BlendDataBlock>();
        public int? ParentId => GetOptionalChild<ParentIdBlock>()?.Value;

        public MeshDataSourceBlock MeshDataSource => GetOptionalChild<MeshDataSourceBlock>(); //this mesh doesnt have its own data
        public SceneObjectBoundsBlock0x3501 SceneObjectBounds => GetOptionalChild<SceneObjectBoundsBlock0x3501>();

        public NodeGraphBlock0xF000 ParentNode => GetOptionalChild<ParentIdBlock>()?.ParentNode;
        
        public IEnumerable<NodeGraphBlock0xF000> ChildNodes => IsRootNode
            ? AllDescendants.Where(c => !c.ParentId.HasValue)
            : Owner.NodeGraph.AllDescendants.Where(c => MeshId.HasValue && c.ParentId == MeshId);

        private Matrix4x4? cachedTransform;
        public Matrix4x4 GetFinalTransform()
        {
            if (cachedTransform == null)
            {
                if (ObjectType == ObjectType.Bone && MeshId < Owner.MatrixList?.MatrixCount)
                {
                    //the bone's offset matrix is already world-relative so no need to check the parent matrix
                    var offsetMatrix = Owner.MatrixList.Matrices[MeshId.Value];
                    cachedTransform = offsetMatrix.Inverse();
                }
                else
                {
                    var parentTransform = ParentNode?.GetFinalTransform() ?? Matrix4x4.Identity;
                    cachedTransform = (Transform ?? Matrix4x4.Identity) * parentTransform;
                }
            }

            return cachedTransform.Value;
        }

        public int? GetAncestorBoneIndex()
        {
            var node = this;
            while (node != null && node.ObjectType != ObjectType.Bone)
                node = node.ParentNode;

            return node?.BoneIndex;
        }

        internal override void Read(EndianReader reader)
        {
            ReadChildren(reader);
            PopulateChildrenOfType(AllDescendants);
        }

        internal override void Validate()
        {
            if (ChildBlocks[0] is CountBlock0x2C01 c && ChildBlocks.Count != c.Value * 2 + 1)
                Debugger.Break();

            if (FilterChildren<MeshBlock0xB903>().Skip(1).Any())
                Debugger.Break();

            var blend = BlendData;
            if (blend != null)
            {
                if (blend.IndexData == null)
                    Debugger.Break();

                if (blend.UnknownBlendDetails.UnknownCount != 4)
                    Debugger.Break();

                if (blend.UnknownBlendDetails.NodeCount != blend.IndexData?.NodeCount)
                    Debugger.Break();

                if (blend.IndexData != null && blend.IndexData.Header.BlockSize != 4 + Mesh.VertexCount * 4)
                    Debugger.Break();

                if (blend.WeightData != null && blend.WeightData.Header.BlockSize != Mesh.VertexCount * 4)
                    Debugger.Break();
            }
        }

        protected override object GetDebugProperties()
        {
            var hasGeo = Mesh?.VertexCount > 0;
            return IsRootNode
                ? new { ObjectType, DescendantCount }
                : new { ObjectType, ChildCount = ChildBlocks.Count, HasGeo = hasGeo, Id = MeshId, ParentId, BoneIdx = BoneIndex, Name = MeshName };
        }
    }

    [DataBlock(0x2C01)] //only on root node
    public class CountBlock0x2C01 : Int32Block
    {

    }

    [DataBlock(0xB903)]
    public class MeshBlock0xB903 : DataBlock
    {
        public string Name { get; set; }
        public short Id { get; set; }
        public short Unknown0 { get; set; } //0x2400
        public byte Unknown1 { get; set; }
        public short Unknown2 { get; set; } //flags?
        public short Unknown3 { get; set; } //flags?
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }

        internal override void Read(EndianReader reader)
        {
            Name = reader.ReadNullTerminatedString();
            Id = reader.ReadInt16();
            Unknown0 = reader.ReadInt16();
            Unknown1 = reader.ReadByte();
            Unknown2 = reader.ReadInt16();
            Unknown3 = reader.ReadInt16();
            VertexCount = reader.ReadInt32();
            FaceCount = reader.ReadInt32();

            EndRead(reader.Position);
        }

        protected override object GetDebugProperties() => new { Id, VertexCount, Name };
    }

    [DataBlock(0x2901, ExpectedSize = 10)] //only appears in scene models
    public class MeshDataSourceBlock : DataBlock
    {
        [Offset(0)]
        public short SourceMeshId { get; set; }

        [Offset(2)]
        public int VertexOffset { get; set; }

        [Offset(6)]
        public int FaceOffset { get; set; }

        protected override object GetDebugProperties() => new { SourceMeshId, VertexOffset, FaceOffset, Name = Owner.GetDebugObjectName(SourceMeshId) };
    }

    [DataBlock(0x2E01, ExpectedSize = 5)]
    public class MeshFlagsBlock0x2E01 : DataBlock
    {
        [Offset(0)]
        public short Unknown0 { get; set; } //always 0x1200

        [Offset(2)]
        public MeshFlags Flags { get; set; }

        [Offset(4)]
        public byte Unknown1 { get; set; } //1 if has vertex buffer, else 0

        public bool HasVertexData => Unknown1 > 0;

        protected override object GetDebugProperties() => new { Flags = (short)Flags, Hex = $"0x{(short)Flags:X2}", Bits = Convert.ToString((short)Flags, 2).PadLeft(16, '0'), Unknown0, Unknown1 };
    }

    public enum MeshFlags : short
    {
        None = 0,
        Compressed = 1,
        HasNormals = 2,

        //below are common combinations

        WorldVertex1 = 0x86,
        WorldVertex2 = 0x8E,

        Materials1 = 0x87,
        Materials2 = 0x8F,
        Materials3 = 0x9F,
        Materials4 = 0xBF,
    }

    #region Scene Blocks

    //below blocks only appear in scene models

    [DataBlock(0x3501, ExpectedSize = 12)]
    public class SceneObjectBoundsBlock0x3501 : DataBlock
    {
        public Vector3 Translation { get; set; } //translate X, Y, Z (int16)
        public Vector3 Scale { get; set; } //scale X, Y, Z (int16)

        public Matrix4x4 GetTransform() => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Translation);

        internal override void Read(EndianReader reader)
        {
            Translation = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            Scale = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());

            EndRead(reader.Position);
        }
    }

    //0x2301 (unmapped, always empty?)

    //0x3101 (unmapped, always empty?)

    //0x2A01 (unmapped, always empty?)

    #endregion

    #region Resource Data

    [DataBlock(0xF100)]
    public class VertexPositionListBlock : DataBlock
    {
        public int Count { get; set; }

        public Vector3 Translation { get; set; } // (int16 * 3, only if compressed)
        public Vector3 Scale { get; set; } // (int16 * 3, only if compressed)

        // Count * either [float32 * 3] (uncompressed) or [int16 * 4] (compressed)
        public IVectorBuffer PositionBuffer { get; set; }

        public Matrix4x4 GetTransform() => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Translation);

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();

            var compressed = ((NodeGraphBlock0xF000)ParentBlock).MeshFlags?.Flags.HasFlag(MeshFlags.Compressed) ?? false;

            (Translation, Scale) = compressed
                ? (new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()), new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()))
                : (Vector3.Zero, Vector3.One);

            if (Count == 0)
                return;

            //when compressed, each vertex takes up 4 shorts
            //the 4th short contains the normal vector
            const int stride = sizeof(ushort) * 4;

            var bufferBytes = reader.ReadBytes((int)(Header.EndOfBlock - reader.Position));
            PositionBuffer = compressed
                ? VectorBuffer.Transform3d(new VectorBuffer<Int16N3>(bufferBytes, Count, stride), GetTransform())
                : new VectorBuffer<RealVector3>(bufferBytes);

            if (PositionBuffer.Count != Count)
                Debugger.Break();

            EndRead(reader.Position);
        }

        protected override object GetDebugProperties() => new { VertexCount = Count };
    }

    [DataBlock(0x3001)] //contains texcoords (big endian) + other data (unknown)
    public class VertexDataListBlock : DataBlock
    {
        internal override int ExpectedSize => 13 + Count * DataSize;

        [Offset(0)]
        public int Count { get; set; }

        [Offset(4)]
        public short Unknown0 { get; set; } //0x2E00

        //big endian from here on?

        [Offset(6)]
        public short Unknown1 { get; set; } //flags? 0x1C00 if uncompressed positions

        [Offset(8)]
        public byte Unknown2 { get; set; }

        [Offset(9)]
        public byte Unknown3 { get; set; }

        [Offset(10)]
        public byte Unknown4 { get; set; }

        [Offset(11)]
        public byte Unknown5 { get; set; } //0x00 if uncompressed positions, else 0x20

        [Offset(12)]
        public byte DataSize { get; set; }

        // + Count * DataSize bytes
        private byte[] bufferBytes;
        public IVectorBuffer TexCoordsBuffer { get; set; }

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            Unknown0 = reader.ReadInt16();
            Unknown1 = reader.ReadInt16();
            Unknown2 = reader.ReadByte();
            Unknown3 = reader.ReadByte();
            Unknown4 = reader.ReadByte();
            Unknown5 = reader.ReadByte();
            DataSize = reader.ReadByte();

            var offset = DataSize switch
            {
                8 => 4,
                12 => 4,
                16 => 12,
                20 => 16,
                24 => 16,
                28 => 20,
                32 => 16,
                36 => 24,
                44 => 28,
                _ => default
            };

            bufferBytes = reader.ReadBytes(Count * DataSize);
            TexCoordsBuffer = new VectorBuffer<Int16N2>(bufferBytes, Count, 0, DataSize, offset);
            TexCoordsBuffer.ReverseEndianness();
        }

        protected override object GetDebugProperties() => new { VertexCount = Count, VertexSize = DataSize };
    }

    [DataBlock(0xF200)]
    public class FaceListBlock : DataBlock
    {
        internal override int ExpectedSize => 4 + 2 * Count * 3;

        public int Count { get; set; }

        // Count * [ushort * 3]
        public IndexBuffer IndexBuffer { get; set; }

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            if (Count == 0)
                return;

            IndexBuffer = new IndexBuffer(reader.ReadBytes(sizeof(ushort) * 3 * Count), typeof(ushort)) { Layout = IndexFormat.TriangleList };
        }

        protected override object GetDebugProperties() => new { Faces = Count, Size = Header.BlockSize };
    }

    [DataBlock(0x1D01, ExpectedSize = 4 + 4 * 3 * 2)]
    public class BoundsBlock0x1D01 : BoundsBlock0x0803
    {
        //sharingObj bounds always between -1,1
    }

    [DataBlock(0xF800)]
    public class UnknownBlock0xF800 : Int32Block
    {
        //index? -1 so far

        internal override void Validate()
        {
            if (Value != -1)
                Debugger.Break();
        }
    }

    [DataBlock(0x2F01)]
    public class MaterialBlock0x2F01 : DataBlock
    {
        internal override int ExpectedSize => 1 + 5 * Count;

        public byte Count { get; set; }
        public (byte Index, int Count)[] UnknownArray { get; set; }

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadByte();
            UnknownArray = new (byte, int)[Count];

            for (var i = 0; i < Count; i++)
                UnknownArray[i] = (reader.ReadByte(), reader.ReadInt32());
        }
    }

    #endregion

    [DataBlock(0xF900, ExpectedSize = 4 * 16)]
    public class MatrixBlock0xF900 : DataBlock
    {
        public Matrix4x4 Value { get; set; }

        internal override void Read(EndianReader reader)
        {
            Value = reader.ReadMatrix4x4();
        }

        protected override object GetDebugProperties() => new { Value.IsIdentity };
    }

    [DataBlock(0xFA00)]
    public class BoneIndexBlock : Int32Block
    {

    }

    [DataBlock(0x8304)] //only appears in scene models
    public class UnknownScriptBlock0x0F03 : Int32Block
    {
        //Script index? appears on nodes where name starts with Zone
    }

    [DataBlock(0xFD00, ExpectedChildCount = 1)] //only on root node's first child
    public class UnknownBlock0xFD00 : CollectionDataBlock
    {
        public string UnknownString => GetOptionalChild<StringBlock0xBA01>()?.Value;
    }

    //0x0701 (MeshBlocks.cs)

    #region Blend Data

    [DataBlock(0x1601)]
    public class BlendDataBlock : CollectionDataBlock
    {
        public UnknownBlendDetailsBlock0x1701 UnknownBlendDetails => GetUniqueChild<UnknownBlendDetailsBlock0x1701>();
        public BlendIndexBufferBlock IndexData => GetOptionalChild<BlendIndexBufferBlock>();
        public BlendWeightBufferBlock WeightData => GetOptionalChild<BlendWeightBufferBlock>();
    }

    [DataBlock(0x1701, ExpectedSize = 8)]
    public class UnknownBlendDetailsBlock0x1701 : DataBlock
    {
        [Offset(0)]
        public int NodeCount { get; set; } //same as 0x3301 NodeCount

        [Offset(4)]
        public int UnknownCount { get; set; } //always 4 so far (max indices/weights per vertex?)

        protected override object GetDebugProperties() => new { NodeCount, UnknownCount };
    }

    [DataBlock(0x3301)]
    public class BlendIndexBufferBlock : DataBlock
    {
        //blend indices are relative to FirstNodeId and refer to node IDs
        //on skin compound meshes, each vertex has a single blend index (int32?) and is transformed as part of the referenced node

        public short FirstNodeId { get; set; }
        public short NodeCount { get; set; }

        //UByte4 * vertex count
        public VectorBuffer<UByte4> BlendIndexBuffer { get; set; }

        //creates a copy of the blend index buffer but using actual bone indexes instead of relative node IDs
        public VectorBuffer<UByte4> CreateMappedIndexBuffer()
        {
            var indexMap = (from i in Enumerable.Range(0, NodeCount)
                            join n in Owner.NodeGraph.AllDescendants on FirstNodeId + i equals n.MeshId
                            select (Index: i, BoneIndex: n.GetAncestorBoneIndex().Value)).ToDictionary(t => t.Index, t => (byte)t.BoneIndex);

            var indexBytes = (byte[])BlendIndexBuffer.GetBuffer().Clone();
            var indexBuffer = new VectorBuffer<UByte4>(indexBytes);
            for (var i = 0; i < indexBuffer.Count; i++)
            {
                //doesnt matter that unused indices get mapped since the weights for them will be zero
                var (x, y, z, w) = indexBuffer[i];
                indexBuffer[i] = new UByte4(indexMap[x], indexMap[y], indexMap[x], indexMap[w]);
            }

            return indexBuffer;
        }

        internal override void Read(EndianReader reader)
        {
            FirstNodeId = reader.ReadInt16();
            NodeCount = reader.ReadInt16();

            var bufferBytes = reader.ReadBytes((int)(Header.EndOfBlock - reader.Position));
            BlendIndexBuffer = new VectorBuffer<UByte4>(bufferBytes);

            EndRead(reader.Position);
        }

        protected override object GetDebugProperties() => new { FirstNodeId, NodeCount, Nodes = Owner.GetDebugObjectNames(FirstNodeId, NodeCount) };
    }

    [DataBlock(0x1A01)]
    public class BlendWeightBufferBlock : DataBlock
    {
        //on skin compound meshes, this appears to be all zero but is essentially the same as 100% weight on a single node

        //UByteN4 * vertex count
        public VectorBuffer<UByteN4> BlendWeightBuffer { get; set; }

        internal override void Read(EndianReader reader)
        {
            var bufferBytes = reader.ReadBytes((int)(Header.EndOfBlock - reader.Position));
            BlendWeightBuffer = new VectorBuffer<UByteN4>(bufferBytes);
            EndRead(reader.Position);
        }
    }

    #endregion

    //0x1501 (CommonBlocks.cs)

    [DataBlock(0x2B01)]
    public class ParentIdBlock : Int32Block
    {
        public NodeGraphBlock0xF000 ParentNode => Owner.NodeLookup[Value];

        protected override object GetDebugProperties() => new { Value, Name = ParentNode?.MeshName };
    }
}
