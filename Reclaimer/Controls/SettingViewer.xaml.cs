using Reclaimer.Models;
using Reclaimer.Plugins;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for SettingViewer.xaml
    /// </summary>
    public partial class SettingViewer : UserControl, IDisposable
    {
        public TabModel TabModel { get; }

        public SettingViewer()
        {
            InitializeComponent();
            TabModel = new TabModel(this, Studio.Controls.TabItemType.Tool);
            TabModel.Header = TabModel.ToolTip = "Settings";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var defaultPlugin = Substrate.AllPlugins.First();
            var plugins = Substrate.AllPlugins
                .Where(p => p.settings != null)
                .OrderByDescending(p => p == defaultPlugin)
                .ThenBy(p => p.Name)
                .ToList();

            cmbPlugins.ItemsSource = plugins;
            cmbPlugins.SelectedIndex = 0;
        }

        private void cmbPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var plugin = (Plugin)cmbPlugins.SelectedItem;
            var assembly = plugin.GetType().Assembly;
            var origin = System.IO.Path.GetFileName(new Uri(assembly.Location).LocalPath);
            txtVersion.Text = $"{origin} Version {assembly.GetName().Version}";
            propGrid.SelectedObject = plugin.settings;
        }

        void IDisposable.Dispose() => App.Settings.Save();
    }
}
