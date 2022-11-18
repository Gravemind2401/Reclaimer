using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class Int16Block : DataBlock
    {
        internal override int ExpectedSize => sizeof(short);

        [Offset(0)]
        public virtual short Value { get; set; }

        protected override object GetDebugProperties() => new { Value };
    }
}
