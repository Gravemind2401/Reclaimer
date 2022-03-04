using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Annotations;
using Reclaimer.Audio;
using Reclaimer.Blam.Utilities;
using Reclaimer.Controls.Editors;

namespace Reclaimer.Plugins
{
    public class SoundExtractorPlugin : Plugin
    {
        private const string FFmpegFolderName = "ffmpeg";
        private const string FFmpegExecutableName = "ffmpeg.exe";

        private static SoundExtractorSettings Settings;
        private static SoundExtractorPlugin Instance;

        public override string Name => "Sound Extractor";

        public override void Initialise()
        {
            Instance = this;
            Settings = LoadSettings<SoundExtractorSettings>();

            if (!Settings.NoConversion && !File.Exists(Settings.FFmpegPath))
            {
                SaveSettings(Settings);
                throw new FileNotFoundException("FFmpeg is required for sound conversion but was not found.");
            }
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
        }

        [SharedFunction]
        public static bool WriteSoundFile(GameSound sound, string directory, bool overwrite)
        {
            var extracted = 0;

            var ext = "." + (Settings.NoConversion ? sound.DefaultExtension : Settings.OutputExtension);
            for (int i = 0; i < sound.Permutations.Count; i++)
            {
                var permutation = sound.Permutations[i];
                var targetFile = Path.Combine(directory, string.Format(Settings.OutputNameFormat, sound.Name, permutation.Name, i) + ext);

                if (!overwrite && File.Exists(targetFile))
                    continue;

                if (Settings.NoConversion)
                {
                    using (var fs = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
                        SoundUtils.WriteRiffData(fs, sound.FormatHeader, permutation.SoundData);
                }
                else
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = Settings.FFmpegPath,
                            Arguments = $"-y -i - \"{targetFile}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardInput = true,
                            RedirectStandardError = true
                        },
                        EnableRaisingEvents = true
                    };

                    if (Settings.LogFFmpegOutput)
                    {
                        Instance.ClearLog();
                        process.ErrorDataReceived += Process_ErrorDataReceived;
                    }

                    process.Start();
                    process.BeginErrorReadLine();

                    var inputStream = process.StandardInput.BaseStream;
                    SoundUtils.WriteRiffData(inputStream, sound.FormatHeader, permutation.SoundData);

                    process.WaitForExit();
                }

                extracted++;
            }

            return extracted > 0;
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Instance.LogOutput(e.Data);
        }

        private sealed class SoundExtractorSettings
        {
            [Editor(typeof(BrowseFileEditor), typeof(PropertyValueEditor))]
            [DisplayName("FFmpeg Path")]
            [RuntimeDefaultValue(typeof(SoundExtractorSettings), nameof(DefaultFFmpegPath))]
            public string FFmpegPath { get; set; }

            [DisplayName("Output File Extension")]
            [DefaultValue("wav")]
            public string OutputExtension { get; set; }

            [DisplayName("Output Filename Format")]
            [DefaultValue("{0}[{1}]")]
            public string OutputNameFormat { get; set; }

            [DisplayName("Log FFmpeg Output")]
            [DefaultValue(false)]
            public bool LogFFmpegOutput { get; set; }

            internal bool NoConversion => string.IsNullOrWhiteSpace(OutputExtension);

            private static string DefaultFFmpegPath
            {
                get
                {
                    var relative = Substrate.PluginsDirectory.Replace(Reclaimer.Settings.AppBaseDirectory, string.Empty);
                    return Path.Combine(".", relative, FFmpegFolderName, FFmpegExecutableName);
                }
            }
        }
    }
}
