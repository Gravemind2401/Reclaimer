using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common
{
    public class CacheMetadata
    {
        private const CacheMetadataFlags PreRelease = CacheMetadataFlags.PreBeta | CacheMetadataFlags.Beta | CacheMetadataFlags.Flight;

        public HaloGame Game { get; }
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
            Game = meta.Game;
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

            //ensure build string still looks correct before trying to fall back
            if (!result.HasValue && DateTime.TryParse(buildString, out _))
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

            var game = parent.Parent.Name switch
            {
                "halo1" => HaloGame.Halo1,
                "halo3" => HaloGame.Halo3,
                "halo3odst" => HaloGame.Halo3ODST,
                "haloreach" => HaloGame.HaloReach,
                "halo4" => HaloGame.Halo4,
                "groundhog" => HaloGame.Halo2X,
                _ => HaloGame.Unknown
            };

            if (game == HaloGame.Unknown)
                return CacheType.Unknown;

            //find highest CacheType for the matching HaloGame
            var maxEnum = Enum.GetValues<CacheType>()
                .Where(e => e != CacheType.Unknown)
                .Select(e => new
                {
                    CacheType = e,
                    Meta = Utils.GetEnumAttributes<CacheType, CacheMetadataAttribute>(e).Single()
                }).Where(x => x.Meta.Game == game)
                .OrderBy(x => x.CacheType)
                .LastOrDefault();

            return maxEnum?.CacheType ?? CacheType.Unknown;
        }
    }
}
