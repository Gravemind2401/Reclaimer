using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public abstract class MetaValue : MetaValueBase
    {
        protected readonly ICacheFile cache;

        public override FieldDefinition FieldDefinition { get; }

        protected MetaValue(XmlNode node, ICacheFile cache, EndianReader reader, long baseAddress)
            : base(node, baseAddress)
        {
            this.cache = cache;
            FieldDefinition = FieldDefinition.GetHalo3Definition(node);
        }
    }
}
