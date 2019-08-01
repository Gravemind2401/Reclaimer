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

        private void menuOutput_Click(object sender, RoutedEventArgs e)
        {
            if (outputViewer.Parent != null)
                return;

            Substrate.AddUtility(outputViewer, Substrate.GetHostWindow(), Dock.Bottom, new GridLength(250));
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
            menuItem.Tag = item;

            menuItem.Click -= CustomMenuItem_Click; // incase the key is not unique
            menuItem.Click += CustomMenuItem_Click;

            var root = GetRoot(menuItem);
            if (!menu.Items.Contains(root))
                menu.Items.Add(root);
        }

        private void CustomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem)?.Tag as PluginMenuItem;
            item?.ExecuteHandler();
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
