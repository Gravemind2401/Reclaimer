using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
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
        static App()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            ReleaseVersion = new Version(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Revision);
        }

        internal static readonly Version ReleaseVersion;
        internal static App Instance { get; private set; }
        internal static Settings Settings { get; private set; }
        internal static UserSettings UserSettings => Settings.UserSettings;

        private static readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

#if DEBUG
        public static string AppVersion => "DEBUG";
#else
        public static string AppVersion => ReleaseVersion.ToString(3);
#endif

        public App() : base()
        {
            Instance = this;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) => LogUnhandledException(e.Exception);

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
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
            if (!SingletonHelper.EnsureSingleInstance(Constants.ApplicationInstanceKey, ProcessCommandLineArguments))
                return;

            base.OnStartup(e);

            var embeddedThemes = (ResourceDictionary[])Resources["Themes"];
            foreach (var theme in embeddedThemes.Skip(1))
            {
                theme.MergedDictionaries.Insert(0, embeddedThemes[0]);
                AddTheme((string)theme["Name"], theme);
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
                    else
                        Substrate.LogOutput($"No handler found for file: {arg}");
                }

                MainWindow.Activate();
            });
        }

        private void LogUnhandledException(Exception ex)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();

            var fileName = Path.Combine(Settings.AppDataDirectory, Constants.CrashDumpFileName);
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
            Instance.Resources.MergedDictionaries.Add(themes[theme]);

            Settings.Theme = theme;
        }

        #endregion
    }
}
