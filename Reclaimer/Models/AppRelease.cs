﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;

namespace Reclaimer.Models
{
    public partial class AppRelease
    {
        [GeneratedRegex(@"(\d+(?:\.\d+){1,3})")]
        private static partial Regex RxVersionPattern();

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public string Name { get; set; }
        public string Tag { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
        public string DetailsUrl { get; set; }
        public string DownloadUrl { get; set; }
        public int DownloadSize { get; set; }

        [JsonIgnore]
        public string VersionDisplay => Version.ToString(3);

        [JsonIgnore]
        public string ReleaseDateDisplay => ReleaseDate?.ToString("d");

        public AppRelease() { }

        public AppRelease(Octokit.Release release)
        {
            var match = RxVersionPattern().Match(release.Name);
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
