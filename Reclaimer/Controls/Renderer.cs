using Adjutant.Spatial;
using Reclaimer.Geometry;
using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using Numerics = System.Numerics;
using Keys = System.Windows.Forms.Keys;

namespace Reclaimer.Controls
{
    [TemplatePart(Name = PART_Viewport, Type = typeof(FrameworkElement))]
    public class Renderer : Control, IDisposable
    {
        private const string PART_Viewport = "PART_Viewport";

        private const double SpeedMultipler = 0.0013;
        private const double MinFarPlaneDistance = 50;
        private const double MinNearPlaneDistance = 0.01;

        #region Dependency Properties

        public static readonly DependencyPropertyKey ViewportPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Viewport), typeof(Viewport3D), typeof(Renderer), new PropertyMetadata((object)null));

        public static readonly DependencyProperty ViewportProperty = ViewportPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey YawPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Yaw), typeof(double), typeof(Renderer), new PropertyMetadata(0.0));

        public static readonly DependencyProperty YawProperty = YawPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PitchPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Pitch), typeof(double), typeof(Renderer), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PitchProperty = PitchPropertyKey.DependencyProperty;

        public static readonly DependencyProperty CameraSpeedProperty =
            DependencyProperty.Register(nameof(CameraSpeed), typeof(double), typeof(Renderer), new PropertyMetadata(0.015));

        private static readonly DependencyPropertyKey MaxCameraSpeedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(MaxCameraSpeed), typeof(double), typeof(Renderer), new PropertyMetadata(1.5));

        public static readonly DependencyProperty MaxCameraSpeedProperty = MaxCameraSpeedPropertyKey.DependencyProperty;

        public Viewport3D Viewport
        {
            get => (Viewport3D)GetValue(ViewportProperty);
            private set => SetValue(ViewportPropertyKey, value);
        }

        public double Yaw
        {
            get => (double)GetValue(YawProperty);
            private set => SetValue(YawPropertyKey, value);
        }

        public double Pitch
        {
            get => (double)GetValue(PitchProperty);
            private set => SetValue(PitchPropertyKey, value);
        }

        public double CameraSpeed
        {
            get => (double)GetValue(CameraSpeedProperty);
            set => SetValue(CameraSpeedProperty, value);
        }

        public double MaxCameraSpeed
        {
            get => (double)GetValue(MaxCameraSpeedProperty);
            private set => SetValue(MaxCameraSpeedPropertyKey, value);
        }

        #endregion

        static Renderer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Renderer), new FrameworkPropertyMetadata(typeof(Renderer)));
        }

        private PerspectiveCamera Camera => Viewport.Camera as PerspectiveCamera;
        
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = new TimeSpan(0, 0, 0, 0, 15) };
        private readonly List<Visual3D> children = new List<Visual3D>();

        private Point lastPoint;

        public Renderer()
        {
            Loaded += delegate { timer.Start(); };
            Unloaded += delegate { timer.Stop(); };
            timer.Tick += Timer_Tick;
        }

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnViewportUnset();
            Viewport = Template.FindName(PART_Viewport, this) as Viewport3D;
            OnViewportSet();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            Focus();
            CaptureMouse();
            Cursor = Cursors.None;
            lastPoint = PointToScreen(e.GetPosition(this));
            lastPoint = new Point((int)lastPoint.X, (int)lastPoint.Y);

            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            ReleaseMouseCapture();
            Cursor = Cursors.Cross;

            e.Handled = true;
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            CameraSpeed = e.Delta > 0
                ? ClipValue(Math.Ceiling(CameraSpeed * 1050) / 1000, 0.001, MaxCameraSpeed)
                : ClipValue(Math.Floor(CameraSpeed * 0950) / 1000, 0.001, MaxCameraSpeed);

            e.Handled = true;
        }

        private void OnViewportUnset()
        {
            if (Viewport == null)
                return;

            Viewport.Loaded -= Viewport_Loaded;
            
            foreach (var c in children)
                Viewport.Children.Remove(c);
        }

        private void OnViewportSet()
        {
            if (Viewport == null)
                return;

            foreach (var c in children)
                Viewport.Children.Add(c);

            Viewport.Loaded += Viewport_Loaded;
        }
        #endregion

        #region Event Handlers
        private void Timer_Tick(object sender, EventArgs e)
        {
            NativeMethods.GetCursorPos(out var cursorPos);

            UpdateCameraPosition();
            UpdateCameraDirection(new Point(cursorPos.X, cursorPos.Y));
        }

        private void Viewport_Loaded(object sender, RoutedEventArgs e)
        {
            Viewport.Loaded -= Viewport_Loaded;
            Viewport.Camera = new PerspectiveCamera(default, default, new Vector3D(0, 0, 1), 90);
            ScaleToContent();
        }
        #endregion

        public void ScaleToContent()
        {
            if (Viewport == null)
                return;

            var content = children.OfType<ModelVisual3D>().Select(v => v.Content).OfType<Model3DGroup>();

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

            Camera.FarPlaneDistance = Math.Max(MinFarPlaneDistance, bounds.Length * 2);
            Camera.NearPlaneDistance = MinNearPlaneDistance;

            ZoomToBounds(bounds);

            var len = bounds.Length;
            CameraSpeed = Math.Ceiling(len);
            MaxCameraSpeed = Math.Ceiling(len * 6);
        }

        private void ZoomToBounds(RealBounds3D bounds)
        {
            if (bounds.Length == 0)
                return;

            var center = new Point3D(bounds.XBounds.Midpoint, bounds.YBounds.Midpoint, bounds.ZBounds.Midpoint);
            var radius = bounds.Length / 2;

            var hDistance = radius / Math.Tan(0.5 * Camera.FieldOfView * Math.PI / 180);
            var vFov = Camera.FieldOfView / Viewport.ActualWidth * Viewport.ActualHeight;
            var vDistance = radius / Math.Tan(0.5 * vFov * Math.PI / 180);

            //adjust angle and distance to side view for long models like weapons
            var adjust = vDistance > hDistance ? 0.75 : 1;
            var camDistance = Math.Max(hDistance, vDistance) * adjust;
            var lookDirection = (bounds.XBounds.Length > bounds.YBounds.Length * 1.5)
                ? new Vector3D(0, -Math.Sign(center.Y), 0)
                : new Vector3D(-Math.Sign(center.X), 0, 0);

            if (lookDirection.Length == 0)
                lookDirection = new Vector3D(-1, 0, 0);

            MoveCamera(center - lookDirection * camDistance, lookDirection);
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
                    Min = (float)m.Bounds.Y,
                    Max = (float)(m.Bounds.Y + m.Bounds.SizeY)
                },

                ZBounds = new RealBounds
                {
                    Min = (float)m.Bounds.Z,
                    Max = (float)(m.Bounds.Z + m.Bounds.SizeZ)
                }
            };

            ZoomToBounds(bounds);
        }

        public void AddChild(ModelVisual3D child)
        {
            children.Add(child);
            Viewport?.Children.Add(child);
        }

        public void RemoveChild(ModelVisual3D child)
        {
            children.Remove(child);
            Viewport?.Children.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
            OnViewportUnset(); //don't use Viewport.Children.Clear() because it will remove the lights
        }

        public void Dispose()
        {
            timer.Stop();
            Viewport?.Children.Clear();
        }

        private void MoveCamera(Point3D position, Vector3D direction)
        {
            Camera.Position = position;
            Camera.LookDirection = direction;

            var len = Camera.LookDirection.Length;
            Camera.LookDirection = new Vector3D(Camera.LookDirection.X / len, Camera.LookDirection.Y / len, Camera.LookDirection.Z / len);
            Yaw = Math.Atan2(Camera.LookDirection.X, Camera.LookDirection.Z);
            Pitch = Math.Atan(Camera.LookDirection.Y);
        }

        private void UpdateCameraPosition()
        {
            if (!IsMouseCaptured && !IsFocused)
                return;

            //#region Set FOV
            //if (CheckKeyState(Keys.NumPad6))
            //    Camera.FieldOfView = ClipValue(Camera.FieldOfView + Camera.FieldOfView / 100.0, 45, 120);
            //if (CheckKeyState(Keys.NumPad4))
            //    Camera.FieldOfView = ClipValue(Camera.FieldOfView - Camera.FieldOfView / 100.0, 45, 120);
            //#endregion
            //
            //#region Set FPD
            //if (CheckKeyState(Keys.NumPad8))
            //    Camera.FarPlaneDistance = ClipValue(Camera.FarPlaneDistance * 1.01, MinFarPlaneDistance, MaxFarPlaneDistance);
            //if (CheckKeyState(Keys.NumPad2))
            //    Camera.FarPlaneDistance = ClipValue(Camera.FarPlaneDistance * 0.99, MinFarPlaneDistance, MaxFarPlaneDistance);
            //#endregion

            if (!IsMouseCaptured)
                return;

            if (CheckKeyState(Keys.W) || CheckKeyState(Keys.A) || CheckKeyState(Keys.S) || CheckKeyState(Keys.D) || CheckKeyState(Keys.R) || CheckKeyState(Keys.F))
            {
                var moveVector = new Vector3D();
                var upVector = Camera.UpDirection;
                var forwardVector = Camera.LookDirection;
                var rightVector = Vector3D.CrossProduct(forwardVector, upVector);

                upVector.Normalize();
                forwardVector.Normalize();
                rightVector.Normalize();

                var dist = CameraSpeed * SpeedMultipler;
                if (CheckKeyState(Keys.ShiftKey))
                    dist *= 3;
                if (CheckKeyState(Keys.Space))
                    dist /= 3;

                #region Check WASD/RF

                if (CheckKeyState(Keys.W))
                    moveVector += forwardVector * dist;

                if (CheckKeyState(Keys.A))
                    moveVector -= rightVector * dist;

                if (CheckKeyState(Keys.S))
                    moveVector -= forwardVector * dist;

                if (CheckKeyState(Keys.D))
                    moveVector += rightVector * dist;

                if (CheckKeyState(Keys.R))
                    moveVector += upVector * dist;

                if (CheckKeyState(Keys.F))
                    moveVector -= upVector * dist;

                #endregion

                Camera.Position += moveVector;
            }
        }

        private void UpdateCameraDirection(Point mousePos)
        {
            if (!IsMouseCaptured || lastPoint.Equals(mousePos))
                return;

            var deltaX = (float)(mousePos.X - lastPoint.X) * (float)SpeedMultipler * 2;
            var deltaY = (float)(mousePos.Y - lastPoint.Y) * (float)SpeedMultipler * 2;

            var upAnchor = new Numerics.Vector3(0, 0, 1);
            var upVector = Numerics.Vector3.Normalize(new Numerics.Vector3((float)Camera.UpDirection.X, (float)Camera.UpDirection.Y, (float)Camera.UpDirection.Z));
            var forwardVector = Numerics.Vector3.Normalize(new Numerics.Vector3((float)Camera.LookDirection.X, (float)Camera.LookDirection.Y, (float)Camera.LookDirection.Z));
            var rightVector = Numerics.Vector3.Normalize(Numerics.Vector3.Cross(forwardVector, upVector));

            var yaw = Numerics.Matrix4x4.CreateFromAxisAngle(upAnchor, -deltaX);

            forwardVector = Numerics.Vector3.TransformNormal(forwardVector, yaw);
            rightVector = Numerics.Vector3.TransformNormal(rightVector, yaw);

            var pitch = Numerics.Matrix4x4.CreateFromAxisAngle(rightVector, -deltaY);

            forwardVector = Numerics.Vector3.TransformNormal(forwardVector, pitch);
            upVector = Numerics.Vector3.Normalize(Numerics.Vector3.Cross(rightVector, forwardVector));

            Camera.LookDirection = new Vector3D(forwardVector.X, forwardVector.Y, forwardVector.Z);
            Camera.UpDirection = new Vector3D(upVector.X, upVector.Y, upVector.Z);

            Yaw = Math.Atan2(forwardVector.X, forwardVector.Y);
            Pitch = Math.Asin(-forwardVector.Z);

            NativeMethods.SetCursorPos((int)lastPoint.X, (int)lastPoint.Y);
        }

        private static double ClipValue(double val, double min, double max)
        {
            return Math.Min(Math.Max(min, val), max);
        }

        private static bool CheckKeyState(Keys keys)
        {
            return (NativeMethods.GetAsyncKeyState((int)keys) & 32768) != 0;
        }
    }
}
