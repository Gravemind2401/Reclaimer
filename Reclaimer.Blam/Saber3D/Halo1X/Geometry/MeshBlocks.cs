using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DataBlock(0x0701, ExpectedChildCount = 3)] //F300, 0401, 0100
    public class SubmeshBlock0x0701 : CollectionDataBlock
    {
        public List<UnknownBlock0x0301> UnknownItems => GetUniqueChild<UnknownListBlock0xF300>().UnknownItems;
        public List<SubmeshInfo> Submeshes => GetUniqueChild<SubmeshListBlock0x0401>().Submeshes;
    }

    #region Unknown List
    
    [DataBlock(0xF300, ExpectedChildCount = 10)]
    public class UnknownListBlock0xF300 : CollectionDataBlock
    {
        public int Count { get; set; }

        public List<UnknownBlock0x0301> UnknownItems { get; } = new List<UnknownBlock0x0301>();

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            ReadChildren(reader, Count * 2); //empty block after each
            PopulateChildrenOfType(UnknownItems);
        }
    }

    [DataBlock(0x0301)]
    public class UnknownBlock0x0301 : Int32Block
    {
        internal override void Validate()
        {
            if (Value != 0)
                Debugger.Break();
        }
    }

    #endregion

    #region Submesh Data

    [DataBlock(0x0401)]
    public class SubmeshListBlock0x0401 : CollectionDataBlock
    {
        public int Count { get; set; }

        public List<SubmeshInfo> Submeshes { get; } = new List<SubmeshInfo>();

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32(); //refers to number of groups, empty block after each group
            ReadChildren(reader);
            PopulateGroupList(Submeshes, Count);
        }
    }

    public class SubmeshInfo : DataBlockGroup
    {
        public FaceRangeBlock FaceRange => GetUniqueChild<FaceRangeBlock>();
        public VertexRangeBlock VertexRange => GetUniqueChild<VertexRangeBlock>();
        public short? UnknownId => GetOptionalChild<CompoundParentIdBlock>()?.Value;
        public List<MaterialInfoGroup> Materials => GetUniqueChild<MaterialListBlock0x0B01>().Materials;
        public SubmeshBlock0x3201 UnknownBoneDetails => GetOptionalChild<SubmeshBlock0x3201>();
        public MaterialBlock0x1C01 UnknownMaterial0 => GetUniqueChild<MaterialBlock0x1C01>();
        public MaterialBlock0x2001 UnknownMaterial1 => GetUniqueChild<MaterialBlock0x2001>();
        public SubmeshBlock0x2801 UnknownMeshDetails => GetOptionalChild<SubmeshBlock0x2801>();

        public NodeGraphBlock0xF000 CompoundParent => GetOptionalChild<CompoundParentIdBlock>()?.CompoundParent;
    }

    [DataBlock(0x0501, ExpectedSize = 8)]
    public class FaceRangeBlock : RangeBlock
    {

    }

    [DataBlock(0x0D01, ExpectedSize = 8)]
    public class VertexRangeBlock : RangeBlock
    {

    }

    [DataBlock(0x3201, ExpectedSize = 6)] //mutally exclusive with 0x3401?
    public class SubmeshBlock0x3201 : DataBlock
    {
        [Offset(0)]
        public short UnknownId0 { get; set; } //points to first inheritor if skincompound, otherwise parent bone

        [Offset(2)]
        public byte UnknownCount0 { get; set; } //number of inheritors/bones (starts at UnknownId0 and increments through object IDs)

        [Offset(3)]
        public short UnknownId1 { get; set; } //secondary parent bone

        [Offset(5)]
        public byte UnknownCount1 { get; set; } //secondary number of bones
    }

    [DataBlock(0x3401)]
    public class CompoundParentIdBlock : Int16Block
    {
        //ID pointing to inherited sharingObj / parent skin compound

        public NodeGraphBlock0xF000 CompoundParent => Owner.NodeLookup[Value];

        protected override object GetDebugProperties() => new { Value, Name = CompoundParent?.MeshName };
    }

    #region Material Data List

    [DataBlock(0x0B01)]
    public class MaterialListBlock0x0B01 : CollectionDataBlock
    {
        public int Count { get; set; }

        public List<MaterialInfoGroup> Materials { get; } = new List<MaterialInfoGroup>();

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            ReadChildren(reader);
            PopulateGroupList(Materials, Count);
        }
    }

    public class MaterialInfoGroup : DataBlockGroup
    {
        public int MaterialIndex => GetUniqueChild<MaterialIndexBlock>().MaterialIndex;
        public MaterialBlock0x1401 Unknown0 => GetUniqueChild<MaterialBlock0x1401>();
        public MaterialBlock0x1F01 Unknown1 => GetUniqueChild<MaterialBlock0x1F01>();
        public string UnknownMaterialString => GetUniqueChild<StringBlock0xBA01>().Value;
    }

    [DataBlock(0x0E01, ExpectedSize = 10)]
    public class MaterialIndexBlock : DataBlock
    {
        [Offset(0)]
        public int MaterialIndex { get; set; }
        
        [Offset(4)]
        public int Unknown0 { get; set; } //-1

        [Offset(8)]
        public short Unknown1 { get; set; } //0x00FF or 0xFFFF

        protected override object GetDebugProperties() => new { MaterialIndex, Unknown0, Unknown1 };
    }

    [DataBlock(0x1401)]
    public class MaterialBlock0x1401 : Int16Block
    {
        //-1 or 1
    }

    [DataBlock(0x1F01)]
    public class MaterialBlock0x1F01 : Int16Block
    {
        //0x00FF
    }

    //0xBA01 (CommonBlocks.cs)

    #endregion

    [DataBlock(0x1C01, ExpectedSize = 8)]
    public class MaterialBlock0x1C01 : DataBlock
    {
        [Offset(0)]
        public float Unknown0 { get; set; }

        [Offset(4)]
        public float Unknown1 { get; set; }

        protected override object GetDebugProperties() => new { Unknown0, Unknown1 };
    }

    [DataBlock(0x2001, ExpectedSize = 32)]
    public class MaterialBlock0x2001 : DataBlock
    {
        [Offset(0)]
        public float Unknown0 { get; set; }

        [Offset(4)]
        public float Unknown1 { get; set; }

        [Offset(8)]
        public float Unknown2 { get; set; }

        [Offset(12)]
        public float Unknown3 { get; set; }

        [Offset(16)]
        public float Unknown4 { get; set; }

        [Offset(20)]
        public float Unknown5 { get; set; }

        [Offset(24)]
        public float Unknown6 { get; set; }

        [Offset(28)]
        public float Unknown7 { get; set; }
    }

    [DataBlock(0x2801, ExpectedSize = 28)]
    public class SubmeshBlock0x2801 : DataBlock
    {
        public byte Unknown0 { get; set; } //0x81
        public int Unknown1 { get; set; }
        public byte Unknown2 { get; set; } //0xFF
        public short UnknownEnum { get; set; }

        public short VertexCount { get; set; }
        public short IndexCount { get; set; }
        public int UnknownId { get; set; }
        public int Unknown3 { get; set; }
        public int Unknown4 { get; set; }
        public short Unknown5 { get; set; }
        public short Unknown6 { get; set; }

        internal override void Read(EndianReader reader)
        {
            Unknown0 = reader.ReadByte();
            Unknown1 = reader.ReadInt32();
            Unknown2 = reader.ReadByte();
            UnknownEnum = reader.ReadInt16(); //mesh type enum? 16 = standard, 18 = skin, 19 = skincompound

            VertexCount = reader.ReadInt16(); //vertex count
            IndexCount = reader.ReadInt16(); //face count * 3 [usually]
            UnknownId = reader.ReadInt32(); //object ID, unknown purpose, same as parent ID, only used on vertless meshes (inheritors)
            Unknown3 = reader.ReadInt32(); //increases with vert count
            Unknown4 = reader.ReadInt32(); //seems to increase with mesh size
            Unknown5 = reader.ReadInt16(); //not used on standard meshes
            Unknown6 = reader.ReadInt16(); //not used on standard meshes

            EndRead(reader.Position);
        }

        protected override object GetDebugProperties() => new { VertexCount, IndexCount };
    }

    #endregion
}
