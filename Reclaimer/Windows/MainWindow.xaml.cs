using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Octokit;
using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Utilities;
using Terminology = Reclaimer.Resources.Terminology;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, ITabContentHost
    {
        #region Dependency Properties
        private static readonly DependencyPropertyKey HasUpdatePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasUpdate), typeof(bool), typeof(MainWindow), new PropertyMetadata(false, null, (d, baseValue) =>
            {
                return App.Settings.LatestRelease?.Version > App.AssemblyVersion;
            }));

        public static readonly DependencyProperty HasUpdateProperty = HasUpdatePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsBusyPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsBusy), typeof(bool), typeof(MainWindow), new PropertyMetadata(false, (s, e) =>
            {
                (s as MainWindow).RefreshStatus();
            }));

        public static readonly DependencyProperty IsBusyProperty = IsBusyPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey CurrentStatusPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CurrentStatus), typeof(string), typeof(MainWindow), new PropertyMetadata());

        public static readonly DependencyProperty CurrentStatusProperty = CurrentStatusPropertyKey.DependencyProperty;

        public bool HasUpdate
        {
            get { return (bool)GetValue(HasUpdateProperty); }
        }

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            private set { SetValue(IsBusyPropertyKey, value); }
        }

        public string CurrentStatus
        {
            get { return (string)GetValue(CurrentStatusProperty); }
            private set { SetValue(CurrentStatusPropertyKey, value); }
        }
        #endregion

        DockContainerModel ITabContentHost.DockContainer => Model;
        DocumentPanelModel ITabContentHost.DocumentPanel => DocPanel;

        public DockContainerModel Model { get; }
        private DocumentPanelModel DocPanel { get; }
        private MenuItem RecentsMenuItem { get; }

        public MainWindow()
        {
            InitializeComponent();

            Model = new DockContainerModel();
            Model.Content = DocPanel = new DocumentPanelModel();
            RecentsMenuItem = new MenuItem { Header = Terminology.Menu.RecentFiles };

            Substrate.LoadPlugins();

            Substrate.StatusChanged += Substrate_StatusChanged;
            Substrate.RecentsChanged += Substrate_RecentsChanged;
        }

        #region Event Handlers
        private void menuOutput_Click(object sender, RoutedEventArgs e)
        {
            Substrate.ShowOutput();
        }

        private void menuAppDir_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Settings.AppBaseDirectory);
        }

        private void menuAppDataDir_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Settings.AppDataDirectory);
        }

        private void menuSettings_Click(object sender, RoutedEventArgs e)
        {
            const string tabId = "Reclaimer::Settings";
            if (Substrate.ShowTabById(tabId))
                return;

            var settings = new Controls.SettingViewer();
            settings.TabModel.ContentId = tabId;
            Substrate.AddTool(settings.TabModel, this, Dock.Right, new GridLength(350));
        }

        private void menuUpdates_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!await CheckForUpdates())
                {
                    MessageBox.Show(Terminology.Message.ErrorCheckingUpdates, Terminology.UI.Reclaimer);
                    return;
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    if (!HasUpdate)
                        MessageBox.Show(Terminology.Message.NoUpdatesAvailable, Terminology.UI.Reclaimer);
                    else
                        UpdateDialog.ShowUpdate();
                });
            });
        }

        private void menuIssue_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Gravemind2401/Reclaimer/issues");
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //add initial menu items
            foreach (MenuItem item in menu.Items)
                menuLookup.Add(item.Header as string, item);

            foreach (var plugin in Substrate.AllPlugins)
            {
                foreach (var item in plugin.GetMenuItems())
                    AddMenuItem(plugin, item);
            }

            var themeRoot = GetMenuItem(Terminology.Menu.Themes);
            foreach (var theme in App.Themes)
            {
                var item = new MenuItem { Header = theme, Tag = theme };
                themeRoot.Items.Add(item);
                item.Click += ThemeMenuItem_Click;
            }

            if (App.Settings.WindowState != WindowState.Minimized)
                WindowState = App.Settings.WindowState;

            CoerceValue(HasUpdateProperty);
            RefreshRecents();
            RefreshStatus();

            Controls.OutputViewer.Instance.Height = 250;
            Substrate.GetHostWindow().DockContainer.BottomDockItems.Add(Controls.OutputViewer.Instance);

            if (App.UserSettings.AutoUpdatesCheck && App.Settings.ShouldCheckUpdates)
                Task.Run(CheckForUpdates);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.UserSettings.RememberWindowState)
                App.Settings.WindowState = WindowState;
        }

        private void Substrate_StatusChanged(object sender, StatusChangedArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Status == null)
                    IsBusy = false;
                else
                {
                    IsBusy = true;
                    CurrentStatus = $"{e.PluginName}: {e.Status}";
                }
            });
        }

        private void Substrate_RecentsChanged(object sender, EventArgs e)
        {
            App.Settings.Save();
            RefreshRecents();
        }

        private void ThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.SetTheme((sender as MenuItem).Tag as string);
        }

        private void AddMenuItem(Plugin source, PluginMenuItem item)
        {
            var menuItem = GetMenuItem(item.Path);
            menuItem.Tag = item;

            menuItem.Click -= CustomMenuItem_Click; // incase the key is not unique
            menuItem.Click += CustomMenuItem_Click;

            var root = GetRoot(menuItem);
            if (!menu.Items.Contains(root))
                menu.Items.Add(root);
        }
        #endregion

        private async Task<bool> CheckForUpdates()
        {
            if (Dispatcher.Invoke(() => HasUpdate && !App.Settings.ShouldCheckUpdates))
                return true; //dont check again if we already found one recently

            Substrate.SetSystemWorkingStatus(Terminology.Status.CheckingForUpdates);

            try
            {
                var client = new GitHubClient(new ProductHeaderValue(nameof(Reclaimer), App.AppVersion));
                var latest = await client.Repository.Release.GetLatest("Gravemind2401", nameof(Reclaimer));

                App.Settings.LastUpdateCheck = DateTime.Now;
                App.Settings.LatestRelease = new AppRelease(latest);
                App.Settings.Save();

                await Dispatcher.InvokeAsync(() => CoerceValue(HasUpdateProperty));

                return true;
            }
            catch (Exception ex)
            {
                Substrate.LogError("Error checking for updates", ex);
                return false;
            }
            finally
            {
                Substrate.ClearSystemWorkingStatus();
            }
        }

        private void RefreshStatus()
        {
            if (!IsBusy)
                CurrentStatus = HasUpdate ? Terminology.Status.UpdateAvailable : Terminology.Status.Ready;
        }

        private void RefreshRecents()
        {
            const int maxChars = 60;

            RecentsMenuItem.Items.Clear();
            fileMenu.Items.Remove(RecentsMenuItem);

            foreach (var fileName in App.Settings.RecentFiles.Where(s => File.Exists(s)))
            {
                var displayName = fileName;
                if (displayName.Length > maxChars)
                {
                    var name = Path.GetFileName(fileName);
                    var dir = Directory.GetParent(fileName);
                    var drive = dir.Root.FullName;
                    var tally = drive.Length + name.Length;

                    var parts = dir.SplitPath()
                        .TakeWhile(s =>
                        {
                            if (tally + s.Length > maxChars)
                                return false;

                            tally += s.Length;
                            return true;
                        }).Reverse().ToArray();

                    displayName = Path.Combine(parts);
                    displayName = Path.Combine(drive, "...", displayName, name);
                }

                var item = new MenuItem { Header = displayName, Tag = fileName };
                RecentsMenuItem.Items.Add(item);
                item.Click += RecentsItem_Click;
            }

            if (RecentsMenuItem.HasItems)
                fileMenu.Items.Add(RecentsMenuItem);
        }

        private void RecentsItem_Click(object sender, RoutedEventArgs e)
        {
            var fileName = (sender as MenuItem)?.Tag as string;
            Substrate.HandlePhysicalFile(fileName);
        }

        private void CustomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem)?.Tag as PluginMenuItem;
            item?.ExecuteHandler();
        }

        private readonly Dictionary<string, MenuItem> menuLookup = new Dictionary<string, MenuItem>();
        private MenuItem GetMenuItem(string path)
        {
            if (menuLookup.ContainsKey(path))
                return menuLookup[path];

            var index = path.LastIndexOf('\\');
            var branch = index < 0 ? null : path.Substring(0, index);
            var leaf = index < 0 ? path : path.Substring(index + 1);

            var item = new MenuItem { Header = leaf };
            menuLookup.Add(path, item);

            if (branch == null)
                return item;

            var parent = GetMenuItem(branch);
            parent.Items.Add(item);

            return item;
        }

        private MenuItem GetRoot(MenuItem item)
        {
            var temp = item;
            while ((temp = temp.Parent as MenuItem) != null)
                item = temp;

            return item;
        }
    }
}