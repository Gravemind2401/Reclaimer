using Adjutant.Blam.Common;
using Reclaimer.Plugins;
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
    public partial class MapViewer : UtilityItem
    {
        private ICacheFile cache;

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

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithDefault(args);
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithDefault(args);
        }

        private void menuOpenWith_Click(object sender, RoutedEventArgs e)
        {
            var item = (tv.SelectedItem as TreeNode)?.Tag as IIndexItem;
            if (item == null) return;

            var fileName = $"{Path.GetFileName(item.FileName)}.{item.ClassName}";
            var fileKey = $"{cache.CacheType}.{item.ClassCode}";
            var args = new OpenFileArgs(fileName, item, fileKey, Substrate.GetHostWindow(this));
            Substrate.OpenWithPrompt(args);
        }
    }
}
