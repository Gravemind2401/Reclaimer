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

        public static CacheMetadata FromBuildString(string buildString, string fileName)
        {
            var result = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                .FirstOrNull(p => p.Value.BuildString == buildString);

            if (!result.HasValue)
            {
                var fallBack = GuessCacheType(fileName);

                if (fallBack != CacheType.Unknown)
                    System.Diagnostics.Debug.WriteLine($"Warning: Falling back to CacheType {fallBack} based on file path");

                result = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                    .FirstOrNull(p => p.Key == fallBack);
            }

            if (!result.HasValue)
                return null;

            var cacheType = result.Value.Key;
            System.Diagnostics.Debug.WriteLine($"Resolved CacheType {cacheType}");

            var buildAttr = result.Value.Value;
            var metaAttr = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(cacheType).Single();

            return new CacheMetadata(cacheType, metaAttr, buildAttr);
        }

        private static CacheType GuessCacheType(string fileName)
        {
            var parent = System.IO.Directory.GetParent(fileName);
            if (parent.Name != "maps")
                return CacheType.Unknown;

            //TODO: detect latest enum for each game instead of hardcoding
            switch (parent.Parent.Name)
            {
                case "halo1":
                    return CacheType.MccHalo1;
                case "halo3":
                    return CacheType.MccHalo3U9;
                case "halo3odst":
                    return CacheType.MccHalo3ODSTU4;
                case "haloreach":
                    return CacheType.MccHaloReachU10;
                case "halo4":
                    return CacheType.MccHalo4U4;
                case "groundhog":
                    return CacheType.MccHalo2XU8;
            }

            return CacheType.Unknown;
        }
    }
}
