using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class shader
    {
        [Offset(12)]
        public BlockCollection<ShaderMapBlock> ShaderMaps { get; set; }

        [Offset(32)]
        public BlockCollection<ShaderPropertiesBlock> ShaderProperties { get; set; }
    }

    [FixedSize(80)]
    public class ShaderMapBlock
    {
        [Offset(0)]
        public TagReference DiffuseBitmapReference { get; set; }

        [Offset(8)]
        public TagReference IllumBitmapReference { get; set; }

        [Offset(54)]
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
        public TagReference TemplateReference { get; set; }

        [Offset(20)]
        public BlockCollection<TilingInfoBlock> Tilings { get; set; }
    }

    [FixedSize(16)]
    public class TilingInfoBlock
    {
        [Offset(0)]
        public float UTiling { get; set; }

        [Offset(4)]
        public float VTiling { get; set; }

        [Offset(8)]
        public float Unknown0 { get; set; }

        [Offset(12)]
        public float Unknown1 { get; set; }
    }
}
