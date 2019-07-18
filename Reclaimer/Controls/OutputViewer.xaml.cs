using Reclaimer.Plugins;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for OutputViewer.xaml
    /// </summary>
    public partial class OutputViewer : UtilityItem
    {
        #region Dependency Properties
        public static readonly DependencyProperty WordWrapEnabledProperty =
            DependencyProperty.Register(nameof(WordWrapEnabled), typeof(bool), typeof(OutputViewer), new PropertyMetadata(false, WordWrapEnabledChanged));

        public bool WordWrapEnabled
        {
            get { return (bool)GetValue(WordWrapEnabledProperty); }
            set { SetValue(WordWrapEnabledProperty, value); }
        }

        public static void WordWrapEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as OutputViewer;
            control.txtOutput.TextWrapping = control.WordWrapEnabled ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }
        #endregion

        public ObservableCollection<Tuple<string, string>> LoadedPlugins { get; }

        public OutputViewer()
        {
            TabHeader = "Output";
            TabToolTip = "Output";

            LoadedPlugins = new ObservableCollection<Tuple<string, string>>();

            var list = Substrate.AllPlugins.Select(p => Tuple.Create(p.Key, p.Name)).ToList();
            LoadedPlugins.Add(Tuple.Create(string.Empty, "All Plugins"));

            foreach (var p in list.OrderBy(t => t.Item2))
                LoadedPlugins.Add(p);

            InitializeComponent();
            DataContext = this;

            cmbPlugins.SelectedIndex = 0;
        }

        #region Event Handlers
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Substrate.Log += Substrate_Log;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Substrate.Log -= Substrate_Log;
        }

        private void cmbPlugins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var key = cmbPlugins.SelectedValue as string;

            var selection = key == string.Empty
                ? Substrate.AllPlugins
                : new[] { Substrate.GetPlugin(key) };

            var output = selection.SelectMany(p => p.logEntries)
                .OrderBy(p => p.Timestamp)
                .Select(p => p.Message);

            txtOutput.Clear();
            txtOutput.AppendText(string.Join(Environment.NewLine, output) + Environment.NewLine);
            txtOutput.CaretIndex = txtOutput.Text.Length;
            txtOutput.ScrollToEnd();
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var p in Substrate.AllPlugins)
                p.ClearLog();

            cmbPlugins_SelectionChanged(null, null);
        }

        private async void Substrate_Log(object sender, LogEventArgs e)
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    var key = cmbPlugins.SelectedValue as string;

                    if (key != string.Empty && key != e.Source.Key)
                        return;

                    txtOutput.AppendText(e.Entry.Message + Environment.NewLine);

                    if (txtOutput.CaretIndex == txtOutput.Text.Length)
                        txtOutput.ScrollToEnd();
                });
            }
            catch (TaskCanceledException) { }
        }
        #endregion
    }
}
