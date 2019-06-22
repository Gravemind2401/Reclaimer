using Adjutant.Blam.Common;
using Reclaimer.Utils;
using Reclaimer.Windows;
using Studio.Controls;
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

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for MapViewer.xaml
    /// </summary>
    public partial class MapViewer : ControlBase, ITabContent
    {
        private ICacheFile cache;

        TabItemUsage ITabContent.TabUsage => TabItemUsage.Utility;

        public MapViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadMap(string fileName)
        {
            cache = CacheFactory.ReadCacheFile(fileName);

            TabHeader = Path.GetFileName(cache.FileName);
            TabToolTip = $"Map Viewer - {TabHeader}";

            tv.Items.Clear();

            var result = cache.TagIndex.GroupBy(i => i.ClassName);

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
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;

            if (item == null) return;

            if (item.ClassCode == "sbsp")
            {
                Adjutant.Utilities.IRenderGeometry sbsp;
                switch (cache.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo1.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo2Xbox:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo2.scenario_structure_bsp>();
                        break;
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        sbsp = item.ReadMetadata<Adjutant.Blam.Halo3.scenario_structure_bsp>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new ModelViewer();
                viewer.LoadGeometry(sbsp, $"{item.FileName}.{item.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }

            if (item.ClassCode == "mod2" || item.ClassCode == "mode")
            {
                Adjutant.Utilities.IRenderGeometry mode;
                switch (cache.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo1.gbxmodel>();
                        break;
                    case CacheType.Halo2Xbox:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo2.render_model>();
                        break;
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        mode = item.ReadMetadata<Adjutant.Blam.Halo3.render_model>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new ModelViewer();
                viewer.LoadGeometry(mode, $"{item.FileName}.{item.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }

            if (item.ClassCode == "bitm")
            {
                Adjutant.Utilities.IBitmap bitm;
                switch (cache.CacheType)
                {
                    case CacheType.Halo1CE:
                    case CacheType.Halo1PC:
                        bitm = item.ReadMetadata<Adjutant.Blam.Halo1.bitmap>();
                        break;
                    case CacheType.Halo2Xbox:
                        bitm = item.ReadMetadata<Adjutant.Blam.Halo2.bitmap>();
                        break;
                    case CacheType.Halo3Beta:
                    case CacheType.Halo3Retail:
                    case CacheType.Halo3ODST:
                        bitm = item.ReadMetadata<Adjutant.Blam.Halo3.bitmap>();
                        break;
                    default: throw new NotSupportedException();
                }

                var viewer = new BitmapViewer();
                viewer.LoadImage(bitm, $"{item.FileName}.{item.ClassCode}");

                var wnd = Application.Current.MainWindow as MainWindow;
                wnd.docTab.Items.Add(viewer);
                return;
            }
        }
    }
}
