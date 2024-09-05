using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class TagMapper
    {
        public static Dictionary<int, string> TagMappings = new();
        public static void LoadTagMap(string filename)
        {
            foreach (var line in File.ReadLines(filename))
            {
                var parts = line.Split(" : ");
                if (parts.Length == 2)
                {
                    var intValue = ReverseEndianness(Convert.ToInt32(parts[0].Trim(), 16));
                    var name = parts[1].Trim();
                    TagMappings[intValue] = name;
                }
            }
        }

        private static int ReverseEndianness(int value)
        {
            return (value >> 24) |
                   ((value >> 8) & 0x0000FF00) |
                   ((value & 0x0000FF00) << 8) |
                   (value << 24);
        }

    }
}
