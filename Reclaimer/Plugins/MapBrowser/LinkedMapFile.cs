using Reclaimer.Blam.Common;
using System.Diagnostics;
using System.IO;

namespace Reclaimer.Plugins.MapBrowser
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class LinkedMapFile
    {
        public string FilePath { get; set; }
        public BlamEngine Engine { get; set; }
        public CachePlatform Platform { get; set; }
        public CacheType CacheType { get; set; }
        public CacheMetadataFlags Flags { get; set; }
        public bool FromSteam { get; set; }
        public bool FromWorkshop { get; set; }
        public string CustomGroup { get; set; }
        public string CustomSection { get; set; }
        public string CustomName { get; set; }
        public string Thumbnail { get; set; }

        public string GetDisplayGroupName()
        {
            var result = CustomGroup;
            var subtitle = string.IsNullOrEmpty(CustomSection) ? "Other" : CustomSection;
            result += " [" + char.ToUpper(subtitle[0]) + subtitle[1..] + "]";
            return result;
        }

        public LinkedMapFile()
        { }

        public LinkedMapFile(CacheMetadata cacheMetadata)
        {
            FilePath = cacheMetadata.FileName;
            Engine = cacheMetadata.Engine;
            Platform = cacheMetadata.Platform;
            CacheType = cacheMetadata.CacheType;
            Flags = cacheMetadata.Flags;
        }

        private string GetDebuggerDisplay() => $"[{CacheType}] {Path.GetFileName(FilePath)}";
    }
}
