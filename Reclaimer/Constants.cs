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
        public const string SharedFuncExportBitmaps = "Reclaimer.Plugins.ModelViewerPlugin.ExportBitmaps";
        public const string SharedFuncExportSelectedBitmaps = "Reclaimer.Plugins.ModelViewerPlugin.ExportSelectedBitmaps";

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
