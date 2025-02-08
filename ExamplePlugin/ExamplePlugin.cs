using Reclaimer.Annotations;
using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

#pragma warning disable IDE0052 // Remove unread private members

namespace ExamplePlugin
{
    public class ExamplePlugin : Plugin
    {
        public override string Name => "ZZZ_Example Plugin";

        private PluginSettings Settings { get; set; }

        private delegate int ForeignFunction(string param0, int param1);
        private ForeignFunction NotALocalFunction;

        public override void Initialise()
        {
            Settings = LoadSettings<PluginSettings>();
            LogOutput("Loaded example settings");

            NotALocalFunction = Substrate.GetSharedFunction<ForeignFunction>("PluginNamespace.PluginClass.FunctionName");
        }

        public override IEnumerable<PluginMenuItem> GetMenuItems()
        {
            yield return new PluginMenuItem("Key1", "Example\\Item 1", OnMenuItemClick);
            yield return new PluginMenuItem("Key2", "Example\\Item 2", OnMenuItemClick);
            yield return new PluginMenuItem("Key3", "Example\\Output Test", OnMenuItemClick);
        }

        private void OnMenuItemClick(string key)
        {
            if (key == "Key1")
                MessageBox.Show("Item 1 Click");
            else if (key == "Key2")
                MessageBox.Show("Item 2 Click");
            else
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        LogOutput($"Output line {i}!");
                        System.Threading.Thread.Sleep(100);
                    }
                });
            }
        }

        public override bool CanOpenFile(OpenFileArgs args) => true;

        public override void OpenFile(OpenFileArgs args)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open {args.FileName}");
                LogError("Not Implemented!", ex);
            }
        }

        public override void Suspend()
        {
            SaveSettings(Settings);
            LogOutput("Saved example settings");
        }

        [SharedFunction]
        public void OtherPluginsCanUseMe(string message) => LogOutput($"Shared Function Message: {message}");

        private class PluginSettings
        {
            public string ExampleSetting { get; set; }

            public PluginSettings()
            {
                ExampleSetting = "<insert value here>";
            }
        }
    }
}
