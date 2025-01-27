using Newtonsoft.Json;
using Reclaimer.Blam.Common;
using System.IO;

namespace Reclaimer.Plugins.MapBrowser
{
    internal class MapInfoTemplate
    {
        internal static readonly string[] GroupSortOrder = new[] { "Campaign", "Multiplayer", "Forge", "Firefight", "SpartanOpsLocations", "SpartanOpsMissions", "Other" };

        public string DisplayName { get; set; }
        public string FileName { get; set; }
        public string[] MccThumbnails { get; set; }

        public BlamEngine Engine { get; set; }
        public string GroupName { get; set; }
        public int SortOrder { get; set; }

        public string MakeThumbnailPath()
        {
            var thumbnail = MccThumbnails?.LastOrDefault(s => !string.IsNullOrWhiteSpace(s));
            return thumbnail == null
                ? null
                : Path.Combine(MapScanner.ThumbnailCacheDirectory, $"{thumbnail}.png");
        }

        public static IEnumerable<MapInfoTemplate> EnumerateTemplates()
        {
            var templateJson = Properties.Resources.MapBrowserTemplates;
            var templateData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, MapInfoTemplate[]>>>(templateJson);

            foreach (var (engineString, engineGroups) in templateData)
            {
                var engine = Enum.Parse<BlamEngine>(engineString, true);

                foreach (var (groupName, mapList) in engineGroups)
                {
                    foreach (var (mapInfo, index) in mapList.Select((m, i) => (m, i)))
                    {
                        mapInfo.Engine = engine;
                        mapInfo.GroupName = groupName;

                        if (groupName == "Campaign")
                            mapInfo.SortOrder = index;

                        yield return mapInfo;
                    }
                }
            }
        }
    }
}
