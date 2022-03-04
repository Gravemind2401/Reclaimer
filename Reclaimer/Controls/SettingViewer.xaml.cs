using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Reclaimer.Models;
using Reclaimer.Plugins;

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
            var assembly = plugin.GetType().Assembly.GetName();
            var origin = System.IO.Path.GetFileName(new Uri(assembly.CodeBase).LocalPath);
            txtVersion.Text = $"{origin} Version {assembly.Version}";
            propGrid.SelectedObject = plugin.settings;
        }

        void IDisposable.Dispose()
        {
            App.Settings.Save();
        }
    }
}
