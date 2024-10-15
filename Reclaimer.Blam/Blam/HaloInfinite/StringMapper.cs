using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class StringMapper
    {
        private static StringMapper _instance;
        public static StringMapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StringMapper();
                }
                return _instance;
            }
        }

        public Dictionary<int, string> StringMappings;

        private StringMapper()
        {
            StringMappings = new Dictionary<int, string>();
        }

        /// <summary>
        /// Loads string mappings from a file that contains hashes and
        /// their corresponding strings on each line, separated by a semicolon. 
        /// </summary>
        /// <param name="filename"></param>
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
