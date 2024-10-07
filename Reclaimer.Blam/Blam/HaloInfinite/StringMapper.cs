using System.Buffers.Binary;
using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class StringMapper
    {
        public static Dictionary<int, string> TagMappings = new();
        public static Dictionary<uint, string> StringMappings = new();
        public static void LoadTagMap(string filename)
        {
            foreach (var line in File.ReadLines(filename))
            {
                var parts = line.Split(" : ");
                if (parts.Length == 2)
                {
                    var intValue = BinaryPrimitives.ReverseEndianness(Convert.ToInt32(parts[0].Trim(), 16));
                    var name = parts[1].Trim();
                    TagMappings[intValue] = Path.ChangeExtension(name, null);
                }
            }
        }

        public static void LoadStringMap(string filename)
        {
            foreach (var line in File.ReadLines(filename))
            {
                var parts = line.Split(":");
                if (parts.Length == 2)
                {
                    var intValue = BinaryPrimitives.ReverseEndianness(Convert.ToUInt32(parts[0].Trim(), 16));
                    var name = parts[1].Trim();
                    StringMappings[intValue] = name;
                }
            }
        }

    }
}
