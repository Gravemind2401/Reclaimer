using Adjutant.Spatial;
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
    [DataBlock(0xE402)]
    public class TemplateBlock : CollectionDataBlock
    {

    }

    [DataBlock(0xE502)] //model name
    public class StringBlock0xE502 : StringBlock
    {
        internal override int ExpectedSize => Value.Length + 2;

        public byte Unknown { get; set; }

        internal override void Read(EndianReader reader)
        {
            Value = reader.ReadNullTerminatedString();
            Unknown = reader.ReadByte();
        }

        internal override void Validate()
        {
            if (Unknown != 0)
                Debugger.Break();
        }
    }

    [DataBlock(0x1603, ExpectedSize = 3)]
    public class UnknownBlock0x1603 : DataBlock
    {
        [Offset(0)]
        public byte Unknown0 { get; set; }

        [Offset(1)]
        public byte Unknown1 { get; set; }

        [Offset(2)]
        public byte Unknown2 { get; set; }

        internal override void Validate()
        {
            if ((Unknown0, Unknown1, Unknown2) != (2, 1, 1) || Header.BlockSize > 3)
                Debugger.Break();
        }
    }

    #region Material List

    [DataBlock(0x5501)]
    public class MaterialListBlock : CollectionDataBlock
    {
        public int MaterialCount { get; set; }
        public List<MaterialReferenceBlock> Materials { get; } = new List<MaterialReferenceBlock>();

        internal override void Read(EndianReader reader)
        {
            MaterialCount = reader.ReadInt32();
            ReadChildren(reader, MaterialCount);
            PopulateChildrenOfType(Materials);
        }
    }

    [DataBlock(0x5601)]
    public class MaterialReferenceBlock : StringBlock
    {
        public int Index => Owner.Materials.IndexOf(this);

        protected override object GetDebugProperties() => new { Index, Value };
    }

    #endregion

    //0xF000 (NodeBlocks.cs)

    //0xE802 (BoneBlocks.cs)

    [DataBlock(0xE602)]
    public class UnknownBlock0xE602 : Int32Block
    {
        internal override void Validate()
        {
            if (Value != 0)
                Debugger.Break();
        }
    }

    [DataBlock(0x1D02)]
    public class UnknownBlock0x1D02 : Int32Block
    {
        internal override void Validate()
        {
            if (Value != 0)
                Debugger.Break();
        }
    }

    [DataBlock(0x0403)]
    public class StringBlock0x0403 : StringBlock
    {

    }

    #region Transform List

    [DataBlock(0x0503, ExpectedChildCount = 2)] //MatrixList + EmptyBlock
    public class TransformBlock0x0503 : CollectionDataBlock
    {
        public MatrixListBlock0x0D03 MatrixList => GetUniqueChild<MatrixListBlock0x0D03>();
    }

    [DataBlock(0x0D03)]
    public class MatrixListBlock0x0D03 : DataBlock
    {
        public int MatrixCount { get; set; }
        public short Unknown0 { get; set; } //always 3?
        public byte Unknown1 { get; set; } //always 0, 2 or 3?

        public List<Matrix4x4> Matrices { get; } = new List<Matrix4x4>();

        internal override void Read(EndianReader reader)
        {
            MatrixCount = reader.ReadInt32();
            Unknown0 = reader.ReadInt16();
            Unknown1 = reader.ReadByte();

            //3/3 = no matrix data here?
            if (Header.BlockSize == 7 && MatrixCount > 0)
            {
                if ((Unknown0, Unknown1) != (3, 3))
                    Debugger.Break();
                return;
            }

            while (Matrices.Count < MatrixCount)
                Matrices.Add(reader.ReadMatrix4x4());

            EndRead(reader.Position);
        }

        protected override object GetDebugProperties() => new { MatrixCount, Unknown0, Unknown1 };
    }

    #endregion

    [DataBlock(0x0803, ExpectedSize = 4 + 4 * 3 * 2)]
    public class BoundsBlock0x0803 : DataBlock
    {
        [Offset(0)]
        public int Unknown0 { get; set; } //count? always 1?

        [Offset(4)]
        public RealVector3D MinBound { get; set; }

        [Offset(16)]
        public RealVector3D MaxBound { get; set; }

        public bool IsEmpty => MinBound == MaxBound;

        protected override object GetDebugProperties() => new { Header.StartOfBlock, IsEmpty };
    }

    //0x0E03 (unmapped)

    #region Other

    [DataBlock(0x1103)]
    public class UnknownBlock0x1103 : DataBlock
    {
        internal override int ExpectedSize => 4 + 4 * Count;

        public int Count { get; set; }
        public (short, short)[] UnknownArray { get; set; }

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            UnknownArray = new (short, short)[Count];

            for (var i = 0; i < Count; i++)
                UnknownArray[i] = (reader.ReadInt16(), reader.ReadInt16());
        }
    }

    [DataBlock(0x1203)]
    public class StringBlock0x1203 : StringBlock
    {
        internal override int ExpectedSize => 4 + Value.Length;

        [Offset(0), LengthPrefixed]
        public override string Value { get; set; }
    }

    #endregion
}
