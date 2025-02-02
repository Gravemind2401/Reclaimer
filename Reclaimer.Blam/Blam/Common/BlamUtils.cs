using Reclaimer.Geometry;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Common
{
    internal static partial class BlamUtils
    {
        //for sorting, ignore leading symbol chars
        [GeneratedRegex("(?<=^[^a-z0-9]*)[a-z0-9].*", RegexOptions.IgnoreCase)]
        private static partial Regex RxInstanceNameSort();

        //also pad numbers to 3 digits so abc_9 comes before abc_10 etc
        [GeneratedRegex("(?<!\\d)\\d{1,2}(?!\\d)", RegexOptions.IgnoreCase)]
        private static partial Regex RxInstanceNumberSort();

        //take the longest string of one or more words separated by underscores (ie "word_word_word", final word must be 2+ chars)
        [GeneratedRegex("[a-z]+(?:_[a-z]+)*(?<=[a-z]{2,})", RegexOptions.IgnoreCase)]
        private static partial Regex RxInstanceGroupName();

        //get the "installdir" value from a steam app manifest file
        [GeneratedRegex(@"\t""installdir""\t\t""([^""]+)""")]
        private static partial Regex RxSteamManifestInstallDir();

        public static IEnumerable<IGrouping<string, TInstance>> GroupGeometryInstances<TInstance>(IEnumerable<TInstance> instances, Func<TInstance, string> nameSelector)
        {
            return from i in instances
                   let name = nameSelector(i)
                   orderby GetSortValue(name), name
                   group i by GetGroupName(name) into g
                   orderby g.Key
                   select g;

            static string GetSortValue(string value)
            {
                var m = RxInstanceNameSort().Match(value);
                return m.Success ? RxInstanceNumberSort().Replace(m.Value, x => x.Value.PadLeft(3, '0')) : value;
            }

            static string GetGroupName(string value)
            {
                //note the matches are ranked by word count, not string length
                //this ensures that if only singular words were found, then the leftmost match is the one that gets used
                var m = RxInstanceGroupName().Matches(value);
                return m.Count == 0 ? value : m.OfType<Match>().MaxBy(m => m.Value.Count(c => c == '_')).Value;
            }
        }

        public static IndexBuffer CreateImpliedIndexBuffer(int vertexCount, IndexFormat indexFormat)
        {
            //decorator models have no explicit index buffer.
            //instead, the index buffer is implied to be a triangle strip ranging from zero to N-1 where N is the vertex count.

            Type indexType;
            byte[] buffer;

            if (vertexCount > ushort.MaxValue)
            {
                indexType = typeof(int);
                buffer = new byte[vertexCount * sizeof(int)];
                var indices = MemoryMarshal.Cast<byte, int>(buffer);
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = i;
            }
            else
            {
                indexType = typeof(ushort);
                buffer = new byte[vertexCount * sizeof(ushort)];
                var indices = MemoryMarshal.Cast<byte, ushort>(buffer);
                for (var i = 0; i < indices.Length; i++)
                    indices[i] = (ushort)i;
            }

            return new IndexBuffer(buffer, indexType) { Layout = indexFormat };
        }

        private static readonly ConditionalWeakTable<ICacheFile, string> resourceDirectoryCache = new();
        public static string FindResourceFile(ICacheFile source, string targetName)
        {
            //TODO: find examples of workshop maps that make use of shared files, for engines other than Halo1,
            //then update the resource code for those engines to use this function

            lock (resourceDirectoryCache)
            {
                //this assumes all resource files are in the same directory
                if (resourceDirectoryCache.TryGetValue(source, out var targetDir))
                    return Path.Combine(targetDir, targetName);

                //test for local resources first - this should be the case for all standard maps
                var sourceDir = Path.GetDirectoryName(source.FileName);
                var testPath = Path.Combine(sourceDir, targetName);
                if (File.Exists(testPath))
                    return SetResult(testPath);

                //check if this is a workshop map
                testPath = Path.Combine(sourceDir, "..", "ModInfo.json");
                if (!File.Exists(testPath))
                    return SetResult(null); //not MCC; give up

                var steamAppsPath = Path.GetFullPath(Path.Combine(sourceDir, @"..\..\..\..\.."));
                testPath = Path.Combine(steamAppsPath, "appmanifest_976730.acf");
                if (!File.Exists(testPath))
                    return SetResult(null); //leftover workshop files?

                //find the MCC install directory
                var manifestContent = File.ReadAllText(testPath);
                var match = RxSteamManifestInstallDir().Match(manifestContent);
                if (!match.Success)
                    return SetResult(null);

                //get the relative path to the shared maps folder, depending on the engine
                var installDir = match.Groups[1].Value;
                var mapsDir = source.Metadata.Game switch
                {
                    HaloGame.Halo1 => @"halo1\maps\custom_edition",
                    HaloGame.Halo2 => @"halo2\h2_maps_win64_dx11",
                    HaloGame.Halo3 => @"halo3\maps",
                    HaloGame.Halo3ODST => @"halo3odst\maps",
                    HaloGame.Halo4 => @"halo4\maps",
                    HaloGame.HaloReach => @"haloreach\maps",
                    HaloGame.Halo2X => @"groundhog\maps",
                    _ => throw new NotSupportedException()
                };

                testPath = Path.Combine(steamAppsPath, "common", installDir, mapsDir, targetName);
                return SetResult(File.Exists(testPath) ? testPath : null);
            }

            string SetResult(string result)
            {
                var dir = string.IsNullOrEmpty(result) ? result : Path.GetDirectoryName(result);
                resourceDirectoryCache.AddOrUpdate(source, dir);
                return result;
            }
        }
    }
}
