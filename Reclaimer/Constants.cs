﻿namespace Reclaimer
{
    internal static class Constants
    {
        //global
        public const string PluginsFolderToken = ":plugins:";
        public const string SharedFuncGetDataFolder = "Reclaimer.Plugins.BatchExtractPlugin.GetDataFolder";
        public const string SharedFuncGetModelExtension = "Reclaimer.Plugins.ModelViewerPlugin.GetFormatExtension";
        public const string SharedFuncBatchSaveImage = "Reclaimer.Plugins.BatchExtractPlugin.SaveImage";
        public const string SharedFuncBatchSaveModel = "Reclaimer.Plugins.BatchExtractPlugin.SaveModel";
        public const string SharedFuncBatchSaveSound = "Reclaimer.Plugins.BatchExtractPlugin.SaveSound";
        public const string SharedFuncWriteModelFile = "Reclaimer.Plugins.ModelViewerPlugin.WriteModelFile";
        public const string SharedFuncWriteSoundFile = "Reclaimer.Plugins.SoundExtractorPlugin.WriteSoundFile";
        public const string SharedFuncExportBitmaps = "Reclaimer.Plugins.ModelViewerPlugin.ExportBitmaps";

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
