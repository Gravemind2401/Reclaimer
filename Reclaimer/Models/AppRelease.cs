using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reclaimer.Models
{
    public class AppRelease
    {
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public string Name { get; set; }
        public string Tag { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
        public string DetailsUrl { get; set; }
        public string DownloadUrl { get; set; }
        public int DownloadSize { get; set; }

        public AppRelease() { }

        public AppRelease(Octokit.Release release)
        {
            const string versionRegex = @"(\d+(?:\.\d+){1,3})";

            var match = Regex.Match(release.Name, versionRegex);
            if (match.Success)
                Version = Version.Parse(match.Groups[1].Value);

            Name = release.Name;
            Tag = release.TagName;
            Description = release.Body;
            ReleaseDate = release.PublishedAt;
            DetailsUrl = release.HtmlUrl;

            var asset = release.Assets.OrderByDescending(a => a.Size)
                .FirstOrDefault(a => a.Name.ToLower().EndsWith(".exe") || a.Name.ToLower().EndsWith(".msi"));

            DownloadUrl = asset?.BrowserDownloadUrl;
            DownloadSize = asset?.Size ?? 0;
        }
    }
}
