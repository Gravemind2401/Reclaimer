using Adjutant.Utilities;
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
using System.Windows.Shapes;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : UserControl, ITabContent
    {
        private const double dpi = 96;

        public BitmapViewer()
        {
            InitializeComponent();
        }

        #region ITabContent
        public object Header => nameof(BitmapViewer);

        public object Icon => null;

        public TabItemUsage Usage => TabItemUsage.Document;
        #endregion

        public void LoadImage(IBitmap image)
        {
            try
            {
                var dds = image.ToDds(0);
                img.Source = dds.ToBitmapSource(dpi);
            }
            catch
            {

            }
        }
    }
}
