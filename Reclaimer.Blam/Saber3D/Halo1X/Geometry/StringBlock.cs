using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class StringBlock : DataBlock
    {
        internal override int ExpectedSize => Value.Length + 1;

        [Offset(0), NullTerminated]
        public virtual string Value { get; set; }

        protected override object GetDebugProperties() => new { Value };
    }
}
