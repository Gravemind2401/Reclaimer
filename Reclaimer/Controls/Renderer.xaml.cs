using Adjutant.Spatial;
using Reclaimer.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Keys = System.Windows.Forms.Keys;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for Renderer.xaml
    /// </summary>
    public partial class Renderer : UserControl
    {
        private const double RAD_089 = 1.5706217940;
        private const double RAD_090 = 1.5707963268;
        private const double RAD_360 = 6.2831853072;

        #region Dependency Properties

        public static readonly DependencyProperty NearPlaneDistanceProperty =
            DependencyProperty.Register(nameof(NearPlaneDistance), typeof(double), typeof(Renderer), new PropertyMetadata(0.01));

        private static readonly DependencyPropertyKey MinFarPlaneDistancePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MinFarPlaneDistance), typeof(double), typeof(Renderer), new PropertyMetadata(200.0));

        public static readonly DependencyProperty MinFarPlaneDistanceProperty = MinFarPlaneDistancePropertyKey.DependencyProperty;

        public static readonly DependencyProperty FarPlaneDistanceProperty =
            DependencyProperty.Register(nameof(FarPlaneDistance), typeof(double), typeof(Renderer), new PropertyMetadata(1000.0));

        private static readonly DependencyPropertyKey MaxFarPlaneDistancePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MaxFarPlaneDistance), typeof(double), typeof(Renderer), new PropertyMetadata(5000.0));

        public static readonly DependencyProperty MaxFarPlaneDistanceProperty = MaxFarPlaneDistancePropertyKey.DependencyProperty;

        public static readonly DependencyProperty FieldOfViewProperty =
            DependencyProperty.Register(nameof(FieldOfView), typeof(double), typeof(Renderer), new PropertyMetadata(90.0));

        public static readonly DependencyProperty CameraSpeedProperty =
            DependencyProperty.Register(nameof(CameraSpeed), typeof(double), typeof(Renderer), new PropertyMetadata(0.015));

        private static readonly DependencyPropertyKey MaxCameraSpeedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MaxCameraSpeed), typeof(double), typeof(Renderer), new PropertyMetadata(1.5));

        public static readonly DependencyProperty MaxCameraSpeedProperty = MaxCameraSpeedPropertyKey.DependencyProperty;

        public double NearPlaneDistance
        {
            get { return (double)GetValue(NearPlaneDistanceProperty); }
            set { SetValue(NearPlaneDistanceProperty, value); }
        }

        public double MinFarPlaneDistance
        {
            get { return (double)GetValue(MinFarPlaneDistanceProperty); }
            private set { SetValue(MinFarPlaneDistancePropertyKey, value); }
        }

        public double FarPlaneDistance
        {
            get { return (double)GetValue(FarPlaneDistanceProperty); }
            set { SetValue(FarPlaneDistanceProperty, value); }
        }

        public double MaxFarPlaneDistance
        {
            get { return (double)GetValue(MaxFarPlaneDistanceProperty); }
            private set { SetValue(MaxFarPlaneDistancePropertyKey, value); }
        }

        public double FieldOfView
        {
            get { return (double)GetValue(FieldOfViewProperty); }
            set { SetValue(FieldOfViewProperty, value); }
        }

        public double CameraSpeed
        {
            get { return (double)GetValue(CameraSpeedProperty); }
            set { SetValue(CameraSpeedProperty, value); }
        }

        public double MaxCameraSpeed
        {
            get { return (double)GetValue(MaxCameraSpeedProperty); }
            private set { SetValue(MaxCameraSpeedPropertyKey, value); }
        }

        #endregion

        private readonly DispatcherTimer timer;

        private Point lastPoint;
        private double yaw, pitch;

        public Point3D MaxPosition { get; private set; } = new Point3D(500, 500, 500);
        public Point3D MinPosition { get; private set; } = new Point3D(-500, -500, -500);

        public Renderer()
        {
            InitializeComponent();

            DataContext = this;

            NormalizeSet();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }

        public void ScaleToContent(IEnumerable<Model3DGroup> content)
        {
            var bounds = new RealBounds3D
            {
                XBounds = new RealBounds
                {
                    Min = (float)content.Min(m => m?.Bounds.X),
                    Max = (float)content.Max(m => m?.Bounds.X + m?.Bounds.SizeX)
                },

                YBounds = new RealBounds
                {
                    Min = (float)content.Min(m => m?.Bounds.Y),
                    Max = (float)content.Max(m => m?.Bounds.Y + m?.Bounds.SizeY)
                },

                ZBounds = new RealBounds
                {
                    Min = (float)content.Min(m => m?.Bounds.Z),
                    Max = (float)content.Max(m => m?.Bounds.Z + m?.Bounds.SizeZ)
                }
            };

            ZoomToBounds(bounds);

            var len = bounds.Length;

            CameraSpeed = Math.Ceiling(len) / 1000;
            MaxCameraSpeed = Math.Ceiling(len * 5) / 1000;
            MaxPosition = new Point3D(
                bounds.XBounds.Max + len * 3,
                bounds.YBounds.Max + len * 3,
                bounds.ZBounds.Max + len * 3);
            MinPosition = new Point3D(
                bounds.XBounds.Min - len * 3,
                bounds.YBounds.Min - len * 3,
                bounds.ZBounds.Min - len * 3);

            MinFarPlaneDistance = 100;
            FarPlaneDistance = Math.Max(MinFarPlaneDistance, Math.Ceiling(len));
            MaxFarPlaneDistance = Math.Max(MinFarPlaneDistance, Math.Ceiling(len * 3));

        }

        public void LocateObject(Model3DGroup m)
        {
            var bounds = new RealBounds3D
            {
                XBounds = new RealBounds
                {
                    Min = (float)m.Bounds.X,
                    Max = (float)(m.Bounds.X + m.Bounds.SizeX)
                },

                YBounds = new RealBounds
                {
                    Min = (float)(m.Bounds.Y),
                    Max = (float)(m.Bounds.Y + m.Bounds.SizeY)
                },

                ZBounds = new RealBounds
                {
                    Min = (float)(m.Bounds.Z),
                    Max = (float)(m.Bounds.Z + m.Bounds.SizeZ)
                }
            };

            ZoomToBounds(bounds);
        }

        public void AddChild(ModelVisual3D child)
        {
            Viewport.Children.Add(child);
        }

        public void RemoveChild(ModelVisual3D child)
        {
            Viewport.Children.Remove(child);
        }

        public void ClearChildren()
        {
            Viewport.Children.Clear();
        }

        private void ZoomToBounds(RealBounds3D bounds)
        {
            var len = bounds.Length;

            if (bounds.XBounds.Length / 2 > bounds.YBounds.Length) //side view for long models like weapons
            {
                var p = new Point3D(
                    bounds.XBounds.Midpoint,
                    bounds.YBounds.Max + len * 0.5,
                    bounds.ZBounds.Midpoint);
                MoveCamera(p, new Vector3D(0, 0, -2));
            }
            else //normal camera position
            {
                var p = new Point3D(
                    bounds.XBounds.Max + len * 0.5,
                    bounds.YBounds.Midpoint,
                    bounds.ZBounds.Midpoint);
                MoveCamera(p, new Vector3D(-1, 0, 0));
            }
        }

        private void NormalizeSet()
        {
            var len = Camera.LookDirection.Length;
            Camera.LookDirection = new Vector3D(Camera.LookDirection.X / len, Camera.LookDirection.Y / len, Camera.LookDirection.Z / len);
            yaw = Math.Atan2(Camera.LookDirection.X, Camera.LookDirection.Z);
            pitch = Math.Atan(Camera.LookDirection.Y);
        }

        private void MoveCamera(Point3D Position, Vector3D Direction)
        {
            Camera.Position = Position;
            Camera.LookDirection = Direction;
            NormalizeSet();
        }

        private void UpdateCameraPosition()
        {
            if (!IsMouseCaptured) return;

            #region Set FOV
            if (CheckKeyState(Keys.NumPad6)) Camera.FieldOfView = ClipValue(Camera.FieldOfView + Camera.FieldOfView / 100.0, 45, 120);
            if (CheckKeyState(Keys.NumPad4)) Camera.FieldOfView = ClipValue(Camera.FieldOfView - Camera.FieldOfView / 100.0, 45, 120);
            #endregion

            #region Set FPD
            if (CheckKeyState(Keys.NumPad8)) Camera.FarPlaneDistance = ClipValue(Camera.FarPlaneDistance * 1.01, MinFarPlaneDistance, MaxFarPlaneDistance);
            if (CheckKeyState(Keys.NumPad2)) Camera.FarPlaneDistance = ClipValue(Camera.FarPlaneDistance * 0.99, MinFarPlaneDistance, MaxFarPlaneDistance);
            #endregion

            if (CheckKeyState(Keys.W) || CheckKeyState(Keys.A) || CheckKeyState(Keys.S) || CheckKeyState(Keys.D) || CheckKeyState(Keys.R) || CheckKeyState(Keys.F))
            {
                var nextPosition = Camera.Position;
                var len = Camera.LookDirection.Length;
                var lookDirection = Camera.LookDirection = new Vector3D(Camera.LookDirection.X / len, Camera.LookDirection.Y / len, Camera.LookDirection.Z / len);

                var dist = CameraSpeed;
                if (CheckKeyState(Keys.ShiftKey)) dist *= 3;
                if (CheckKeyState(Keys.Space)) dist /= 3;

                #region Check WASD

                if (CheckKeyState(Keys.W))
                {
                    nextPosition.X += lookDirection.X * dist;
                    nextPosition.Y += lookDirection.Y * dist;
                    nextPosition.Z += lookDirection.Z * dist;
                }

                if (CheckKeyState(Keys.A))
                {
                    nextPosition.X -= Math.Sin(yaw + RAD_090) * dist;
                    nextPosition.Y -= Math.Cos(yaw + RAD_090) * dist;
                }

                if (CheckKeyState(Keys.S))
                {
                    nextPosition.X -= lookDirection.X * dist;
                    nextPosition.Y -= lookDirection.Y * dist;
                    nextPosition.Z -= lookDirection.Z * dist;
                }

                if (CheckKeyState(Keys.D))
                {
                    nextPosition.X += Math.Sin(yaw + RAD_090) * dist;
                    nextPosition.Y += Math.Cos(yaw + RAD_090) * dist;
                }
                #endregion

                #region Check RF

                if (CheckKeyState(Keys.R))
                {
                    var upAxis = Vector3D.CrossProduct(Camera.LookDirection, Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection));
                    upAxis.Normalize();
                    nextPosition.X -= upAxis.X * dist;
                    nextPosition.Y -= upAxis.Y * dist;
                    nextPosition.Z -= upAxis.Z * dist;
                }

                if (CheckKeyState(Keys.F))
                {
                    var upAxis = Vector3D.CrossProduct(Camera.LookDirection, Vector3D.CrossProduct(Camera.LookDirection, Camera.UpDirection));
                    upAxis.Normalize();
                    nextPosition.X += upAxis.X * dist;
                    nextPosition.Y += upAxis.Y * dist;
                    nextPosition.Z += upAxis.Z * dist;
                }
                #endregion

                Camera.Position = new Point3D(
                    ClipValue(nextPosition.X, MinPosition.X, MaxPosition.X),
                    ClipValue(nextPosition.Y, MinPosition.Y, MaxPosition.Y),
                    ClipValue(nextPosition.Z, MinPosition.Z, MaxPosition.Z));
            }
        }

        private void UpdateCameraDirection(Point mousePos)
        {
            if (!IsMouseCaptured) return;
            if (lastPoint.Equals(mousePos)) return;

            var deltaX = mousePos.X - lastPoint.X;
            var deltaY = mousePos.Y - lastPoint.Y;

            yaw += deltaX * 0.01;
            pitch -= deltaY * 0.01;

            yaw %= RAD_360;
            pitch = ClipValue(pitch, -RAD_089, RAD_089);

            Camera.LookDirection = new Vector3D(Math.Sin(yaw), Math.Cos(yaw), Math.Tan(pitch));
            NativeMethods.SetCursorPos((int)lastPoint.X, (int)lastPoint.Y);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var CursorPos = new System.Drawing.Point();
            NativeMethods.GetCursorPos(out CursorPos);

            UpdateCameraPosition();
            UpdateCameraDirection(new Point(CursorPos.X, CursorPos.Y));
        }

        private void Renderer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            CaptureMouse();
            Cursor = Cursors.None;
            lastPoint = PointToScreen(e.GetPosition(this));
            lastPoint = new Point((int)lastPoint.X, (int)lastPoint.Y);
            timer.Start();
            e.Handled = true;
        }

        private void Renderer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            Cursor = Cursors.Cross;
            timer.Stop();
            e.Handled = true;
        }

        private void Renderer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) CameraSpeed = ClipValue(Math.Ceiling(CameraSpeed * 1050) / 1000, 0.001, MaxCameraSpeed);
            else CameraSpeed = ClipValue(Math.Floor(CameraSpeed * 0950) / 1000, 0.001, MaxCameraSpeed);
        }

        private static double ClipValue(double val, double min, double max)
        {
            return Math.Min(Math.Max(min, val), max);
        }

        private static bool CheckKeyState(Keys keys)
        {
            return ((NativeMethods.GetAsyncKeyState((int)keys) & 32768) != 0);
        }
    }
}
