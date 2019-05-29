using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adjutant.Utilities;
using Reclaimer.Utils;

namespace Reclaimer.Entities
{
    public partial class CacheFile : ICacheFile
    {
        CacheType ICacheFile.CacheType => CacheType;

        IStringIndex ICacheFile.StringIndex => StringIndex;

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;

        private IAddressTranslator tagAddressTranslator;
        internal IAddressTranslator TagAddressTranslator
        {
            get
            {
                if (tagAddressTranslator == null)
                    tagAddressTranslator = new StandardAddressTranslator(TagIndex.Magic);

                return tagAddressTranslator;
            }
        }

        public DependencyReader CreateReader(IAddressTranslator translator)
        {
            throw new NotImplementedException();
        }
    }
}
