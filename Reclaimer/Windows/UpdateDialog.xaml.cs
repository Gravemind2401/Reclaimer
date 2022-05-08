using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        private UpdateDialog()
        {
            InitializeComponent();
            DataContext = App.Settings.LatestRelease;
        }

        public static void ShowUpdate()
        {
            if (App.Settings.LatestRelease == null)
                return;

            var window = new UpdateDialog();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start(App.Settings.LatestRelease.DetailsUrl);
        private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
