using Reclaimer.Models;
using Reclaimer.Plugins.MapBrowser;
using Studio.Controls;
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
    }
}
