using Reclaimer.Plugins;
using Reclaimer.Utilities;
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
using System.Windows.Shapes;

namespace Reclaimer.Windows
{
    /// <summary>
    /// Interaction logic for OpenWithDialog.xaml
    /// </summary>
    public partial class OpenWithDialog : Window
    {
        private OpenFileArgs args;

        public ObservableCollection<Tuple<Plugin, string>> FileHandlers { get; }

        public Plugin SelectedPlugin => FileHandlers[list.SelectedIndex].Item1;

        private OpenWithDialog()
        {
            InitializeComponent();
            FileHandlers = new ObservableCollection<Tuple<Plugin, string>>();
            DataContext = this;
        }

        public static void HandleFile(IEnumerable<Plugin> handlers, OpenFileArgs args)
        {
            var window = new OpenWithDialog();

            if (!App.Settings.DefaultHandlers.ContainsKey(args.FileTypeKey))
                App.Settings.DefaultHandlers.Add(args.FileTypeKey, handlers.First().Key);

            window.LoadList(handlers, args);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void LoadList(IEnumerable<Plugin> handlers, OpenFileArgs args)
        {
            this.args = args;
            Title = $"Open With - {Utils.GetFileName(args.FileName)}";

            var defaultHandler = App.Settings.DefaultHandlers[args.FileTypeKey];

            FileHandlers.Clear();
            foreach (var p in handlers.OrderBy(p => p.Name))
            {
                var isDefault = p.Key == defaultHandler;
                var item = Tuple.Create(p, p.Name + (isDefault ? " (Default)" : string.Empty));

                FileHandlers.Add(item);

                if (isDefault)
                    list.SelectedIndex = FileHandlers.Count - 1;
            }
        }

        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedPlugin.OpenFile(args);
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedPlugin.OpenFile(args);
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.DefaultHandlers[args.FileTypeKey] = SelectedPlugin.Key;
            LoadList(FileHandlers.Select(t => t.Item1).ToList(), args);
        }
    }
}
