using Adjutant.IO;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    [FixedSize(8)]
    public struct TagReference
    {
        private readonly CacheFile cache;
        private readonly int tagId;

        public int TagId => tagId;
        public IndexItem Tag => cache.TagIndex[TagId];

        public TagReference(CacheFile cache, DependencyReader reader)
        {
            this.cache = cache;
            tagId = reader.ReadInt16();
        }

        public override string ToString() => Tag.ToString();
    }
}
