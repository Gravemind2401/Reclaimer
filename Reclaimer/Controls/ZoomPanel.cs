using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Reclaimer.Controls
{
    public class ZoomPanel : Border
    {
        private static readonly Cursor grabCursor;
        private static readonly Cursor grabbingCursor;

        private readonly TransformGroup group = new TransformGroup();
        private readonly TranslateTransform translate = new TranslateTransform();
        private readonly ScaleTransform scale = new ScaleTransform();
        private Point dragStart;

        private Transform prevTransform;

        #region Dependency Properties
        public static DependencyProperty MinZoomProperty =
            DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(ZoomPanel), new PropertyMetadata(0.25));

        public static DependencyProperty MaxZoomProperty =
            DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(ZoomPanel), new PropertyMetadata(4.0));

        public static DependencyProperty ZoomVarianceProperty =
            DependencyProperty.Register(nameof(ZoomVariance), typeof(double), typeof(ZoomPanel), new PropertyMetadata(0.1));

        public static DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(ZoomPanel), new PropertyMetadata(1.0, ZoomLevelPropertyChanged));

        public double MinZoom
        {
            get => (double)GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public double MaxZoom
        {
            get => (double)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        public double ZoomVariance
        {
            get => (double)GetValue(ZoomVarianceProperty);
            set => SetValue(ZoomVarianceProperty, value);
        }

        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set
            {
                value = Math.Min(Math.Max(MinZoom, value), MaxZoom);
                SetValue(ZoomLevelProperty, value);
            }
        }

        public static void ZoomLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ZoomPanel)d;
            control.scale.ScaleX = control.scale.ScaleY = (double)e.NewValue;
        }
        #endregion

        static ZoomPanel()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(ZoomPanel), new FrameworkPropertyMetadata(true));
            BackgroundProperty.OverrideMetadata(typeof(ZoomPanel), new FrameworkPropertyMetadata(Brushes.Transparent));

            try
            {
                var grabStream = Application.GetResourceStream(new Uri("Resources\\grab.cur", UriKind.Relative));
                var grabbingStream = Application.GetResourceStream(new Uri("Resources\\grabbing.cur", UriKind.Relative));

                grabCursor = new Cursor(grabStream.Stream);
                grabbingCursor = new Cursor(grabbingStream.Stream);
            }
            catch //the designer view doesn't like this but it works at runtime
            {
                grabCursor = grabbingCursor = Cursors.Arrow;
            }
        }

        public ZoomPanel()
        {
            group.Children.Add(scale);
            group.Children.Add(translate);
            Cursor = grabCursor;
        }

        public void ResetZoom()
        {
            ZoomLevel = 1;
            translate.X = translate.Y = 0;
        }

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                OnChildChanged(base.Child, value);
                base.Child = value;
            }
        }

        private void OnChildChanged(UIElement prev, UIElement next)
        {
            if (prev != null)
                prev.RenderTransform = prevTransform;

            if (next != null)
            {
                prevTransform = next.RenderTransform;
                next.RenderTransform = group;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            dragStart = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            Cursor = grabbingCursor;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Child == null)
                return;

            var multiplier = e.Delta > 0 ? 1 + ZoomVariance : 1 - ZoomVariance;

            var transformOrigin = new Point(Child.DesiredSize.Width * Child.RenderTransformOrigin.X, Child.DesiredSize.Height * Child.RenderTransformOrigin.Y);

            var relative = e.GetPosition(Child) - transformOrigin;
            var origin = new Point(relative.X * ZoomLevel, relative.Y * ZoomLevel);

            ZoomLevel *= multiplier;

            var dest = new Point(relative.X * ZoomLevel, relative.Y * ZoomLevel);
            var offset = dest - origin;

            translate.X -= offset.X;
            translate.Y -= offset.Y;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            Cursor = grabCursor;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsMouseCaptured)
                return;

            var pos = e.GetPosition(this);
            var delta = dragStart - pos;

            translate.X -= delta.X;
            translate.Y -= delta.Y;

            dragStart = pos;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Child?.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var x = finalSize.Width / 2 - Child.DesiredSize.Width / 2;
            var y = finalSize.Height / 2 - Child.DesiredSize.Height / 2;

            Child?.Arrange(new Rect(x, y, Child.DesiredSize.Width, Child.DesiredSize.Height));
            return finalSize;
        }
    }
}
