using Adjutant.Audio;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public class SoundExtractorPlugin : Plugin
    {
        public override string Name => "Sound Extractor";

        private static SoundExtractorSettings Settings;

        public override void Initialise()
        {
            Settings = LoadSettings<SoundExtractorSettings>();

            if (!File.Exists(Settings.FFmpegPath))
                throw new FileNotFoundException("FFmpeg is required for sound extraction but was not found.");
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        [SharedFunction]
        public static bool WriteSoundFile(GameSound sound, string directory, bool overwrite)
        {
            var extracted = 0;

            var ext = "." + Settings.OutputExtension;
            for (int i = 0; i < sound.Permutations.Count; i++)
            {
                var permutation = sound.Permutations[i];
                var targetFile = Path.Combine(directory, string.Format(Settings.OutputNameFormat, sound.Name, permutation.Name, i) + ext);

                if (!overwrite && File.Exists(targetFile))
                    continue;

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = Settings.FFmpegPath,
                        Arguments = $"-y -i - {targetFile}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                process.Start();
                process.BeginErrorReadLine();

                var inputStream = process.StandardInput.BaseStream;
                SoundUtils.WriteRiffData(inputStream, sound.FormatHeader, permutation.SoundData);

                process.WaitForExit();
                extracted++;
            }

            return extracted > 0;
        }

        private class SoundExtractorSettings
        {
            public string FFmpegPath { get; set; }
            public string OutputExtension { get; set; }
            public string OutputNameFormat { get; set; }

            public SoundExtractorSettings()
            {
                var relative = Substrate.PluginsDirectory.Replace(AppDomain.CurrentDomain.BaseDirectory, string.Empty);
                FFmpegPath = Path.Combine(".", relative, "ffmpeg", "ffmpeg.exe");
                OutputExtension = "wav";
                OutputNameFormat = "{0}[{1}]";
            }
        }
    }
}
