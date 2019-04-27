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

        public void LoadImage(IBitmap image)
        {
            try
            {
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

            for (int i = 0; i < RenderData.Length; i++)
            {
                if (i % 4 == 0 && !BlueChannel)
                    RenderData[i] = 0;
                else if (i % 4 == 1 && !GreenChannel)
                    RenderData[i] = 0;
                else if (i % 4 == 2 && !RedChannel)
                    RenderData[i] = 0;
                else if (i % 4 == 3 && !AlphaChannel)
                    RenderData[i] = byte.MaxValue;
            }

            var src = BitmapSource.Create(PixelWidth, PixelHeight, dpi, dpi, PixelFormats.Bgra32, null, RenderData, PixelStride);
            if (src.CanFreeze) src.Freeze();

            ImageSource = src;
        }

        #region ITabContent
        public object Header => nameof(BitmapViewer);

        public object Icon => null;

        public TabItemUsage Usage => TabItemUsage.Document;

        //public object ToolTip => base.ToolTip;
        #endregion
    }
}
