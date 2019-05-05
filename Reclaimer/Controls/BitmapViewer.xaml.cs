using Adjutant.Utilities;
using Studio.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for BitmapViewer.xaml
    /// </summary>
    public partial class BitmapViewer : UserControl, ITabContent
    {
        private const double dpi = 96;

        private byte[] ImageData;
        private byte[] RenderData;

        private int PixelWidth, PixelHeight, PixelStride;

        #region Dependency Properties
        public static readonly DependencyPropertyKey ImageSourcePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageSource), typeof(BitmapSource), typeof(BitmapViewer), new PropertyMetadata());

        public static readonly DependencyProperty ImageSourceProperty = ImageSourcePropertyKey.DependencyProperty;

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

        public static void ChannelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BitmapViewer)d;
            if (control.ImageData != null)
                control.Render();
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
                TabToolTip = fileName;
                TabHeader = System.IO.Path.GetFileName(fileName);

                var dds = image.ToDds(0);
                var src = dds.ToBitmapSource();

                PixelWidth = src.PixelWidth;
                PixelHeight = src.PixelHeight;
                PixelStride = src.PixelWidth * 4;

                ImageData = new byte[PixelStride * PixelHeight];
                src.CopyPixels(ImageData, PixelStride, 0);

                RenderData = new byte[PixelStride * PixelHeight];
                Render();
            }
            catch
            {

            }
        }

        private void Render()
        {
            Array.Copy(ImageData, RenderData, ImageData.Length);

            var singleChannel = -1;
            if (Convert.ToInt32(BlueChannel) + Convert.ToInt32(GreenChannel) + Convert.ToInt32(RedChannel) + Convert.ToInt32(AlphaChannel) == 1)
            {
                if (BlueChannel) singleChannel = 0;
                else if (GreenChannel) singleChannel = 1;
                else if (RedChannel) singleChannel = 2;
                else if (AlphaChannel) singleChannel = 3;
            }

            for (int i = 0; i < RenderData.Length; i += 4)
            {
                //if only one channel is selected treat it as greyscale
                if (singleChannel >= 0)
                {
                    RenderData[i + 0] = RenderData[i + 1] = RenderData[i + 2] = RenderData[i + singleChannel];
                    RenderData[i + 3] = byte.MaxValue;
                }
                else
                {
                    if (!BlueChannel) RenderData[i] = 0;
                    if (!GreenChannel) RenderData[i + 1] = 0;
                    if (!RedChannel) RenderData[i + 2] = 0;
                    if (!AlphaChannel) RenderData[i + 3] = byte.MaxValue;
                }
            }

            var src = BitmapSource.Create(PixelWidth, PixelHeight, dpi, dpi, PixelFormats.Bgra32, null, RenderData, PixelStride);
            if (src.CanFreeze) src.Freeze();

            ImageSource = src;
        }

        #region ITabContent
        public object TabHeader { get; private set; }

        public object TabToolTip { get; private set; }

        public object TabIcon => null;

        TabItemUsage ITabContent.TabUsage => TabItemUsage.Document;
        #endregion
    }
}
