using MahApps.Metro.Controls;
using Reclaimer.Models;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for TabWindow.xaml
    /// </summary>
    public partial class RaftedWindow : MetroWindow, ITabContentHost
    {
        DockContainerModel ITabContentHost.DockContainer => Model;
        DocumentPanelModel ITabContentHost.DocumentPanel => DocPanel;

        public DockContainerModel Model { get; }
        private DocumentPanelModel DocPanel { get; }

        public RaftedWindow()
        {
            InitializeComponent();

            Model = new DockContainerModel { Host = this };
            Model.Content = DocPanel = new DocumentPanelModel();
        }

        public RaftedWindow(DockContainerModel container, DocumentPanelModel panel)
        {
            InitializeComponent();

            Model = container;
            DocPanel = panel;
            Model.Host = this;
        }

        private void RaftedWindow_Closed(object sender, EventArgs e) => Model.Dispose();
    }
}
