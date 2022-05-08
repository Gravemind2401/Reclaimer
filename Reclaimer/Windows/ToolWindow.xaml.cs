using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for ToolyWindow.xaml
    /// </summary>
    public partial class ToolWindow : MetroWindow
    {
        public ToolWindow()
        {
            InitializeComponent();
        }

        private void ToolWindow_Closed(object sender, EventArgs e) => (Content as IDisposable)?.Dispose();
    }
}
