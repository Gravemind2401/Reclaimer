using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    #region Submesh Data

    [DataBlock(0x3201, ExpectedSize = 6)]
    public class SubmeshBlock0x3201 : DataBlock
    {
        [Offset(0)]
        public short UnknownId0 { get; set; } //points to first inheritor if skincompound, otherwise parent bone

        [Offset(2)]
        public byte UnknownCount0 { get; set; } //number of inheritors/bones (starts at unkID0 and increments through object IDs)

        [Offset(3)]
        public short UnknownId1 { get; set; } //secondary parent bone

        [Offset(5)]
        public byte UnknownCount1 { get; set; } //secondary number of bones
    }

    [DataBlock(0x3401, ExpectedSize = 2)]
    public class SubmeshBlock0x3401 : DataBlock
    {
        [Offset(0)]
        public short UnknownId0 { get; set; } //points to inherited sharingObj
    }

    [DataBlock(0x1C01, ExpectedSize = 8)]
    public class MaterialBlock0x1C01 : DataBlock
    {
        [Offset(0)]
        public float Unknown0 { get; set; }

        [Offset(4)]
        public float Unknown1 { get; set; }
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
    }

    #endregion

    #region Blend Data

    [DataBlock(0x3301)]
    public class BlendIndexBlock : DataBlock
    {
        [Offset(0)]
        public short FirstBoneId { get; set; }

        [Offset(2)]
        public short BoneCount { get; set; }

        // + UByte4 * vertex count
    }

    [DataBlock(0x1A01)]
    public class BlendWeightBlock : DataBlock
    {
        //UByteN4 * vertex count
    }

    #endregion
}
