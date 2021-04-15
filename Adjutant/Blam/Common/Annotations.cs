using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    using static CacheMetadataFlags;
    using static CacheResourceCodec;

    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class CacheMetadataAttribute : Attribute
    {
        public CacheGeneration Generation { get; }
        public CachePlatform Platform { get; }
        public CacheResourceCodec ResourceCodec { get; }
        public CacheMetadataFlags Flags { get; }

        public CacheMetadataAttribute(CacheGeneration generation, CachePlatform platform)
            : this(generation, platform, generation < CacheGeneration.Gen3 ? Uncompressed : Deflate, None)
        { }

        public CacheMetadataAttribute(CacheGeneration generation, CachePlatform platform, CacheMetadataFlags flags)
            : this(generation, platform, generation < CacheGeneration.Gen3 ? Uncompressed : Deflate, flags)
        { }

        public CacheMetadataAttribute(CacheGeneration generation, CachePlatform platform, CacheResourceCodec codec)
             : this(generation, platform, codec, None)
        { }

        public CacheMetadataAttribute(CacheGeneration generation, CachePlatform platform, CacheResourceCodec codec, CacheMetadataFlags flags)
        {
            Generation = generation;
            Platform = platform;
            ResourceCodec = codec;
            Flags = flags;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal sealed class BuildStringAttribute : Attribute
    {
        public string BuildString { get; }
        public string StringIds { get; }
        public CacheResourceCodec? ResourceCodecOverride { get; }
        public CacheMetadataFlags? FlagsOverride { get; }

        public BuildStringAttribute(string buildString)
            : this(buildString, null, null, null)
        { }

        public BuildStringAttribute(string buildString, string stringIds)
            : this(buildString, stringIds, null, null)
        { }

        public BuildStringAttribute(string buildString, CacheMetadataFlags flags)
            : this(buildString, null, null, flags)
        { }

        public BuildStringAttribute(string buildString, string stringIds, CacheResourceCodec codec)
            : this(buildString, stringIds, codec, null)
        { }

        public BuildStringAttribute(string buildString, string stringIds, CacheMetadataFlags flags)
            : this(buildString, stringIds, null, flags)
        { }

        public BuildStringAttribute(string buildString, string stringIds, CacheResourceCodec? codecOverride, CacheMetadataFlags? flagsOverride)
        {
            BuildString = buildString;
            StringIds = stringIds;
            ResourceCodecOverride = codecOverride;
            FlagsOverride = flagsOverride;
        }
    }
}
