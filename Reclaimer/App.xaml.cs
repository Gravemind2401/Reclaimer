using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static App Instance { get; private set; }
        internal static Settings Settings { get; private set; }

        private static ResourceDictionary defaultResources;
        private static readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();


        public App() : base()
        {
            Instance = this;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            defaultResources = new ResourceDictionary { Source = new Uri("/Reclaimer;component/Themes/Default.xaml", UriKind.RelativeOrAbsolute) };

            var blue = new ResourceDictionary();
            blue.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Studio;component/Themes/Blue.xaml", UriKind.RelativeOrAbsolute) });
            blue.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Reclaimer;component/Themes/Blue.xaml", UriKind.RelativeOrAbsolute) });
            AddTheme("Blue", blue);

            var dark = new ResourceDictionary();
            dark.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Studio;component/Themes/Dark.xaml", UriKind.RelativeOrAbsolute) });
            dark.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/Reclaimer;component/Themes/Dark.xaml", UriKind.RelativeOrAbsolute) });
            AddTheme("Dark", dark);

            Settings = Settings.FromFile();

            SetTheme(Settings.Theme);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Substrate.Shutdown();
            base.OnExit(e);
        }

        #region Themes

        internal static IEnumerable<string> Themes => themes.Keys.AsEnumerable();

        internal static void AddTheme(string name, ResourceDictionary theme)
        {
            if (!themes.ContainsKey(name))
                themes.Add(name, new ResourceDictionary());

            themes[name].MergedDictionaries.Add(theme);
        }

        internal static void SetTheme(string theme)
        {
            if (!themes.ContainsKey(theme))
                throw new KeyNotFoundException($"'{theme}' does not exist");

            Instance.Resources.MergedDictionaries.Clear();
            Instance.Resources.MergedDictionaries.Add(defaultResources);
            Instance.Resources.MergedDictionaries.Add(themes[theme]);

            Settings.Theme = theme;
        }

        #endregion
    }
}
