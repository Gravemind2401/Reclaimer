using Reclaimer.Models;
using Reclaimer.Plugins;
using Reclaimer.Plugins.MapBrowser;
using Studio.Controls;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MapBrowser.xaml
    /// </summary>
    public partial class MapBrowser : UserControl
    {
        public TabModel TabModel { get; }

        public MapBrowser()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Tool) { Header = "Map Browser" };
            DataContext = MapLibraryModel.Build();
        }

        private void GroupListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualTreeHelperEx.FindDescendantByType<ScrollViewer>(mapListView)?.ScrollToTop();
        }

        private void MapListItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var model = (sender as FrameworkElement).DataContext as MapFileDisplayModel;
            if (!File.Exists(model.FilePath))
            {
                Substrate.ShowErrorMessage($"'{model.FileName}' no longer exists in this location.");
                return;
            }

            Substrate.HandlePhysicalFile(model.FilePath);
            mapListView.SelectedItem = null;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MapScanner.ScanForMaps();
            DataContext = MapLibraryModel.Build();
        }
    }
}
