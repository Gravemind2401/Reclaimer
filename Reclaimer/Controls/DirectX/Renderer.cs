using Reclaimer.Geometry;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static HelixToolkit.Wpf.SharpDX.CameraExtensions;
using static HelixToolkit.Wpf.SharpDX.Media3DExtension;
using Helix = HelixToolkit.Wpf.SharpDX;
using HelixCore = HelixToolkit.SharpDX.Core;
using Keys = System.Windows.Forms.Keys;
using Media3D = System.Windows.Media.Media3D;
using NativeMethods = Reclaimer.Utilities.NativeMethods;
using Numerics = System.Numerics;

namespace Reclaimer.Controls.DirectX
{
    [TemplatePart(Name = PART_Viewport, Type = typeof(FrameworkElement))]
    public class Renderer : Control, IDisposable
    {
        private const string PART_Viewport = "PART_Viewport";

        private const double SpeedMultipler = 0.0013;
        private const double MinFarPlaneDistance = 50;
        private const double MinNearPlaneDistance = 0.01;

        private static readonly HelixCore.IEffectsManager effectsManager = new HelixCore.DefaultEffectsManager();

        #region Dependency Properties

        public static readonly DependencyPropertyKey ViewportPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Viewport), typeof(Helix.Viewport3DX), typeof(Renderer), new PropertyMetadata((object)null));

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

        public Helix.Viewport3DX Viewport
        {
            get => (Helix.Viewport3DX)GetValue(ViewportProperty);
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

        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send) { Interval = new TimeSpan(0, 0, 0, 0, 15) };
        private readonly List<Helix.Element3D> children = new List<Helix.Element3D>();

        private Helix.PerspectiveCamera Camera => Viewport.Camera as Helix.PerspectiveCamera;

        private CoordinateSystem coordinateSystem = CoordinateSystem.Default;
        private bool isLeftHand = false;
        private SharpDX.BoundingBox defaultBounds;
        private Point lastMousePoint;

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
            Viewport = Template.FindName(PART_Viewport, this) as Helix.Viewport3DX;
            OnViewportSet();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            Focus();
            CaptureMouse();
            Cursor = Cursors.None;
            lastMousePoint = PointToScreen(e.GetPosition(this));
            lastMousePoint = new Point((int)lastMousePoint.X, (int)lastMousePoint.Y);

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

            Viewport.OnRendered -= Viewport_OnRendered;

            foreach (var c in children)
                Viewport.Items.Remove(c);

            Viewport.EffectsManager = null;
        }

        private void OnViewportSet()
        {
            if (Viewport == null)
                return;

            foreach (var c in children)
                Viewport.Items.Add(c);

            SetCoordinateSystem(coordinateSystem);
            Viewport.EffectsManager = effectsManager;
            Viewport.OnRendered += Viewport_OnRendered;
        }
        #endregion

        #region Event Handlers
        private void Timer_Tick(object sender, EventArgs e)
        {
            NativeMethods.GetCursorPos(out var cursorPos);

            UpdateCameraPosition();
            UpdateCameraDirection(new Point(cursorPos.X, cursorPos.Y));
        }

        private void Viewport_OnRendered(object sender, EventArgs e)
        {
            Viewport.OnRendered -= Viewport_OnRendered;
            Camera.FieldOfView = 90;
            ScaleToContent();
        }
        #endregion

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            this.coordinateSystem = coordinateSystem;
            isLeftHand = coordinateSystem.RightVector.Length() < 0;

            if (Viewport == null)
                return;

            Viewport.ModelUpDirection = coordinateSystem.UpVector.ToMediaVector3();
            Camera.CreateLeftHandSystem = isLeftHand;
        }

        public void SetDefaultBounds(SharpDX.BoundingBox bounds) => defaultBounds = bounds;

        public void LocateObject(Helix.Element3D m) => ZoomToBounds(Enumerable.Repeat(m, 1).GetTotalBounds());

        public void ScaleToContent()
        {
            if (Viewport == null)
                return;

            //this determines how far the camera can move in any given direction
            var totalBounds = children.GetTotalBounds();

            Viewport.FixedRotationPoint = totalBounds.Center.ToPoint3D();
            Camera.FarPlaneDistance = Math.Max(MinFarPlaneDistance, totalBounds.Size.Length() * 2);
            Camera.NearPlaneDistance = MinNearPlaneDistance;

            //this determines the intitial camera location, direction and speed
            var scaleBounds = defaultBounds;
            if (scaleBounds.Width == 0 || scaleBounds.Height == 0 || scaleBounds.Depth == 0)
                scaleBounds = totalBounds;

            var len = scaleBounds.Size.Length();
            CameraSpeed = Math.Ceiling(len);
            MaxCameraSpeed = Math.Ceiling(len * 6);

            ZoomToBounds(scaleBounds);
        }

        public void ZoomToBounds(SharpDX.BoundingBox bounds)
        {
            if (Viewport == null || bounds.Size.Length() == 0)
                return;

            //transform bounds to default coordsys so we know X is depth, Y is width and Z is height
            bounds = HelixCore.BoundingBoxExtensions.Transform(bounds, CoordinateSystem.GetTransform(coordinateSystem, CoordinateSystem.Default, false).ToMatrix3());

            var center = bounds.Center;
            var radius = bounds.Size.Length() / 2;

            var hDistance = radius / Math.Tan(0.5 * Camera.FieldOfView * Math.PI / 180);
            var vFov = Camera.FieldOfView / Viewport.ActualWidth * Viewport.ActualHeight;
            var vDistance = radius / Math.Tan(0.5 * vFov * Math.PI / 180);

            //adjust angle and distance to side view for long models like weapons
            var adjust = vDistance > hDistance ? 0.75 : 1;
            var camDistance = Math.Max(hDistance, vDistance) * adjust;
            var lookDirection = bounds.Size.X > bounds.Size.Y * 1.5
                ? coordinateSystem.RightVector.ToMediaVector3() //view from left
                : -coordinateSystem.ForwardVector.ToMediaVector3(); //view from front

            if (lookDirection.Length == 0)
                lookDirection = -coordinateSystem.ForwardVector.ToMediaVector3();

            //transform back to original coordsys
            center = SharpDX.Vector3.TransformCoordinate(center, CoordinateSystem.GetTransform(CoordinateSystem.Default, coordinateSystem, false).ToMatrix3());

            Camera.LookAt(center.ToPoint3D(), lookDirection * camDistance, Viewport.ModelUpDirection, default);
        }

        public void AddChild(Helix.Element3D child)
        {
            children.Add(child);
            Viewport?.Items.Add(child);
        }

        public void RemoveChild(Helix.Element3D child)
        {
            children.Remove(child);
            Viewport?.Items.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
            OnViewportUnset(); //don't use Viewport.Children.Clear() because it will remove the lights
        }

        public void Dispose()
        {
            timer.Stop();

            if (Viewport != null)
            {
                foreach (var item in Viewport.Items)
                    item.Dispose();

                Viewport.Items.Clear();
                Viewport.Dispose();
                ClearChildren();
            }
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
                var moveVector = new Media3D.Vector3D();
                var upVector = Camera.UpDirection;
                var forwardVector = Camera.LookDirection;
                var rightVector = Media3D.Vector3D.CrossProduct(forwardVector, upVector) * (isLeftHand ? -1 : 1);

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
            if (!IsMouseCaptured || lastMousePoint.Equals(mousePos))
                return;

            var deltaX = (float)(mousePos.X - lastMousePoint.X) * (float)SpeedMultipler * 2;
            var deltaY = (float)(mousePos.Y - lastMousePoint.Y) * (float)SpeedMultipler * 2;

            var upAnchor = Viewport.ModelUpDirection.ToNumericsVector3();
            var upVector = Camera.UpDirection.ToNumericsVector3();
            var forwardVector = Camera.LookDirection.ToNumericsVector3();

            MoveLookDirection(in upAnchor, ref upVector, ref forwardVector, in deltaX, in deltaY);

            Camera.LookDirection = new Media3D.Vector3D(forwardVector.X, forwardVector.Y, forwardVector.Z);
            Camera.UpDirection = new Media3D.Vector3D(upVector.X, upVector.Y, upVector.Z);

            Yaw = Math.Atan2(forwardVector.X, forwardVector.Y);
            Pitch = Math.Asin(-forwardVector.Z);

            NativeMethods.SetCursorPos((int)lastMousePoint.X, (int)lastMousePoint.Y);
        }

        private static void MoveLookDirection(in Numerics.Vector3 worldUp, ref Numerics.Vector3 cameraUp, ref Numerics.Vector3 cameraForward, in float deltaX, in float deltaY)
        {
            cameraUp = Numerics.Vector3.Normalize(cameraUp);
            cameraForward = Numerics.Vector3.Normalize(cameraForward);

            var rightVector = Numerics.Vector3.Normalize(Numerics.Vector3.Cross(cameraForward, cameraUp));
            var yawTransform = Numerics.Matrix4x4.CreateFromAxisAngle(worldUp, -deltaX);

            cameraForward = Numerics.Vector3.TransformNormal(cameraForward, yawTransform);
            rightVector = Numerics.Vector3.TransformNormal(rightVector, yawTransform);

            var pitchTransform = Numerics.Matrix4x4.CreateFromAxisAngle(rightVector, -deltaY);

            cameraForward = Numerics.Vector3.TransformNormal(cameraForward, pitchTransform);
            cameraUp = Numerics.Vector3.Normalize(Numerics.Vector3.Cross(rightVector, cameraForward));
        }

        private static double ClipValue(double val, double min, double max) => Math.Min(Math.Max(min, val), max);
        private static bool CheckKeyState(Keys keys) => (NativeMethods.GetAsyncKeyState((int)keys) & 32768) != 0;
    }
}
