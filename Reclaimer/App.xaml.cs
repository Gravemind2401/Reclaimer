using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        internal static UserSettings UserSettings => Settings.UserSettings;

        private static ResourceDictionary styleResources;
        private static readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

        public App() : base()
        {
            Instance = this;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogUnhandledException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                LogUnhandledException(ex);
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

                var root = Path.GetDirectoryName(args.RequestingAssembly.Location);
                test = Path.Combine(root, assemblyName + ".dll");
                if (File.Exists(test))
                    return Assembly.LoadFile(test);
            }
            catch { }

            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!InstanceManager.CreateSingleInstance("Reclaimer.Application", OnReceivedCommandLineArguments))
                return;

            base.OnStartup(e);

            styleResources = new ResourceDictionary { Source = new Uri("/Reclaimer;component/Resources/Styles.xaml", UriKind.RelativeOrAbsolute) };

            var themeList = new[] { "Blue", "Dark", "Light", "Green", "Purple", "Red", "Tan", "Solarized (Dark)", "Solarized (Light)" };

            foreach (var s in themeList)
            {
                var resource = Regex.Replace(s, @"[ \(\)]", string.Empty);

                var theme = new ResourceDictionary();
                theme.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/Studio;component/Themes/{resource}.xaml", UriKind.RelativeOrAbsolute) });

                if (resource != "Blue" && resource != "Dark")
                    resource = resource.Contains("Dark") ? "Dark" : "Blue";

                theme.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/Reclaimer;component/Themes/{resource}.xaml", UriKind.RelativeOrAbsolute) });
                AddTheme(s, theme);
            }

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

        private void LogUnhandledException(Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();

            var fileName = Path.Combine(Settings.AppDataDirectory, "crash.txt");
            File.WriteAllText(fileName, ex.ToString());
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
            Instance.Resources.MergedDictionaries.Add(styleResources);
            Instance.Resources.MergedDictionaries.Add(themes[theme]);

            Settings.Theme = theme;
        }

        #endregion
    }
}
