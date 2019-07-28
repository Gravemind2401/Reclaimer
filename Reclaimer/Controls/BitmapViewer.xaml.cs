using Adjutant.Utilities;
using Microsoft.Win32;
using Reclaimer.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Dds;
using System.IO;
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

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : DocumentItem
    {
        private const double dpi = 96;

        private byte[] imageData;
        private byte[] renderData;

        private IBitmap bitmap;
        private string sourceName;
        private int pixelWidth, pixelHeight, pixelStride;

        public IEnumerable<int> Indexes { get; private set; }

        #region Dependency Properties
        public static readonly DependencyPropertyKey ImageSourcePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageSource), typeof(BitmapSource), typeof(BitmapViewer), new PropertyMetadata());

        public static readonly DependencyProperty ImageSourceProperty = ImageSourcePropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(BitmapViewer), new PropertyMetadata(0, SelectedIndexPropertyChanged));

        public static readonly DependencyPropertyKey HasMultiplePropertyKey =
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

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            private set { SetValue(ImageSourcePropertyKey, value); }
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public bool HasMultiple
        {
            get { return (bool)GetValue(HasMultipleProperty); }
            private set { SetValue(HasMultiplePropertyKey, value); }
        }

        public bool BlueChannel
        {
            get { return (bool)GetValue(BlueChannelProperty); }
            set { SetValue(BlueChannelProperty, value); }
        }

        public bool GreenChannel
        {
            get { return (bool)GetValue(GreenChannelProperty); }
            set { SetValue(GreenChannelProperty, value); }
        }

        public bool RedChannel
        {
            get { return (bool)GetValue(RedChannelProperty); }
            set { SetValue(RedChannelProperty, value); }
        }

        public bool AlphaChannel
        {
            get { return (bool)GetValue(AlphaChannelProperty); }
            set { SetValue(AlphaChannelProperty, value); }
        }

        public static object CoerceHasMultiple(DependencyObject d, object baseValue)
        {
            return (d as BitmapViewer)?.Indexes.Count() > 1;
        }

        public static void ChannelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BitmapViewer)d;
            if (control.imageData != null)
                control.Render();
        }

        public static void SelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BitmapViewer)d;
            control.SetImage((int)e.NewValue);
        }
        #endregion

        public BitmapViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadImage(IBitmap image, string fileName)
        {
            try
            {
                bitmap = image;

                TabToolTip = fileName;
                TabHeader = Utils.GetFileName(fileName);
                sourceName = Utils.GetFileNameWithoutExtension(fileName);

                Indexes = Enumerable.Range(0, bitmap.SubmapCount);
                SetImage(0);
                RaisePropertyChanged(nameof(Indexes));
                CoerceValue(HasMultipleProperty);
            }
            catch
            {
                
            }
        }

        private void SetImage(int index)
        {
            var dds = bitmap.ToDds(index);
            var src = dds.ToBitmapSource(DecompressOptions.UnwrapCubemap);

            pixelWidth = src.PixelWidth;
            pixelHeight = src.PixelHeight;
            pixelStride = src.PixelWidth * 4;

            imageData = new byte[pixelStride * pixelHeight];
            src.CopyPixels(imageData, pixelStride, 0);

            renderData = new byte[pixelStride * pixelHeight];
            Render();
        }

        private void Render()
        {
            Array.Copy(imageData, renderData, imageData.Length);

            var singleChannel = -1;
            if (Convert.ToInt32(BlueChannel) + Convert.ToInt32(GreenChannel) + Convert.ToInt32(RedChannel) + Convert.ToInt32(AlphaChannel) == 1)
            {
                if (BlueChannel) singleChannel = 0;
                else if (GreenChannel) singleChannel = 1;
                else if (RedChannel) singleChannel = 2;
                else if (AlphaChannel) singleChannel = 3;
            }

            for (int i = 0; i < renderData.Length; i += 4)
            {
                //if only one channel is selected treat it as greyscale
                if (singleChannel >= 0)
                {
                    renderData[i + 0] = renderData[i + 1] = renderData[i + 2] = renderData[i + singleChannel];
                    renderData[i + 3] = byte.MaxValue;
                }
                else
                {
                    if (!BlueChannel) renderData[i] = 0;
                    if (!GreenChannel) renderData[i + 1] = 0;
                    if (!RedChannel) renderData[i + 2] = 0;
                    if (!AlphaChannel) renderData[i + 3] = byte.MaxValue;
                }
            }

            var src = BitmapSource.Create(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Bgra32, null, renderData, pixelStride);
            if (src.CanFreeze) src.Freeze();

            ImageSource = src;
        }

        private void ExportImage(bool allChannels)
        {
            var filter = "TIF Files|*.tif|PNG Files|*.png";
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

            if (sfd.FilterIndex == 3)
            {
                var dds = bitmap.ToDds(0);
                dds.WriteToDisk(sfd.FileName);
                return;
            }

            using (var fs = new FileStream(sfd.FileName, FileMode.Create))
            {
                var encoder = sfd.FilterIndex == 1
                    ? (BitmapEncoder)new TiffBitmapEncoder()
                    : new PngBitmapEncoder();

                var src = allChannels ? bitmap.ToDds(0).ToBitmapSource() : ImageSource;
                encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Save(fs);
            }
        }

        #region Toolbar Events
        private void btnFitActual_Click(object sender, RoutedEventArgs e)
        {
            zoomPanel.ResetZoom();
        }

        private void btnFitWindow_Click(object sender, RoutedEventArgs e)
        {
            var xScale = zoomPanel.ActualWidth / pixelWidth;
            var yScale = zoomPanel.ActualHeight / pixelHeight;

            zoomPanel.ResetZoom();
            zoomPanel.ZoomLevel = Math.Min(xScale, yScale);
        }

        private void btnFitWidth_Click(object sender, RoutedEventArgs e)
        {
            var xScale = zoomPanel.ActualWidth / pixelWidth;

            zoomPanel.ResetZoom();
            zoomPanel.ZoomLevel = xScale;
        }

        private void btnExportSelected_Click(object sender, RoutedEventArgs e)
        {
            ExportImage(false);
        }

        private void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            ExportImage(true);
        }
        #endregion
    }
}
