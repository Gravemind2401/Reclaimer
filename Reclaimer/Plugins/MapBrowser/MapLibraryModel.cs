using Reclaimer.Blam.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public static MapLibraryModel Build()
        {
            var templateData = MapInfoTemplate.EnumerateTemplates().ToLookup(m => m.Engine);
            var allMaps = MapScanner.ReScan();

            var groups = allMaps["mcc"].GroupBy(m => m.Engine)
                .Select(g => new MapGroupDisplayModel
                {
                    GroupName = g.Key.ToString(),
                    Engine = g.Key,
                    Platform = CachePlatform.PC,
                    Maps = ProcessLinkedMaps(g)
                }).OrderBy(g => g.Engine);

            return new MapLibraryModel
            {
                MapGroups = new(groups),
                SelectedGroup = groups.FirstOrDefault()
            };

            List<MapFileDisplayModel> ProcessLinkedMaps(IGrouping<BlamEngine, LinkedMapFile> mapGroup)
            {
                var mapList = new List<MapFileDisplayModel>();

                foreach (var map in mapGroup)
                {
                    var fileName = Path.GetFileName(map.FilePath);
                    var templates = templateData[mapGroup.Key].Where(x => x.FileName == fileName);

                    if (!templates.Any())
                    {
                        mapList.Add(new MapFileDisplayModel
                        {
                            FilePath = map.FilePath,
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
                            Flags = map.Flags,
                            GroupName = template.GroupName,
                            DisplayName = template.DisplayName,
                            SortOrder = template.SortOrder,
                            Thumbnail = template.MakeThumbnailPath()
                        });
                    }
                }

                return mapList.OrderBy(m => Array.IndexOf(MapInfoTemplate.GroupSortOrder, m.GroupName))
                    .ThenBy(m => m.SortOrder)
                    .ThenBy(m => m.FileName)
                    .ToList();
            }
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class MapGroupDisplayModel
    {
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
        public CacheMetadataFlags Flags { get; set; }
        public int SortOrder { get; set; }
        public string Thumbnail { get; set; }

        private string GetDebuggerDisplay() => FileName;
    }
}
