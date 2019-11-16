using MahApps.Metro.Controls;
using Studio.Controls;
using Studio.Utilities;
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
    /// Interaction logic for TabWindow.xaml
    /// </summary>
    public partial class TabWindow : MetroWindow//, IMultiPanelHost//, ITabWindow
    {
        //private ExtendedTabControl tempControl;
        //private ITabContent tempItem;

        #region Dependency Properties
        public static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsDragging), typeof(bool), typeof(TabWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            private set { SetValue(IsDraggingPropertyKey, value); }
        }
        #endregion

        public TabWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //SizeToContent = SizeToContent.Manual;
            //root.Width = root.Height = double.NaN;
            //
            //var tab = (FrameworkElement)tempControl.ItemContainerGenerator.ContainerFromIndex(0);
            //var curPos = tab.GetRelativeMousePosition();
            //Left += curPos.X - tab.ActualWidth / 2;
            //Top += curPos.Y - tab.ActualHeight / 2;
        }

        //MultiPanel IMultiPanelHost.MultiPanel => contentPanel;

        //DocumentTabControl IMultiPanelHost.DocumentContainer => tempControl as DocumentTabControl;

        #region ITabWindow
        //IEnumerable<ITabContent> ITabWindow.TargetItems =>
        //    contentPanel.GetChildren()
        //    .OfType<ExtendedTabControl>()
        //    .SelectMany(c => c.Items.OfType<ITabContent>())
        //    .Concat(dockContainer.AllDockItems.OfType<ITabContent>());
        //
        //void ITabWindow.Initialize(DetachEventArgs e)
        //{
        //    var visualBounds = e.VisualBounds;
        //
        //    Left = visualBounds.Left;
        //    Top = visualBounds.Top;
        //
        //    SizeToContent = SizeToContent.WidthAndHeight;
        //
        //    tempItem = e.TabItems.First();
        //
        //    //if (tempItem.TabUsage == TabItemUsage.Document)
        //    tempControl = new DocumentTabControl();
        //    //else
        //    //    tempControl = new UtilityTabControl();
        //
        //    root.Width = visualBounds.Width;
        //    root.Height = visualBounds.Height;
        //
        //    foreach (var item in e.TabItems)
        //        tempControl.Items.Add(item);
        //
        //    tempControl.RemoveOnEmpty = false;
        //    contentPanel.AddElement(tempControl, null, Dock.Top);
        //
        //    IsDragging = true;
        //}
        //
        //void ITabWindow.OnDragStop()
        //{
        //    IsDragging = false;
        //}
        //
        //void ITabWindow.OnConsumed()
        //{
        //    Close();
        //}
        //
        //void ITabWindow.OnTabControlEmpty(ExtendedTabControl tabControl)
        //{
        //    var tabControls = contentPanel.GetChildren()
        //        .Where(c => c is ExtendedTabControl)
        //        .Cast<ExtendedTabControl>();
        //
        //    if (!tabControls.Any(c => c.HasItems))
        //        Close();
        //}
        #endregion
    }
}
