using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for TagViewer.xaml
    /// </summary>
    public partial class TagViewer : ControlBase, ITabContent
    {
        #region Properties

        private IEnumerable<CacheType> allGames;
        public IEnumerable<CacheType> AllGames
        {
            get { return allGames; }
            set { SetProperty(ref allGames, value); }
        }

        private IEnumerable<string> allBuilds;
        public IEnumerable<string> AllBuilds
        {
            get { return allBuilds; }
            set { SetProperty(ref allBuilds, value); }
        }

        private IEnumerable<Tuple<int, string>> allMaps;
        public IEnumerable<Tuple<int, string>> AllMaps
        {
            get { return allMaps; }
            set { SetProperty(ref allMaps, value); }
        }

        private CacheType selectedGame;
        public CacheType SelectedGame
        {
            get { return selectedGame; }
            set { SetProperty(ref selectedGame, value); }
        }

        private string selectedBuild;
        public string SelectedBuild
        {
            get { return selectedBuild; }
            set { SetProperty(ref selectedBuild, value); }
        }

        private int selectedMapId;
        public int SelectedMapId
        {
            get { return selectedMapId; }
            set { SetProperty(ref selectedMapId, value); }
        }

        #region ITabContent

        TabItemUsage ITabContent.Usage => TabItemUsage.Utility;

        public object Header => "Tag Viewer";

        //object ITabContent.ToolTip => "Tag Viewer";

        object ITabContent.Icon => null; 

        #endregion

        #endregion

        public TagViewer()
        {
            InitializeComponent();
            DataContext = this;
            ToolTip = Header;
        }

        public async Task Refresh()
        {
            AllGames = await Storage.CacheFiles.Select(c => c.CacheType).Distinct().ToListAsync();
            SelectedGame = AllGames.FirstOrDefault();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async void cmbGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllBuilds = await Storage.CacheFiles
                .Where(c => c.CacheType == SelectedGame)
                .Select(c => c.BuildString)
                .Distinct()
                .ToListAsync();

            SelectedBuild = AllBuilds.First();
        }

        private void cmbBuild_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var temp = Storage.CacheFiles
                .Where(c => c.CacheType == SelectedGame && c.BuildString == SelectedBuild)
                .ToList()
                .Select(c => Tuple.Create((int)c.CacheId, System.IO.Path.GetFileNameWithoutExtension(c.FileName)))
                .ToList();

            temp.Insert(0, Tuple.Create(-1, "All"));

            AllMaps = temp;

            SelectedMapId = -1;
        }

        private void cmbMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
