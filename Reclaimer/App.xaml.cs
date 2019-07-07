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
        public static App Instance { get; private set; }

        private ResourceDictionary defaultResources;
        private readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

        public App() : base()
        {
            Instance = this;
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

            SetTheme(themes.Keys.First());
        }

        public IEnumerable<string> Themes => themes.Keys.AsEnumerable();

        public void AddTheme(string name, ResourceDictionary theme)
        {
            if (!themes.ContainsKey(name))
                themes.Add(name, new ResourceDictionary());

            themes[name].MergedDictionaries.Add(theme);
        }

        public void SetTheme(string theme)
        {
            if (!themes.ContainsKey(theme))
                throw new KeyNotFoundException($"'{theme}' does not exist");

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(defaultResources);
            Resources.MergedDictionaries.Add(themes[theme]);
        }
    }
}
