using Reclaimer.Blam.Common;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;

namespace Reclaimer.Plugins.MapBrowser
{
    public class MapLibraryModel : BindableBase
    {
        public ObservableCollection<MapGroupDisplayModel> MapGroups { get; set; }

        private MapGroupDisplayModel selectedGroup;
        public MapGroupDisplayModel SelectedGroup
        {
            get => selectedGroup;
            set => SetProperty(ref selectedGroup, value);
        }

        public MapLibraryModel()
        { }

        //this could be in the constructor, but the constructor is left empty so the xaml DesignInstance doesnt need to work too hard
        public static MapLibraryModel Build()
        {
            var allMaps = MapScanner.GetLinkedMaps();
            if (allMaps == null)
            {
                return new MapLibraryModel
                {
                    MapGroups = new()
                };
            }

            var templateData = MapInfoTemplate.EnumerateTemplates().ToLookup(m => m.Engine);
            var steamMaps = allMaps.TryGetValue("steam", out var maps) ? maps : Enumerable.Empty<LinkedMapFile>();
            var steamPaths = new HashSet<string>(steamMaps.Select(m => m.FilePath), StringComparer.OrdinalIgnoreCase);

            var mccGroups = steamMaps
                .Where(m => !m.FromWorkshop)
                .GroupBy(m => m.Engine)
                .Select(g => new MapGroupDisplayModel
                {
                    SortOrder = 0,
                    ParentGroup = "MCC (Steam)",
                    GroupName = g.Key.GetEnumDisplay().Name,
                    Engine = g.Key,
                    Platform = CachePlatform.PC,
                    Maps = ProcessTemplatedMaps(g, g.Key)
                });

            var modGroups = steamMaps
                .Where(m => m.FromWorkshop)
                .GroupBy(m => m.Engine)
                .Select(g => new MapGroupDisplayModel
                {
                    SortOrder = 1,
                    ParentGroup = "Steam Workshop",
                    GroupName = g.Key.GetEnumDisplay().Name,
                    Engine = g.Key,
                    Platform = CachePlatform.PC,
                    Maps = ProcessWorkshopMaps(g)
                });

            var customGroups = allMaps.Where(kv => kv.Key != "steam")
                .SelectMany(kv => kv.Value)
                .Where(m => !steamPaths.Contains(m.FilePath))
                .DistinctBy(m => m.FilePath)
                .GroupBy(m => (m.Engine, m.Platform, m.Flags, IsCustomEdition: m.CacheType == CacheType.Halo1CE))
                .Select(g => new MapGroupDisplayModel
                {
                    SortOrder = 2,
                    ParentGroup = g.Key.Platform.ToString(),
                    GroupName = MakeGroupName(g.Key.Engine, g.Key.Platform, g.Key.Flags, g.Key.IsCustomEdition),
                    Engine = g.Key.Engine,
                    Platform = g.Key.Platform,
                    Maps = ProcessTemplatedMaps(g, g.Key.Engine)
                });

            var allGroups = mccGroups
                .Concat(modGroups)
                .Concat(customGroups)
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.ParentGroup)
                .ThenBy(g => g.Engine)
                .ThenBy(g => g.Maps.Min(m => m.CacheType))
                .ThenBy(g => g.GroupName)
                .ToList();

            return new MapLibraryModel
            {
                MapGroups = new(allGroups),
                SelectedGroup = allGroups.FirstOrDefault()
            };

            List<MapFileDisplayModel> ProcessTemplatedMaps(IEnumerable<LinkedMapFile> maps, BlamEngine engine)
            {
                var mapList = new List<MapFileDisplayModel>();

                foreach (var map in maps)
                {
                    var fileName = Path.GetFileName(map.FilePath);
                    var templates = templateData[engine].Where(x => string.Equals(x.FileName, fileName, StringComparison.OrdinalIgnoreCase));

                    if (!templates.Any())
                    {
                        mapList.Add(new MapFileDisplayModel
                        {
                            FilePath = map.FilePath,
                            CacheType = map.CacheType,
                            Flags = map.Flags,
                            DisplayName = fileName,
                            GroupName = "Other"
                        });
                        continue;
                    }

                    foreach (var template in templates)
                    {
                        mapList.Add(new MapFileDisplayModel
                        {
                            FilePath = map.FilePath,
                            CacheType = map.CacheType,
                            Flags = map.Flags,
                            GroupName = template.GroupName,
                            DisplayName = template.DisplayName,
                            SortOrder = template.SortOrder,
                            Thumbnail = template.MakeThumbnailPath(map)
                        });
                    }
                }

                return mapList.OrderBy(m => MapInfoTemplate.GroupSortOrder.FirstIndexWhere(s => m.GroupName.StartsWith(s)))
                    .ThenBy(m => m.GroupName)
                    .ThenBy(m => m.SortOrder)
                    .ThenBy(m => m.FileName)
                    .ToList();
            }

            List<MapFileDisplayModel> ProcessWorkshopMaps(IEnumerable<LinkedMapFile> mapGroup)
            {
                return mapGroup.Select(m => new MapFileDisplayModel
                {
                    FilePath = m.FilePath,
                    CacheType = m.CacheType,
                    Flags = m.Flags,
                    GroupName = m.GetDisplayGroupName(),
                    DisplayName = m.CustomName,
                    Thumbnail = m.Thumbnail
                }).OrderBy(m => m.GroupName)
                .ThenBy(m => m.FileName)
                .ToList();
            }

            static string MakeGroupName(BlamEngine engine, CachePlatform platform, CacheMetadataFlags flags, bool isCustomEdition)
            {
                var name = engine.GetEnumDisplay().Name;
                if (isCustomEdition)
                    return $"{name} (Custom Edition)";

                if (engine == BlamEngine.Halo2 && platform == CachePlatform.PC && !flags.HasFlag(CacheMetadataFlags.Mcc))
                    return $"{name} Vista";

                if (flags == default)
                    return name;

                var suffix = flags.ToString().Replace(" |", ", ");
                return $"{name} ({suffix})";
            }
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class MapGroupDisplayModel
    {
        public int SortOrder { get; set; }
        public string ParentGroup { get; set; }
        public string GroupName { get; set; }
        public BlamEngine Engine { get; set; }
        public CachePlatform Platform { get; set; }

        public List<MapFileDisplayModel> Maps { get; set; }

        private string GetDebuggerDisplay() => $"[{Engine}] {GroupName} ({Maps?.Count ?? 0})";
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class MapFileDisplayModel
    {
        public string FilePath { get; set; }
        public string GroupName { get; set; }
        public string DisplayName { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public CacheType CacheType { get; set; }
        public CacheMetadataFlags Flags { get; set; }
        public int SortOrder { get; set; }
        public string Thumbnail { get; set; }

        private string GetDebuggerDisplay() => FileName;
    }
}
