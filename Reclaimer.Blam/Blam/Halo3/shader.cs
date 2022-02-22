using Adjutant.Blam.Common;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class shader
    {
        [Offset(0)]
        public TagReference BaseShaderReference { get; set; }

        [Offset(40)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(132)]
    public class ShaderPropertiesBlock
    {
        [Offset(0)]
        public TagReference TemplateReference { get; set; }

        [Offset(16)]
        public BlockCollection<ShaderMapBlock> ShaderMaps { get; set; }

        [Offset(28)]
        public BlockCollection<RealVector4D> TilingData { get; set; }
    }

    [FixedSize(24, MaxVersion = (int)CacheType.MccHalo3U4)]
    [FixedSize(28, MinVersion = (int)CacheType.MccHalo3U4, MaxVersion = (int)CacheType.Halo3ODST)]
    [FixedSize(24, MinVersion = (int)CacheType.Halo3ODST)]
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
