using Microsoft.Win32;
using Reclaimer.Drawing;
using Reclaimer.Models;
using Reclaimer.Utilities;
using Studio.Controls;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : IDisposable
    {
        //private const double dpi = 96;

        private IBitmap bitmap;
        private DdsImage dds;
        private string sourceName;

        #region Dependency Properties
        private static readonly DependencyPropertyKey IndexesPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Indexes), typeof(IEnumerable<int>), typeof(BitmapViewer), new PropertyMetadata());

        public static readonly DependencyProperty IndexesProperty = IndexesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ImageSourcePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageSource), typeof(BitmapSource), typeof(BitmapViewer), new PropertyMetadata());

        public static readonly DependencyProperty ImageSourceProperty = ImageSourcePropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(BitmapViewer), new PropertyMetadata(0, SelectedIndexPropertyChanged));

        private static readonly DependencyPropertyKey HasMultiplePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasMultiple), typeof(bool), typeof(BitmapViewer), new PropertyMetadata(false, null, CoerceHasMultiple));

        public static readonly DependencyProperty HasMultipleProperty = HasMultiplePropertyKey.DependencyProperty;

        public static readonly DependencyProperty BlueChannelProperty =
            DependencyProperty.Register(nameof(BlueChannel), typeof(bool), typeof(BitmapViewer), new PropertyMetadata(true, ChannelPropertyChanged));

        public static readonly DependencyProperty GreenChannelProperty =
            DependencyProperty.Register(nameof(GreenChannel), typeof(bool), typeof(BitmapViewer), new PropertyMetadata(true, ChannelPropertyChanged));

        public static readonly DependencyProperty RedChannelProperty =
            DependencyProperty.Register(nameof(RedChannel), typeof(bool), typeof(BitmapViewer), new PropertyMetadata(true, ChannelPropertyChanged));

        public static readonly DependencyProperty AlphaChannelProperty =
            DependencyProperty.Register(nameof(AlphaChannel), typeof(bool), typeof(BitmapViewer), new PropertyMetadata(true, ChannelPropertyChanged));

        public IEnumerable<int> Indexes
        {
            get => (IEnumerable<int>)GetValue(IndexesProperty);
            private set => SetValue(IndexesPropertyKey, value);
        }

        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            private set => SetValue(ImageSourcePropertyKey, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public bool HasMultiple
        {
            get => (bool)GetValue(HasMultipleProperty);
            private set => SetValue(HasMultiplePropertyKey, value);
        }

        public bool BlueChannel
        {
            get => (bool)GetValue(BlueChannelProperty);
            set => SetValue(BlueChannelProperty, value);
        }

        public bool GreenChannel
        {
            get => (bool)GetValue(GreenChannelProperty);
            set => SetValue(GreenChannelProperty, value);
        }

        public bool RedChannel
        {
            get => (bool)GetValue(RedChannelProperty);
            set => SetValue(RedChannelProperty, value);
        }

        public bool AlphaChannel
        {
            get => (bool)GetValue(AlphaChannelProperty);
            set => SetValue(AlphaChannelProperty, value);
        }

        public static object CoerceHasMultiple(DependencyObject d, object baseValue) => (d as BitmapViewer)?.Indexes.Count() > 1;

        public static void ChannelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BitmapViewer)d;
            if (control.dds != null)
                control.Render();
        }

        public static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BitmapViewer)d;
            control.dds = control.bitmap.ToDds((int)e.NewValue);
            control.Render();
        }
        #endregion

        public TabModel TabModel { get; }

        public BitmapViewer()
        {
            InitializeComponent();
            TabModel = new TabModel(this, TabItemType.Document);
            DataContext = this;
        }

        public void LoadImage(IContentProvider<IBitmap> provider, string fileName)
        {
            bitmap = provider.GetContent();

            TabModel.ToolTip = fileName;
            TabModel.Header = Utils.GetFileName(fileName);
            sourceName = Utils.GetFileNameWithoutExtension(fileName);

            Indexes = Enumerable.Range(0, bitmap.SubmapCount);
            dds = bitmap.ToDds(0);
            Render();
            CoerceValue(HasMultipleProperty);
        }

        private void Render()
        {
            var src = dds.ToBitmapSource(new DdsOutputArgs(GetOptions(), bitmap.CubeLayout));
            if (src.CanFreeze)
                src.Freeze();
            ImageSource = src;
        }

        private DecompressOptions GetOptions()
        {
            var options = DecompressOptions.Default;
            if (!BlueChannel) options |= DecompressOptions.RemoveBlueChannel;
            if (!GreenChannel) options |= DecompressOptions.RemoveGreenChannel;
            if (!RedChannel) options |= DecompressOptions.RemoveRedChannel;
            if (!AlphaChannel) options |= DecompressOptions.RemoveAlphaChannel;
            return options;
        }

        private void ExportImage(bool allChannels)
        {
            var filter = "TIF Files|*.tif|PNG Files|*.png|JPEG Files|*.jpeg|TARGA Files|*.tga";
            if (allChannels)
                filter += "|DDS Files|*.dds";

            var sfd = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = sourceName,
                Filter = filter,
                FilterIndex = 1,
                AddExtension = true
            };

            if (sfd.ShowDialog() != true)
                return;

            var dir = Directory.GetParent(sfd.FileName).FullName;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (sfd.FilterIndex == 4)
            {
                var source = allChannels ? dds.ToBitmapSource() : ImageSource;
                source.WriteToTarga(sfd.FileName, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                return;
            }
            else if (sfd.FilterIndex == 5)
            {
                var dds = bitmap.ToDds(0);
                dds.WriteToDxgi(sfd.FileName);
                return;
            }

            using (var fs = new FileStream(sfd.FileName, FileMode.Create))
            {
                BitmapEncoder encoder = sfd.FilterIndex switch
                {
                    1 => new TiffBitmapEncoder(),
                    2 => new PngBitmapEncoder(),
                    3 => new JpegBitmapEncoder(),
                    _ => throw new InvalidOperationException()
                };
                var src = allChannels ? dds.ToBitmapSource() : ImageSource;
                encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Save(fs);
            }
        }

        #region Toolbar Events
        private void btnFitActual_Click(object sender, RoutedEventArgs e) => zoomPanel.ResetZoom();

        private void btnFitWindow_Click(object sender, RoutedEventArgs e)
        {
            var xScale = zoomPanel.ActualWidth / dds.Width;
            var yScale = zoomPanel.ActualHeight / dds.Height;

            zoomPanel.ResetZoom();
            zoomPanel.ZoomLevel = Math.Min(xScale, yScale);
        }

        private void btnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            var xScale = zoomPanel.ActualWidth / dds.Width;
            zoomPanel.ResetZoom();
            zoomPanel.ZoomLevel = xScale;
        }

        private void btnExportSelected_Click(object sender, RoutedEventArgs e) => ExportImage(false);
        private void btnExportAll_Click(object sender, RoutedEventArgs e) => ExportImage(true);
        #endregion

        public void Dispose()
        {
            DataContext = null;
            dds = null;
            bitmap = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
