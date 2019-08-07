using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private static ResourceDictionary templateResources;
        private static readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

        public App() : base()
        {
            Instance = this;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = args.Name.Substring(0, args.Name.IndexOf(','));

            if (assemblyName.EndsWith(".resources"))
                return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Substring(0, args.Name.IndexOf(',')) == assemblyName);
            if (assembly != null)
                return assembly;

            try
            {
                var test = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + ".dll");
                if (File.Exists(test))
                    return Assembly.LoadFrom(test);

                test = Path.Combine(Substrate.PluginsDirectory, "Dependencies", assemblyName + ".dll");
                if (File.Exists(test))
                    return Assembly.LoadFrom(test);
            }
            catch { }

            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!InstanceManager.CreateSingleInstance("Reclaimer.Application", OnReceivedCommandLineArguments))
                return;

            base.OnStartup(e);

            defaultResources = new ResourceDictionary { Source = new Uri("/Reclaimer;component/Themes/Default.xaml", UriKind.RelativeOrAbsolute) };
            templateResources = new ResourceDictionary { Source = new Uri("/Reclaimer;component/Resources/Templates.xaml", UriKind.RelativeOrAbsolute) };

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

            MainWindow = new Windows.MainWindow();
            MainWindow.Show();

            ProcessCommandLineArguments(Environment.GetCommandLineArgs());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Substrate.Shutdown();
            base.OnExit(e);
        }

        private void OnReceivedCommandLineArguments(object sender, InstanceCallbackEventArgs e)
        {
            ProcessCommandLineArguments(e.Arguments);
        }

        private void ProcessCommandLineArguments(params string[] arguments)
        {
            if (Dispatcher == null || arguments == null || arguments.Length == 0)
                return;

            Dispatcher.Invoke(() =>
            {
                foreach (var arg in arguments)
                {
                    if (Path.GetInvalidPathChars().Any(c => arg.Contains(c)))
                        continue; // not a file name

                    if (!Path.HasExtension(arg) || Path.GetExtension(arg).ToLower() == ".exe" || !File.Exists(arg))
                        continue;

                    if (Substrate.HandlePhysicalFile(arg))
                        Substrate.LogOutput($"Handled file: {arg}");
                    else Substrate.LogOutput($"No handler found for file: {arg}");
                }

                MainWindow.Activate();
            });
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
            Instance.Resources.MergedDictionaries.Add(templateResources);
            Instance.Resources.MergedDictionaries.Add(themes[theme]);

            Settings.Theme = theme;
        }

        #endregion
    }
}
