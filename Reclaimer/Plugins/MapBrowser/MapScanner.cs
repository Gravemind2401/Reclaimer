using Newtonsoft.Json;
using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Buffers;
using System.IO;
using System.Text.RegularExpressions;

namespace Reclaimer.Plugins.MapBrowser
{
    internal static class MapScanner
    {
        private const int SteamAppId = 976730;

        public static string PluginFilesDirectory => Path.Combine(Substrate.PluginsDirectory, "MapBrowser");
        public static string ThumbnailCacheDirectory => Path.Combine(PluginFilesDirectory, "ThumbnailCache");
        public static string MapsJsonPath => Path.Combine(PluginFilesDirectory, "LinkedMaps.json");

        public static Dictionary<string, List<LinkedMapFile>> GetLinkedMaps()
        {
            try
            {
                Dictionary<string, List<LinkedMapFile>> allMaps = null;

                if (File.Exists(MapsJsonPath))
                    allMaps = JsonConvert.DeserializeObject<Dictionary<string, List<LinkedMapFile>>>(File.ReadAllText(MapsJsonPath));

                if (allMaps == null || allMaps.Count == 0)
                    allMaps = ScanForMaps();

                return allMaps;
            }
            catch (Exception ex)
            {
                MapBrowserPlugin.Instance.LogError("Error loading linked maps list", ex);
                return null;
            }
        }

        public static Dictionary<string, List<LinkedMapFile>> ScanForMaps()
        {
            var allMaps = new Dictionary<string, List<LinkedMapFile>>();

            try
            {
                var steamDir = GetActualSteamFolder(MapBrowserPlugin.Settings.SteamLibraryFolder);
                if (Directory.Exists(steamDir))
                {
                    try
                    {
                        var maps = ScanSteamFolder(steamDir).ToList();
                        if (maps.Count > 0)
                            allMaps["steam"] = maps;
                    }
                    catch (Exception ex)
                    {
                        MapBrowserPlugin.Instance.LogError("Error loading Steam library", ex);
                    }
                }

                var customDirs = MapBrowserPlugin.Settings.CustomFolders?.Select(x => x.Directory) ?? Enumerable.Empty<string>();
                foreach (var dir in customDirs.Distinct().Where(Directory.Exists))
                    allMaps[dir] = ScanCustomDirectory(dir).ToList();

                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    Converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };

                Directory.CreateDirectory(PluginFilesDirectory);
                File.WriteAllText(MapsJsonPath, JsonConvert.SerializeObject(allMaps, jsonSettings));
            }
            catch (Exception ex)
            {
                MapBrowserPlugin.Instance.LogError("Error scanning for map files", ex);
            }

            return allMaps;
        }

        #region Steam
        private static string GetActualSteamFolder(string path)
        {
            //in case the user selected the MCC install folder instead of the steam library folder

            if (Directory.Exists(Path.Combine(path, "steamapps")))
                return path; //path is already correct

            var parts = path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
            var index = parts.LastIndexWhere(s => s.Equals("steamapps", StringComparison.OrdinalIgnoreCase));
            if (index > 0)
            {
                for (var i = parts.Length; i > index; i--)
                    path = Path.GetDirectoryName(path);
            }

            return path;
        }

