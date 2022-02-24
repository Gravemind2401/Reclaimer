using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public class CacheMetadata
    {
        private const CacheMetadataFlags PreRelease = CacheMetadataFlags.PreBeta | CacheMetadataFlags.Beta | CacheMetadataFlags.Flight;

        public CacheType CacheType { get; }
        public CacheGeneration Generation { get; }
        public CachePlatform Platform { get; }
        public PlatformArchitecture Architecture { get; }
        internal string StringIds { get; }
        public CacheResourceCodec ResourceCodec { get; }
        public CacheMetadataFlags Flags { get; }

        public bool IsPreRelease => (Flags & PreRelease) > 0;
        public bool IsMcc => (Flags & CacheMetadataFlags.Mcc) > 0;

        private CacheMetadata(CacheType cacheType, CacheMetadataAttribute meta, BuildStringAttribute build)
        {
            CacheType = cacheType;
            Generation = meta.Generation;
            Platform = meta.Platform;
            Architecture = Platform == CachePlatform.Xbox360 ? PlatformArchitecture.PowerPC : PlatformArchitecture.x86;
            StringIds = build.StringIds;
            ResourceCodec = build.ResourceCodecOverride ?? meta.ResourceCodec;
            Flags = build.FlagsOverride ?? meta.Flags;
        }

        public static CacheMetadata FromBuildString(string buildString)
        {
            var result = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                .FirstOrNull(p => p.Value.BuildString == buildString);

            if (!result.HasValue)
                return null;

            var cacheType = result.Value.Key;
            System.Diagnostics.Debug.WriteLine($"Resolved CacheType {cacheType}");

            var buildAttr = result.Value.Value;
            var metaAttr = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(cacheType).Single();

            return new CacheMetadata(cacheType, metaAttr, buildAttr);
        }
    }
}
