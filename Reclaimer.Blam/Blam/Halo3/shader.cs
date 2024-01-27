using Reclaimer.Blam.Common;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class shader
    {
        [Offset(0)]
        public TagReference RenderMethodDefinitionReference { get; set; }

        [Offset(16)]
        public BlockCollection<ShaderOptionIndexBlock> ShaderOptions { get; set; }

        [Offset(40)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(2)]
    public class ShaderOptionIndexBlock
    {
        [Offset(0)]
        public short OptionIndex { get; set; }
    }

    [FixedSize(132)]
    public class ShaderPropertiesBlock
    {
        [Offset(0)]
        public TagReference TemplateReference { get; set; }

        [Offset(16)]
        public BlockCollection<ShaderMapBlock> ShaderMaps { get; set; }

        [Offset(28)]
        public BlockCollection<RealVector4> TilingData { get; set; }
    }

    [FixedSize(24, MaxVersion = (int)CacheType.MccHalo3U4)]
    [FixedSize(28, MinVersion = (int)CacheType.MccHalo3U4, MaxVersion = (int)CacheType.Halo3ODST)]
    [FixedSize(24, MinVersion = (int)CacheType.Halo3ODST)]
    [DebuggerDisplay($"{{{nameof(BitmapReference)},nq}}")]
    public class ShaderMapBlock
    {
        [Offset(0)]
        public TagReference BitmapReference { get; set; }

        [Offset(21, MaxVersion = (int)CacheType.MccHalo3U4)]
        [Offset(22, MinVersion = (int)CacheType.MccHalo3U4, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(21, MinVersion = (int)CacheType.Halo3ODST)]
        public byte TilingIndex { get; set; }
    }
}
