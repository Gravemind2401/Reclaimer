using Adjutant.Blam.Common;
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
using Reclaimer.Utils;
using Reclaimer.Windows;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for TagViewer.xaml
    /// </summary>
    public partial class TagViewer : UtilityItem
    {
        private volatile bool isBusy;

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

        #endregion

        public TagViewer()
        {
            InitializeComponent();
            DataContext = this;

            TabHeader = TabToolTip = "Tag Viewer";
        }

        public async Task Refresh()
        {
            isBusy = true;
            AllGames = await Storage.CacheFiles.Select(c => c.CacheType).Distinct().ToListAsync();
            SelectedGame = AllGames.FirstOrDefault();
            LoadTags();
            isBusy = false;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async void cmbGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isBusy = true;

            AllBuilds = await Storage.CacheFiles
                .Where(c => c.CacheType == SelectedGame)
                .Select(c => c.BuildString)
                .Distinct()
                .ToListAsync();

            SelectedBuild = AllBuilds.First();

            cmbBuild_SelectionChanged(null, null);
            LoadTags();

            isBusy = false;
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
            if (!isBusy)
                LoadTags();
        }

        private void LoadTags()
        {
            tv.Items.Clear();
            var items = Storage.IndexItemsFor(SelectedGame, SelectedBuild, SelectedMapId);

            var result = items.GroupBy(i => i.ClassCode);

            foreach (var g in result.OrderBy(g => g.Key))
            {
                var node = new TreeNode { Header = g.Key };
                foreach (var i in g.OrderBy(i => i.FileName))
                {
                    node.Children.Add(new TreeNode
                    {
                        Header = i.FileName,
                        Tag = i
                    });
                }
                tv.Items.Add(node);
            }
        }

        private void tv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as Entities.TagItem;

            if (item == null) return;

            if (item.ClassCode == "sbsp")
            {
                var cache = Storage.CacheFiles.First(c => c.CacheId == item.CacheId);

                var map = CacheFactory.ReadCacheFile(cache.FileName);
                var tag = map.TagIndex[(int)item.TagId];

                Adjutant.Utilities.IRenderGeometry sbsp;
                switch (map.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        sbsp = tag.ReadMetadata<Adjutant.Blam.Halo1.scenario_structure_bsp>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new ModelViewer();
                viewer.LoadGeometry(sbsp, $"{tag.FileName}.{tag.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }

            if (item.ClassCode == "mod2" || item.ClassCode == "mode")
            {
                var cache = Storage.CacheFiles.First(c => c.CacheId == item.CacheId);

                var map = CacheFactory.ReadCacheFile(cache.FileName);
                var tag = map.TagIndex[(int)item.TagId];

                Adjutant.Utilities.IRenderGeometry mode;
                switch (map.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        mode = tag.ReadMetadata<Adjutant.Blam.Halo1.gbxmodel>();
                        break;
                    case CacheType.Halo2Xbox:
                        mode = tag.ReadMetadata<Adjutant.Blam.Halo2.render_model>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new ModelViewer();
                viewer.LoadGeometry(mode, $"{tag.FileName}.{tag.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }

            if (item.ClassCode == "bitm")
            {
                var cache = Storage.CacheFiles.First(c => c.CacheId == item.CacheId);

                var map = CacheFactory.ReadCacheFile(cache.FileName);
                var tag = map.TagIndex[(int)item.TagId];

                Adjutant.Utilities.IBitmap bitm;
                switch (map.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        bitm = tag.ReadMetadata<Adjutant.Blam.Halo1.bitmap>();
                        break;
                    case CacheType.Halo2Xbox:
                        bitm = tag.ReadMetadata<Adjutant.Blam.Halo2.bitmap>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new BitmapViewer();
                viewer.LoadImage(bitm, $"{tag.FileName}.{tag.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }
        }
    }
}
