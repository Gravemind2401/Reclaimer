using ControlzEx.Standard;
using Reclaimer.Blam.Common;
using Reclaimer.Plugins;
using Reclaimer.Windows;
using System.IO;
using System.Reflection;
using System.Text;
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
            var splitIndex = args.Name.IndexOf(',');
            var assemblyName = args.Name[..splitIndex];

            if (assemblyName.EndsWith(".resources"))
                return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName[..splitIndex] == assemblyName);
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

            // Get the command-line arguments
            var commandLineArgs = Environment.GetCommandLineArgs();

            // If there are 2 or fewer command-line arguments, create and show the main window
            if (commandLineArgs.Length <= 2)
            {
                MainWindow = new Windows.MainWindow();
                MainWindow.Show();

                ProcessCommandLineArguments(Environment.GetCommandLineArgs());
            }
            else
            {
                // Access the 2nd command-line argument using commandLineArgs[1]
                string secondArg = commandLineArgs[1].ToLower(); // Using ToLower for case-insensitive comparison

                //load plugins for settings etc
                Substrate.LoadPlugins();

                if (secondArg == "report")
                {
                    string mapFile = commandLineArgs[2];
                    string outputDir = commandLineArgs[3];
                    string tagType = commandLineArgs[4].ToLower();  // Convert to lowercase for case-insensitive comparison

                    var map = CacheFactory.ReadCacheFile(mapFile);
                    var tags = map.TagIndex.ToList();

                    StringBuilder sb = new StringBuilder();

                    foreach (var tag in tags)
                    {
                        string tagString = tag.ToString();

                        if (tagType == "render_model" && tagString.StartsWith("[mode]"))
                        {
                            sb.AppendLine(tagString.Substring(7) + ".render_model");
                        }
                        else if (tagType == "scenario_structure_bsp" && tagString.StartsWith("[sbsp]"))
                        {
                            sb.AppendLine(tagString.Substring(7) + ".scenario_structure_bsp");
                        }
                    }

                    // Generate the output file path
                    string outputFile = Path.Combine(outputDir, "tags_report.txt");

                    // Write the string data to the file, overwriting if it already exists
                    File.WriteAllText(outputFile, sb.ToString());
                }
                else if (secondArg == "export")
                {
                    // Handle the 'export' case here
                    string mapFile = commandLineArgs[2];  // Assuming 3rd arg is .map file
                    string tagFile = commandLineArgs[3]; //Assuming 4th arg is Tag File
                    string outputDir = commandLineArgs[4];  // Assuming 5th arg is output dir
                    string outputFormat = commandLineArgs[5];  // Assuming 6th arg is output format

                    var map = CacheFactory.ReadCacheFile(mapFile);

                    // maybe make scale be an argument too


                    // Remove the period and everything after it from tagFile
                    string tagType = Path.GetExtension(tagFile).TrimStart('.').ToLower();

                    string outputFilename = string.Empty;

                    outputFilename = Path.GetFileNameWithoutExtension(tagFile);

                    //remove the file extension from the tag path
                    tagFile = Path.Combine(Path.GetDirectoryName(tagFile), Path.GetFileNameWithoutExtension(tagFile));


                    string classCode = string.Empty;

                    // Set classCode based on tagType
                    if (tagType == "render_model")
                    {
                        classCode = "mode";
                    }
                    else if (tagType == "scenario_structure_bsp")
                    {
                        classCode = "sbsp";
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported tag type: {tagType}");
                    }

                    var tag = map.TagIndex.First(t => t.TagName == tagFile && t.ClassCode == classCode);

                    if (!ContentFactory.TryGetGeometryContent(tag, out var model))
                    {
                        throw new NotSupportedException();
                    }

                    var geometry = model.ReadGeometry(0);

                    ModelViewerPlugin.WriteModelFile(geometry, outputDir + "\\" + outputFilename + "." + outputFormat, outputFormat);
                }
                else
                {
                    // Handle invalid 2nd argument here
                    Console.WriteLine("Invalid 2nd command-line argument. Only 'report' or 'export' are allowed.");
                }

            }

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

                // Check if the number of arguments is 2 or fewer, then activate the main window
                if (arguments.Length <= 2 && MainWindow != null)
                {
                    MainWindow.Activate();
                }
            });
        }

        private static void LogUnhandledException(Exception ex)
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
