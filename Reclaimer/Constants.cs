using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer
{
    internal static class Constants
    {
        //global
        public const string PluginsFolderToken = ":plugins:";
        public const string SharedFuncGetDataFolder = "Reclaimer.Plugins.BatchExtractPlugin.GetDataFolder";
        public const string SharedFuncGetModelExtension = "Reclaimer.Plugins.ModelViewerPlugin.GetFormatExtension";
        public const string SharedFuncSaveImage = "Reclaimer.Plugins.BatchExtractPlugin.SaveImage";
        public const string SharedFuncWriteModelFile = "Reclaimer.Plugins.ModelViewerPlugin.WriteModelFile";
        public const string SharedFuncWriteSoundFile = "Reclaimer.Plugins.SoundExtractorPlugin.WriteSoundFile";

        //App.xaml
        public const string ApplicationInstanceKey = "Reclaimer.Application";
        public const string CrashDumpFileName = "crash.txt";

        //Settings
        public const string AppDataSubfolder = "Gravemind2401\\Reclaimer";
        public const string SettingsFileName = "settings.json";

        //Substrate
        public const string PluginsFolderName = "Plugins";
    }
}
