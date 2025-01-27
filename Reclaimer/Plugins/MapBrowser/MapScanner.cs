using Newtonsoft.Json;
using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System.IO;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MapBrowser
{
    internal static class MapScanner
    {
        private const int SteamAppId = 976730;

        public static string ThumbnailCacheDirectory => Path.Combine(Substrate.PluginsDirectory, "MapBrowser", "ThumbnailCache");

        public static Dictionary<string, List<LinkedMapFile>> ReScan()
        {
            var allMaps = new Dictionary<string, List<LinkedMapFile>>();

            if (Directory.Exists(MapBrowserPlugin.Settings.SteamLibraryFolder))
            {
                var maps = ScanSteamFolder(MapBrowserPlugin.Settings.SteamLibraryFolder).ToList();
                if (maps.Count > 0)
                    allMaps["mcc"] = maps;
            }

            return allMaps;
        }

        public static IEnumerable<LinkedMapFile> ScanSteamFolder(string steamLibraryPath)
        {
            if (!File.Exists(Path.Combine(steamLibraryPath, "libraryfolder.vdf")))
                throw new ArgumentException("Path is not a Steam library folder", nameof(steamLibraryPath));

            var manifestPath = Path.Combine(steamLibraryPath, "steamapps", $"appmanifest_{SteamAppId}.acf");
            if (!File.Exists(manifestPath))
                throw new ArgumentException("MCC is not installed in this library folder", nameof(steamLibraryPath));

            var manifestData = (dynamic)JsonConvert.DeserializeObject(AcfToJson(File.ReadAllText(manifestPath)));
            var installDir = (string)manifestData.AppState.installdir;

            installDir = Path.Combine(steamLibraryPath, "steamapps", "common", installDir);
            var modsDir = Path.Combine(steamLibraryPath, "steamapps", "workshop", "content", SteamAppId.ToString());

            DumpMccThumbnails(installDir);

            return DiscoverMaps(installDir)
                .Select(m => new LinkedMapFile(m)
                {
                    FromSteam = true
                });
        }

        public static void DumpMccThumbnails(string mccInstallDir)
        {
            var sourceFile = Path.Combine(mccInstallDir, "data", "ui", "texturepacks", "ingamechapterpack.perm.bin");
            if (!File.Exists(sourceFile))
                return;

            var outputDir = ThumbnailCacheDirectory;
            Directory.CreateDirectory(outputDir);

            var segments = new List<(int Address, int Size)>();

            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using var reader = new EndianReader(fs);

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                reader.ReadInt32();
                reader.ReadInt32();
                var size = reader.ReadInt32();
                reader.ReadInt32();

                segments.Add(((int)fs.Position, size));

                reader.Seek(size, SeekOrigin.Current);
            }

            var sizeLookup = new Dictionary<int, (int Width, int Height)>
            {
                [25280] = (316, 158),
                [97528] = (668, 292)
            };

            var dataTemp = new byte[sizeLookup.Keys.Max()];

            for (var i = 0; i < segments.Count; i++)
            {
                var (dataAddress, dataLength) = segments[i];
                if (!sizeLookup.TryGetValue(dataLength, out var dimensions))
                    continue;

                var (nameAddress, _) = segments[i + 3];
                reader.Seek(nameAddress + 52, SeekOrigin.Begin);
                var name = reader.ReadNullTerminatedString();
                var outputPath = Path.Combine(outputDir, $"{name}.png");

                if (File.Exists(outputPath))
                    continue;

                fs.Seek(dataAddress, SeekOrigin.Begin);
                fs.ReadExactly(dataTemp, 0, dataLength);

                new Reclaimer.Drawing.DdsImage(dimensions.Height, dimensions.Width, Drawing.DxgiFormat.BC1_UNorm, dataTemp).WriteToDisk(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static string AcfToJson(string acfString)
        {
            var jsonString = acfString;
            jsonString = Regex.Replace(jsonString, @"""(?=[\r\n]+\s+"")", @""","); //add commas after string properties
            jsonString = Regex.Replace(jsonString, @"""(?=[\s\r\n]+[""\{])", @""":"); //add semicolon between property and value
            jsonString = Regex.Replace(jsonString, @"\}(?=[\s\r\n]+[""\{])", @"},"); //add commas after object properties
            jsonString = "{" + Environment.NewLine + jsonString + Environment.NewLine + "}";

            return jsonString;
        }

        private static IEnumerable<CacheMetadata> DiscoverMaps(string directory)
        {
            var mapFiles = new DirectoryInfo(directory).EnumerateFiles("*.map", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 });
            foreach (var mapFile in mapFiles)
            {
                if (mapFile.Name is "bitmaps.map" or "sounds.map" or "loc.map" or "campaign.map" or "shared.map" or "single_player_shared.map")
                    continue;

                var meta = default(CacheMetadata);

                try
                {
                    meta = CacheMetadata.FromFile(mapFile.FullName);
                }
                catch { }

                if (meta != null)
                    yield return meta;
            }
        }
    }
}
