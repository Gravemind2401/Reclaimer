using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class RangeBlock : DataBlock
    {
        [Offset(0)]
        public int Offset { get; set; }

        [Offset(4)]
        public int Count { get; set; }

        protected override object GetDebugProperties() => new { Offset, Count };
    }
}
