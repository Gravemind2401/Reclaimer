namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class DataBlockAttribute : Attribute
    {
        public int BlockType { get; }
        public int ExpectedSize { get; set; } = -1;
        public int ExpectedChildCount { get; set; } = -1;

        public string TypeString => $"0x{BlockType:X4}";

        public DataBlockAttribute(int blockType)
        {
            if (blockType < 0 || blockType > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(blockType));

            BlockType = blockType;
        }
    }
}
