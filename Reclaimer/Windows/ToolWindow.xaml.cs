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
    public partial class ToolWindow : MetroWindow
    {
        public ToolWindow()
        {
            InitializeComponent();
        }

        private void ToolWindow_Closed(object sender, EventArgs e)
        {
            (Content as IDisposable)?.Dispose();
        }
    }
}
