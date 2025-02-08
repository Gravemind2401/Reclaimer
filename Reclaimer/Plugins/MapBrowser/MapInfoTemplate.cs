using Newtonsoft.Json;
using Reclaimer.Blam.Common;
using System.IO;

namespace Reclaimer.Plugins.MapBrowser
{
    internal class MapInfoTemplate
    {
        internal static readonly string[] GroupSortOrder = ["Campaign", "Multiplayer", "Forge", "Firefight", "Spartan Ops (Locations)", "Spartan Ops (Episode", "Other"];

        public string DisplayName { get; set; }
        public string FileName { get; set; }
        public string[] MccThumbnails { get; set; }

        public BlamEngine Engine { get; set; }
        public string GroupName { get; set; }
        public int SortOrder { get; set; }

        public string MakeThumbnailPath(LinkedMapFile linkedMap)
        {
            string thumbnail;

            if (!linkedMap.FromSteam)
            {
                var mapName = Path.GetFileNameWithoutExtension(linkedMap.FilePath);
                thumbnail = Path.Combine(MapScanner.ThumbnailCacheDirectory, linkedMap.Engine.ToString(), $"{mapName}.png");
                if (File.Exists(thumbnail))
                    return thumbnail;
            }

            thumbnail = MccThumbnails?.LastOrDefault(s => !string.IsNullOrWhiteSpace(s));
            return thumbnail == null
                ? null
                : Path.Combine(MapScanner.ThumbnailCacheDirectory, "MCC", $"{thumbnail}.png");
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

                        if (groupName == "Campaign" || groupName.StartsWith("Spartan Ops (Episode"))
                            mapInfo.SortOrder = index;

                        yield return mapInfo;
                    }
                }
            }
        }
    }
}
