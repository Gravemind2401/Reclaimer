using Reclaimer.Blam.Common;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    public class shader
    {
        [Offset(20, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(12, MinVersion = (int)CacheType.Halo2Xbox)]
        public BlockCollection<RuntimePropertiesBlock> RuntimeProperties { get; set; }

        //[Offset(36, MaxVersion = (int)CacheType.Halo2Xbox)] //maybe come back to this
        [MinVersion((int)CacheType.Halo2Xbox)]
        [Offset(32, MinVersion = (int)CacheType.Halo2Xbox)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(108, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(80, MinVersion = (int)CacheType.Halo2Xbox)]
    public class RuntimePropertiesBlock
    {
        [Offset(0)]
        public TagReference DiffuseBitmapReference { get; set; }

        [Offset(16, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(8, MinVersion = (int)CacheType.Halo2Xbox)]
        public TagReference IllumBitmapReference { get; set; }

        [Offset(76, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(54, MinVersion = (int)CacheType.Halo2Xbox)]
        public TagReference BitmapReference2 { get; set; }

        public IEnumerable<TagReference> EnumerateBitmapReferences()
        {
            yield return DiffuseBitmapReference;
            yield return IllumBitmapReference;
            yield return BitmapReference2;
        }
    }

    [FixedSize(124)]
    public class ShaderPropertiesBlock
    {
        [Offset(0)]
        public TagReferenceShort TemplateReference { get; set; }

        [Offset(4)]
        public BlockCollection<ShaderMapBlock> Bitmaps { get; set; }

        [Offset(20)]
        public BlockCollection<RealVector4> TilingData { get; set; }
    }

    [FixedSize(12)]
    [DebuggerDisplay($"{{{nameof(BitmapReference)},nq}}")]
    public class ShaderMapBlock
    {
        [Offset(0)]
        public TagReferenceShort BitmapReference { get; set; }
    }
}
