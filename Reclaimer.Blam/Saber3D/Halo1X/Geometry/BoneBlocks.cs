using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DataBlock(0xE802)]
    public class BoneListBlock : CollectionDataBlock
    {
        public int BoneCount { get; set; }
        public List<BoneBlock> Bones { get; } = new List<BoneBlock>();

        internal override void Read(EndianReader reader)
        {
            BoneCount = reader.ReadInt32();
            ReadChildren(reader, BoneCount * 2); //empty block after every bone
            PopulateChildrenOfType(Bones);
        }
    }

    [DataBlock(0xE902)]
    public class BoneBlock : CollectionDataBlock
    {
        public float Unknown { get; set; }
        public int UnknownAsInt { get; set; }

        public RealVector3 Position => GetUniqueChild<PositionBlock>().Value;
        public RealVector4 Rotation => GetUniqueChild<RotationBlock>().Value;
        public RealVector3 UnknownVector0xFC02 => GetUniqueChild<VectorBlock0xFC02>().Value;
        public float Scale => GetUniqueChild<ScaleBlock0x0A03>().Value;

        public int Index => Owner.Bones.IndexOf(this);

        public int ParentIndex => GetNodeBlock().ParentNode?.BoneIndex ?? -1;

        public string Name => GetNodeBlock()?.MeshName;

        public NodeGraphBlock0xF000 GetNodeBlock()
        {
            var index = Index;
            return Owner.NodeGraph.AllDescendants.FirstOrDefault(c => c.BoneIndex == index);
        }

        internal override void Read(EndianReader reader)
        {
            UnknownAsInt = reader.PeekInt32();
            Unknown = reader.ReadSingle();

            ReadChildren(reader);
        }
    }

    [DataBlock(0xFA02, ExpectedSize = 4 * 3)]
    public class PositionBlock : DataBlock
    {
        [Offset(0)]
        public RealVector3 Value { get; set; }
    }

    [DataBlock(0xFB02, ExpectedSize = 4 * 4)]
    public class RotationBlock : DataBlock
    {
        [Offset(0)]
        public RealVector4 Value { get; set; }
    }

    [DataBlock(0xFC02, ExpectedSize = 4 * 3)]
    public class VectorBlock0xFC02 : DataBlock
    {
        [Offset(0)]
        public RealVector3 Value { get; set; } //usually 1,1,1 (maybe actually scale?)
    }

    [DataBlock(0x0A03, ExpectedSize = 4)]
    public class ScaleBlock0x0A03 : DataBlock
    {
        [Offset(0)]
        public float Value { get; set; } //usually 1
    }
}
