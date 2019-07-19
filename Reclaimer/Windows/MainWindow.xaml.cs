using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Studio.Controls;
using Reclaimer.Plugins;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IMultiPanelHost
    {
        MultiPanel IMultiPanelHost.MultiPanel => MainPanel;

        DocumentTabControl IMultiPanelHost.DocumentContainer => docTab;

        private readonly Controls.OutputViewer outputViewer;

        public MainWindow()
        {
            InitializeComponent();

            Substrate.LoadPlugins();
            outputViewer = new Controls.OutputViewer();
        }

        private async void menuImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Halo Map Files|*.map",
                Multiselect = true,
                CheckFileExists = true
            };

            if (ofd.ShowDialog() != true)
                return;

            await Task.Run(async () =>
            {
                foreach (var fileName in ofd.FileNames)
                {
                    if (!File.Exists(fileName))
                        continue;

                    await Storage.ImportCacheFile(fileName);
                }

                MessageBox.Show("all done");
            });
        }

        private void menuTagViewer_Click(object sender, RoutedEventArgs e)
        {
            var tc = MainPanel.GetElementAtPath(Dock.Left) as UtilityTabControl ?? new UtilityTabControl();
            tc.Items.Add(new Controls.TagViewer());

            if (!MainPanel.GetChildren().Contains(tc))
                MainPanel.AddElement(tc, null, Dock.Left, new GridLength(400));
        }

        private void menuOutput_Click(object sender, RoutedEventArgs e)
        {
            if (outputViewer.Parent != null)
                return;

            var tc = MainPanel.GetElementAtPath(Dock.Bottom) as UtilityTabControl ?? new UtilityTabControl();

            if (!MainPanel.GetChildren().Contains(tc))
                MainPanel.AddElement(tc, null, Dock.Bottom, new GridLength(250));

            tc.Items.Add(outputViewer);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //add initial menu items
            foreach (MenuItem item in menu.Items)
                menuLookup.Add(item.Header as string, item);

            foreach (var plugin in Substrate.AllPlugins)
            {
                foreach (var item in plugin.MenuItems)
                    AddMenuItem(plugin, item);
            }

            var themeRoot = GetMenuItem("Themes");
            foreach (var theme in App.Themes)
            {
                var item = new MenuItem { Header = theme, Tag = theme };
                themeRoot.Items.Add(item);
                item.Click += ThemeMenuItem_Click;
            }

            WindowState = App.Settings.WindowState;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Settings.WindowState = WindowState;
        }

        private void ThemeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.SetTheme((sender as MenuItem).Tag as string);
        }

        private void AddMenuItem(Plugin source, PluginMenuItem item)
        {
            var menuItem = GetMenuItem(item.Path);
            var fullKey = $"{source.Key}::{item.Key}";
            menuItem.Tag = fullKey;

            menuItem.Click -= GetHandler(source, item.Key); // incase the key is not unique
            menuItem.Click += GetHandler(source, item.Key);

            var root = GetRoot(menuItem);
            if (!menu.Items.Contains(root))
                menu.Items.Add(root);
        }

        private Dictionary<string, RoutedEventHandler> actionLookup = new Dictionary<string, RoutedEventHandler>();
        private RoutedEventHandler GetHandler(Plugin source, string key)
        {
            var fullKey = $"{source.Key}::{key}";
            if (actionLookup.ContainsKey(fullKey))
                return actionLookup[fullKey];

            var action = new RoutedEventHandler((s, e) => source.OnMenuItemClick(key));
            actionLookup.Add(fullKey, action);

            return action;
        }

        private Dictionary<string, MenuItem> menuLookup = new Dictionary<string, MenuItem>();
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
