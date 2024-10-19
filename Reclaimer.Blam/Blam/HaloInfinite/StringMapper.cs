using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class StringMapper
    {
        public static StringMapper Instance { get; } = new();

        public Dictionary<int, string> StringMappings { get; } = new();

        /// <summary>
        /// Loads string mappings from a file that contains hashes and
        /// their corresponding strings on each line, separated by a semicolon.
        /// </summary>
        public void LoadStringMap(string filename)
        {
            foreach (var line in File.ReadLines(filename))
            {
                var parts = line.Split(":");
                if (parts.Length == 2)
                    StringMappings[Convert.ToInt32(parts[0])] = parts[1];
            }
        }
    }
}
