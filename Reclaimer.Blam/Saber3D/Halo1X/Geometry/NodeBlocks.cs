using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DataBlock(0xF000)]
    public class NodeGraphBlock0xF000 : CollectionDataBlock
    {
        public List<NodeGraphBlock0xF000> AllDescendants { get; } = new List<NodeGraphBlock0xF000>();

        public bool IsRootNode => ChildBlocks[0] is CountBlock0x2C01;
        public int? MeshId => Mesh?.Id;
        public string MeshName => Mesh?.Name;

        public int DescendantCount => GetOptionalChild<CountBlock0x2C01>()?.Value ?? default;
        public MeshBlock0xB903 Mesh => GetOptionalChild<MeshBlock0xB903>();
        public VertexPositionListBlock Positions => GetOptionalChild<VertexPositionListBlock>();
        public VertexDataListBlock VertexData => GetOptionalChild<VertexDataListBlock>();
        public FaceListBlock Faces => GetOptionalChild<FaceListBlock>();
        public BoundsBlock0x1D01 Bounds => GetOptionalChild<BoundsBlock0x1D01>();
        public Matrix4x4? Transform => GetOptionalChild<MatrixBlock0xF900>()?.Value;
        public int? BoneIndex => GetOptionalChild<BoneIndexBlock>()?.Value;
        public string UnknownString0xFD00 => GetOptionalChild<UnknownBlock0xFD00>()?.UnknownString;
        public string UnknownString0x1501 => GetOptionalChild<StringBlock0x1501>()?.Value;
        public SubmeshBlock0x0701 SubmeshData => GetOptionalChild<SubmeshBlock0x0701>();
        public BlendDataBlock0x1601 BlendData => GetOptionalChild<BlendDataBlock0x1601>();
        public int? ParentId => GetOptionalChild<ParentIdBlock>()?.Value;

        public NodeGraphBlock0xF000 ParentNode => GetOptionalChild<ParentIdBlock>()?.ParentNode;
        public IEnumerable<NodeGraphBlock0xF000> ChildNodes => IsRootNode
            ? AllDescendants.Where(c => !c.ParentId.HasValue)
            : Owner.NodeGraph.AllDescendants.Where(c => MeshId.HasValue && c.ParentId == MeshId);

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
                if (blend.BlendIndices == null)
                    Debugger.Break();

                if (blend.Unknown0.Unknown1 != 4)
                    Debugger.Break();

                if (blend.Unknown0.Unknown0 != blend.BlendIndices?.BoneCount)
                    Debugger.Break();

                if (blend.BlendIndices != null && blend.BlendIndices.Header.BlockSize != 4 + Mesh.VertexCount * 4)
                    Debugger.Break();

                if (blend.BlendWeights != null && blend.BlendWeights.Header.BlockSize != Mesh.VertexCount * 4)
                    Debugger.Break();
            }
        }

        protected override object GetDebugProperties()
        {
            var hasGeo = Mesh?.VertexCount > 0;
            return IsRootNode
                ? new { DescendantCount }
                : new { ChildCount = ChildBlocks.Count, HasGeo = hasGeo, Id = MeshId, ParentId, BoneIdx = BoneIndex, Name = MeshName };
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

    [DataBlock(0x2E01, ExpectedSize = 5)]
    public class MeshBlock0x2E01 : DataBlock
    {
        [Offset(0)]
        public short Unknown0 { get; set; } //0x1200

        [Offset(2)]
        public byte UnknownEnum { get; set; } //maybe flags? 03 for no materials, 86/8E for world, 87 for one material, 8F for two, 9F for three, BF for four

        [Offset(3)]
        public short Unknown1 { get; set; } //often 0x4001
    }

    #region Resource Data

    [DataBlock(0xF100)]
    public class VertexPositionListBlock : DataBlock
    {
        [Offset(0)]
        public int Count { get; set; }

        // + center (int16 * 3, optional)
        // + radius (int16 * 3, optional)

        // + vertex * Count (either float32 * 3 or int16 * 4)

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

        protected override object GetDebugProperties() => new { VertexCount = Count, VertexSize = DataSize };
    }

    [DataBlock(0xF200)]
    public class FaceListBlock : DataBlock
    {
        internal override int ExpectedSize => 4 + 2 * Count * 3;

        [Offset(0)]
        public int Count { get; set; }

        // + ushort * Count * 3

        protected override object GetDebugProperties() => new { IndexCount = Count };
    }

    [DataBlock(0x1D01, ExpectedSize = 4 + 4 * 3 * 2)]
    public class BoundsBlock0x1D01 : BoundsBlock0x0803
    {

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

    [DataBlock(0xFD00, ExpectedChildCount = 1)] //only on root node's first child
    public class UnknownBlock0xFD00 : CollectionDataBlock
    {
        public string UnknownString => GetOptionalChild<StringBlock0xBA01>()?.Value;
    }

    //0x0701 (MeshBlocks.cs)

    #region Blend Data

    [DataBlock(0x1601)]
    public class BlendDataBlock0x1601 : CollectionDataBlock
    {
        public UnknownBlock0x1701 Unknown0 => GetUniqueChild<UnknownBlock0x1701>();
        public BlendIndexBlock BlendIndices => GetOptionalChild<BlendIndexBlock>();
        public BlendWeightBlock BlendWeights => GetOptionalChild<BlendWeightBlock>();
    }

    [DataBlock(0x1701, ExpectedSize = 8)]
    public class UnknownBlock0x1701 : DataBlock
    {
        [Offset(0)]
        public int Unknown0 { get; set; }

        [Offset(4)]
        public int Unknown1 { get; set; }

        protected override object GetDebugProperties() => new { Unknown0, Unknown1 };
    }

    [DataBlock(0x3301)]
    public class BlendIndexBlock : DataBlock
    {
        [Offset(0)]
        public short FirstBoneId { get; set; }

        [Offset(2)]
        public short BoneCount { get; set; }

        // + UByte4 * vertex count

        protected override object GetDebugProperties() => new { FirstBoneId, BoneCount };
    }

    [DataBlock(0x1A01)]
    public class BlendWeightBlock : DataBlock
    {
        //UByteN4 * vertex count
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
