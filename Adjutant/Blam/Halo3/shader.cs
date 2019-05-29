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

    public class ShaderPropertiesBlock
    {
        [Offset(0)]
        public TagReference TemplateReference { get; set; }

        [Offset(16)]
        public BlockCollection<ShaderMapBlock> ShaderMaps { get; set; }

        [Offset(28)]
        public BlockCollection<RealVector4D> TilingData { get; set; }
    }

    [FixedSize(24)]
    public class ShaderMapBlock
    {
        [Offset(0)]
        public TagReference BitmapReference { get; set; }

        [Offset(20)]
        public short TilingIndex { get; set; }
    }
}
