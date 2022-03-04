using Reclaimer.Audio;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public static class SoundUtils
    {
        //null header indicates the data should be written as-is
        public static void WriteRiffData(Stream output, IFormatHeader header, byte[] data)
        {
            using (var sw = new EndianWriter(output, ByteOrder.LittleEndian))
            {
                if (header == null)
                {
                    sw.Write(data);
                    return;
                }

                sw.WriteStringFixedLength("RIFF");
                sw.Write(data.Length + header.Length + 20); //20 is the combined size of strings length ints
                sw.WriteStringFixedLength("WAVE");
                sw.WriteStringFixedLength("fmt ");
                sw.Write(header.Length);
                sw.Write(header.GetBytes());
                sw.WriteStringFixedLength("data");
                sw.Write(data.Length);
                sw.Write(data);
            }
        }
    }
}
