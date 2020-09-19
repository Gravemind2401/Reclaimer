using Reclaimer.Models;
using Reclaimer.Plugins;
using System;
using System.Collections.Generic;
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

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for SettingViewer.xaml
    /// </summary>
    public partial class SettingViewer : UserControl
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
            var plugins = Substrate.AllPlugins
                .Where(p => p.settings != null)
                .OrderBy(p => p.Name)
                .ToList();

            txtSearch.ItemsSource = plugins;
            txtSearch.SelectedIndex = 0;
        }

        private void txtSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var plugin = (Plugin)txtSearch.SelectedItem;
            propGrid.SelectedObject = plugin.settings;
        }
    }
}