        public static IEnumerable<LinkedMapFile> ScanSteamFolder(string steamLibraryPath)
        {
            if (!Directory.Exists(Path.Combine(steamLibraryPath, "steamapps")))
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
                }).Concat(ScanWorkshopFolder(modsDir));
        }

        public static void DumpMccThumbnails(string mccInstallDir)
        {
            var sourceFile = Path.Combine(mccInstallDir, "data", "ui", "texturepacks", "ingamechapterpack.perm.bin");
            if (!File.Exists(sourceFile))
                return;

            var outputDir = Path.Combine(ThumbnailCacheDirectory, "MCC");
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
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            var jsonString = acfString;
            jsonString = Regex.Replace(jsonString, @"""(?=[\r\n]+\s+"")", @""","); //add commas after string properties
            jsonString = Regex.Replace(jsonString, @"""(?=[\s\r\n]+[""\{])", @""":"); //add semicolon between property and value
            jsonString = Regex.Replace(jsonString, @"\}(?=[\s\r\n]+[""\{])", @"},"); //add commas after object properties
            jsonString = "{" + Environment.NewLine + jsonString + Environment.NewLine + "}";

            return jsonString;
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        }

        private static IEnumerable<LinkedMapFile> ScanWorkshopFolder(string workshopDirectory)
        {
            foreach (var modFolder in new DirectoryInfo(workshopDirectory).GetDirectories())
            {
                var modInfoFile = new FileInfo(Path.Combine(modFolder.FullName, "ModInfo.json"));
                var mapFiles = DiscoverMaps(modFolder.FullName).ToList();
                var infoFiles = modFolder.GetFiles("*.json", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

                var title = modFolder.Name;
                if (modInfoFile.Exists)
                {
                    try
                    {
                        var modInfo = (dynamic)JsonConvert.DeserializeObject(File.ReadAllText(modInfoFile.FullName));
                        title = (string)modInfo.Title.Neutral;
                    }
                    catch { }
                }

                foreach (var mapFile in mapFiles)
                {
                    var result = new LinkedMapFile(mapFile)
                    {
                        FromSteam = true,
                        FromWorkshop = true,
                        CustomGroup = title,
                        CustomName = Path.GetFileNameWithoutExtension(mapFile.FileName)
                    };

                    var mapInfoFile = infoFiles.FirstOrDefault(f => f.Name == Path.ChangeExtension(Path.GetFileName(mapFile.FileName), ".json"));
                    if (mapInfoFile != null)
                    {
                        try
                        {
                            result.CustomSection = mapInfoFile.Directory.Name;
                            var mapInfo = (dynamic)JsonConvert.DeserializeObject(File.ReadAllText(mapInfoFile.FullName));
                            result.CustomName = (string)mapInfo.Title.Neutral;
                            result.Thumbnail = Path.Combine(modFolder.FullName, (string)mapInfo.Images.Thumbnail);
                        }
                        catch { }
                    }

                    yield return result;
                }
            }
        }
        #endregion

        public static IEnumerable<LinkedMapFile> ScanCustomDirectory(string directory)
        {
            foreach (var cacheInfo in DiscoverMaps(directory))
            {
                DumpBlfThumbnail(cacheInfo);
                yield return new LinkedMapFile(cacheInfo);
            }
        }

        private static void DumpBlfThumbnail(CacheMetadata cacheInfo)
        {
            var fileInfo = new FileInfo(cacheInfo.FileName);
            var imagesDir = new DirectoryInfo(Path.Combine(fileInfo.Directory.FullName, "images"));
            if (!imagesDir.Exists)
                return;

            var outputDir = Path.Combine(ThumbnailCacheDirectory, cacheInfo.Engine.ToString());

            var mapName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            if (cacheInfo.Engine == BlamEngine.Halo3)
            {
                if (int.TryParse(mapName[0..3], out var cnum))
                    DumpBlf($"c_{cnum:D3}_sm.blf");
                else
                {
                    //just try both
                    DumpBlf($"m_{mapName}_sm.blf");
                    DumpBlf($"dlc_{mapName}_sm.blf");
                }
            }
            else if (cacheInfo.Engine == BlamEngine.Halo3ODST)
            {
                DumpBlf($"c_{mapName}_sm.blf");
            }
            else if (cacheInfo.Engine == BlamEngine.Halo4)
            {
                if (int.TryParse(mapName[1..4].Trim('_'), out var cnum))
                    DumpBlf($"c_{cnum:D3}_sm.blf");
            }

            void DumpBlf(string name)
            {
                var blfInfo = new FileInfo(Path.Combine(imagesDir.FullName, name));
                var outputFile = new FileInfo(Path.Combine(outputDir, $"{mapName}.png"));
                if (outputFile.Exists || !blfInfo.Exists)
                    return;

                const int dataOffset = 68;
                var data = ArrayPool<byte>.Shared.Rent((int)blfInfo.Length - dataOffset);

                try
                {
                    Directory.CreateDirectory(outputDir);
                    using (var fs = new FileStream(blfInfo.FullName, FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(dataOffset, SeekOrigin.Begin);
                        fs.ReadAll(data);

                        using (var ms = new MemoryStream(data))
                        using (var image = System.Drawing.Image.FromStream(ms))
                        using (var bitmap = new System.Drawing.Bitmap(image))
                            bitmap.Save(outputFile.FullName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch { }
                finally
                {
                    ArrayPool<byte>.Shared.Return(data);
                }
            }
        }

        private static IEnumerable<CacheMetadata> DiscoverMaps(string directory)
        {
            var mapFiles = new DirectoryInfo(directory).EnumerateFiles("*.map", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 });
            foreach (var mapFile in mapFiles)
            {
                if (mapFile.Name.ToLower() is "cheape.map" or "bitmaps.map" or "sounds.map" or "loc.map" or "campaign.map" or "shared.map" or "single_player_shared.map")
                    continue;

                var meta = default(CacheMetadata);

                try
                {
                    meta = CacheMetadata.FromFile(mapFile.FullName);
                }
                catch { }

                if (meta != default)
                    yield return meta;
            }
        }
    }
}
