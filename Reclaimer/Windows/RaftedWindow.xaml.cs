using MahApps.Metro.Controls;
using Reclaimer.Models;
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
    public partial class RaftedWindow : MetroWindow, ITabContentHost
    {
        DockContainerModel ITabContentHost.DockContainer => Model;
        DocumentPanelModel ITabContentHost.DocumentPanel => DocPanel;

        public DockContainerModel Model { get; }
        private DocumentPanelModel DocPanel { get; }

        public RaftedWindow()
        {
            InitializeComponent();

            Model = new DockContainerModel();
            Model.Content = DocPanel = new DocumentPanelModel();
        }

        public RaftedWindow(DockContainerModel container, DocumentPanelModel panel)
        {
            InitializeComponent();

            Model = container;
            DocPanel = panel;
        }

        private void RaftedWindow_Closed(object sender, EventArgs e)
        {
            Model.Dispose();
        }
    }
}
