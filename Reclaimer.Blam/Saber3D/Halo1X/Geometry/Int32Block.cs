using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class Int32Block : DataBlock
    {
        internal override int ExpectedSize => sizeof(int);

        [Offset(0)]
        public virtual int Value { get; set; }

        protected override object GetDebugProperties() => new { Value };
    }
}
