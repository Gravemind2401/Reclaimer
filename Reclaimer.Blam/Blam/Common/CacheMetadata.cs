using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common
{
    public class CacheMetadata
    {
        private const CacheMetadataFlags PreRelease = CacheMetadataFlags.PreBeta | CacheMetadataFlags.Beta | CacheMetadataFlags.Flight;

        public string FileName { get; }
        public BlamEngine Engine { get; }
        public CacheType CacheType { get; }
        public CacheGeneration Generation { get; }
        public CachePlatform Platform { get; }
        public PlatformArchitecture Architecture { get; }
        internal string StringIds { get; }
        public CacheResourceCodec ResourceCodec { get; }
        public CacheMetadataFlags Flags { get; }

        public bool IsPreRelease => (Flags & PreRelease) > 0;
        public bool IsMcc => (Flags & CacheMetadataFlags.Mcc) > 0;

        private CacheMetadata(string fileName, CacheType cacheType, CacheMetadataAttribute meta, BuildStringAttribute build)
        {
            FileName = fileName;
            CacheType = cacheType;
            Engine = meta.Engine;
            Generation = meta.Generation;
            Platform = meta.Platform;
            Architecture = Platform == CachePlatform.Xbox360 ? PlatformArchitecture.PowerPC : PlatformArchitecture.x86;
            StringIds = build.StringIds;
            ResourceCodec = build.ResourceCodecOverride ?? meta.ResourceCodec;
            Flags = build.FlagsOverride ?? meta.Flags;
        }

        public static CacheMetadata FromFile(string fileName) => CacheArgs.FromFile(fileName).Metadata;

        public static CacheMetadata FromBuildString(string buildString, string fileName)
        {
            var result = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                .FirstOrNull(t => t.Attribute.BuildString == buildString);

            //ensure build string still looks correct before trying to fall back
            if (!result.HasValue && DateTime.TryParse(buildString, out _))
            {
                var fallBack = GuessCacheType(fileName);

                if (fallBack != CacheType.Unknown)
                    System.Diagnostics.Debug.WriteLine($"Warning: Falling back to CacheType {fallBack} based on file path");

                result = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                    .FirstOrNull(t => t.EnumValue == fallBack);
            }

            if (!result.HasValue)
                return null;

            var (cacheType, buildAttr) = result.Value;
            System.Diagnostics.Debug.WriteLine($"Resolved CacheType {cacheType}");

            var metaAttr = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(cacheType).Single();
            return new CacheMetadata(fileName, cacheType, metaAttr, buildAttr);
        }

        public static CacheMetadata FromCacheType(string fileName, CacheType cacheType, string buildString)
        {
            var buildAttr = Utils.GetEnumAttributes<CacheType, BuildStringAttribute>()
                .FirstOrNull(t => t.EnumValue == cacheType && t.Attribute.BuildString == buildString)?.Attribute;

            if (buildAttr == null)
                return null;

            System.Diagnostics.Debug.WriteLine($"Resolved CacheType {cacheType}");

            var metaAttr = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(cacheType).Single();
            return new CacheMetadata(fileName, cacheType, metaAttr, buildAttr);
        }

        private static CacheType GuessCacheType(string fileName)
        {
            var parent = System.IO.Directory.GetParent(fileName);
            if (parent.Name != "maps")
                return CacheType.Unknown;

            var game = parent.Parent.Name switch
            {
                "halo1" => BlamEngine.Halo1,
                "halo2" => BlamEngine.Halo2,
                "halo3" => BlamEngine.Halo3,
                "halo3odst" => BlamEngine.Halo3ODST,
                "haloreach" => BlamEngine.HaloReach,
                "halo4" => BlamEngine.Halo4,
                "groundhog" => BlamEngine.Halo2X,
                _ => BlamEngine.Unknown
            };

            if (game == BlamEngine.Unknown)
                return CacheType.Unknown;

            //find highest CacheType for the matching HaloGame
            var maxEnum = Enum.GetValues<CacheType>()
                .Where(e => e != CacheType.Unknown)
                .Select(e => new
                {
                    CacheType = e,
                    Meta = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(e).Single()
                }).Where(x => x.Meta.Engine == game)
                .OrderBy(x => x.CacheType)
                .LastOrDefault();

            return maxEnum?.CacheType ?? CacheType.Unknown;
        }
    }
}
