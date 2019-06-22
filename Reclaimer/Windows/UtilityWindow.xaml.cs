using MahApps.Metro.Controls;
using Studio.Controls;
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
using System.Windows.Shapes;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for UtilityWindow.xaml
    /// </summary>
    public partial class UtilityWindow : MetroWindow, ITabWindow
    {
        public UtilityWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //ignore any SizeChanged events before Loaded
            //first SizeChanged event after Loaded will be when it sizes to content
            SizeChanged += MetroWindow_SizeChanged;
        }

        private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //set SizeToContent back to normal and reset the main grid size
            //this allows the grid to once again stretch to fit the window
            root.Width = double.NaN;
            root.Height = double.NaN;

            SizeToContent = SizeToContent.Manual;
            Width = e.NewSize.Width;
            Height = e.NewSize.Height;

            //only need to do this once
            SizeChanged -= MetroWindow_SizeChanged;
        }

        #region ITabWindow
        IEnumerable<ITabContent> ITabWindow.TargetItems => tabControl.Items.OfType<ITabContent>();

        void ITabWindow.Initialize(DetachEventArgs e)
        {
            var visualBounds = e.VisualBounds;

            Left = visualBounds.Left;
            Top = visualBounds.Top;

            SizeToContent = SizeToContent.WidthAndHeight;

            root.Width = visualBounds.Width;
            root.Height = visualBounds.Height;

            foreach (var item in e.TabItems)
                tabControl.Items.Add(item);
        }

        void ITabWindow.OnDragStop()
        {

        }

        void ITabWindow.OnConsumed()
        {
            Close();
        }

        void ITabWindow.OnTabControlEmpty(ExtendedTabControl tabControl)
        {
            Close();
        }
        #endregion
    }
}
