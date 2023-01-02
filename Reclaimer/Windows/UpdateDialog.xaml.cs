using Reclaimer.Utilities;
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

        private void btnOk_Click(object sender, RoutedEventArgs e) => Utils.StartProcess(App.Settings.LatestRelease.DetailsUrl);
        private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
