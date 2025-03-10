using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Security.Policy;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.TextFormatting;
using System.Windows.Input;

namespace Space_Flight_Code
{
    public partial class Form1 : Form
    {
        private HelixViewport3D helixViewport;
        private Earth earth;
        private Moon moon;
        private DispatcherTimer animationTimer;
        private double earthRotationAngle;
        private double moonOrbitAngle;
        private double moonSelfRotationAngle;
        private Object trackedObject;
        private PerspectiveCamera defaultCamera;
        private bool isTracking;




        public Form1()
        {
            InitializeComponent();
            InitializeViewport();
            InitializeObjects();
            InitializeAnimation();
            InitializeCameraTracking();
        }

        private void InitializeCameraTracking()
        {
            // Сохраняем начальные параметры камеры
            defaultCamera = new PerspectiveCamera
            {
                Position = new Point3D(0, 500000, 1500000),
                LookDirection = new Vector3D(0, -500000, -1500000),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000
            };

            helixViewport.Camera = defaultCamera.Clone();
            helixViewport.MouseDown += Viewport_MouseDown;
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var hit = helixViewport.Viewport.FindNearestVisual(e.GetPosition(helixViewport.Viewport));
            if (hit is ModelVisual3D visual && visual.Content != null)
            {
                var model = visual.Content as Model3DGroup;
                if (model != null)
                {
                    trackedObject = FindObjectByModel(model);
                    isTracking = true;
                }
            }
        }

        private Object FindObjectByModel(Model3DGroup model)
        {
            if (model == earth.Model) return earth;
            if (model == moon.Model) return moon;
            return null;
        }

        private void UpdateCameraTracking()
        {
            if (!isTracking || trackedObject == null) return;

            var currentCamera = helixViewport.Camera as PerspectiveCamera;
            if (currentCamera == null) return;

            // Рассчитываем новую позицию камеры
            var targetPosition = trackedObject.TransformGroup.Transform(trackedObject.Center);
            var cameraOffset = new Vector3D(0, 0, trackedObject.Radius * 5);
            var newPosition = targetPosition + cameraOffset;

            // Плавное перемещение камеры
            currentCamera.Position = Lerp(currentCamera.Position, newPosition, 0.5);
            currentCamera.LookDirection = targetPosition - currentCamera.Position;
            
        }

        private Point3D Lerp(Point3D a, Point3D b, double t)
        {
            return new Point3D(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t);
        }


        private void InitializeViewport()
        {


            // Создание WPF-элемента
            helixViewport = new HelixViewport3D()
            {
                ZoomExtentsWhenLoaded = true,
                Background = System.Windows.Media.Brushes.Black
            };

            // Настройка ElementHost
            var host = new ElementHost
            {
                Parent = panel1,
                Dock = DockStyle.Fill,
                Child = helixViewport
            };

            //this.Controls.Add(host);

            helixViewport.ShowCameraInfo = false;       // Отключает координаты камеры
            helixViewport.ShowViewCube = false;         // Отключает куб ориентации
            helixViewport.ShowFrameRate = true;        // Отключает FPS-счетчик
            helixViewport.ShowCoordinateSystem = false; // Отключает оси координат
            helixViewport.ShowCameraTarget = false;     // Отключает маркер центра вращения
            


            //helixViewport.Camera.LookAt(earth.Center, 1);


            //helixViewport.ZoomExtents();

        }

        private void InitializeObjects()
        {
            earth = new Earth();
            moon = new Moon();

            helixViewport.Children.Add(new ModelVisual3D { Content = earth.Draw() });
            helixViewport.Children.Add(new ModelVisual3D { Content = moon.Draw() });
        }

        private void InitializeAnimation()
        {
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            const double earthRotationSpeed = 10; // Градусов/сек
            const double moonOrbitSpeed = 10;      // Градусов/сек
            const double moonRotationSpeed = 10;   // Градусов/сек 0.22 вроде относительно

            earthRotationAngle += earthRotationSpeed * 0.001;
            moonOrbitAngle += moonOrbitSpeed * 0.001;
            moonSelfRotationAngle += moonRotationSpeed * 0.001;

            earth.SelfRotation.Angle = earthRotationAngle % 360;

            moon.SelfRotation.Angle = moonSelfRotationAngle % 360;

            moon.UpdateOrbit(moonOrbitAngle);

            UpdateCameraTracking();

            helixViewport.InvalidateVisual();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            animationTimer.Interval = TimeSpan.FromMilliseconds((trackBar1.Maximum - trackBar1.Value) + 1);
        }


        private void startbtn_Click(object sender, EventArgs e)
        {
            if (animationTimer.IsEnabled)
            {
                animationTimer.Stop();
                startbtn.Text = "Старт";
            }
            else
            {
                animationTimer.Start();
                startbtn.Text = "Стоп";
            }
        }

        private void resetbtn_Click(object sender, EventArgs e)
        {
            isTracking = false;
            helixViewport.Camera = defaultCamera.Clone();
            helixViewport.ZoomExtents();
        }
    }

    public class Object
    {
        public double Radius;
        public Point3D Center;
        public string texture;
        public Model3DGroup Model;
        public Transform3DGroup TransformGroup = new Transform3DGroup();
        public AxisAngleRotation3D SelfRotation  = new AxisAngleRotation3D();
        public RotateTransform3D RotationTransform;

        public Object()
        {
            RotationTransform = new RotateTransform3D(SelfRotation);
            TransformGroup.Children.Add(RotationTransform);
        }
        public Point3D GetWorldPosition()
        {
            return TransformGroup.Transform(Center);
        }

        public Model3DGroup Draw()
        {

            Model = CreateModelGroup();
            return Model;
            
        }

        private Model3DGroup CreateModelGroup()
        {
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddSphere(Center, Radius, 40, 40);



            var material = new DiffuseMaterial(
                    new ImageBrush
                    {
                        ImageSource = new BitmapImage(
                            new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, texture))),
                        Stretch = Stretch.Fill
                    }
                );
            var sphere = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = material
            };


            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(sphere);
            modelGroup.Children.Add(new AmbientLight(Colors.White));
            modelGroup.Transform = TransformGroup;

            return modelGroup;
        }
    }

    public class Earth : Object
    {
       public Earth()
        {
            Radius = 6301f;
            Center = new Point3D(0, 0, 0);
            texture = "textures/earth.jpg";
            SelfRotation.Axis = new Vector3D(0, 0, 1);
        }
    }

    public class Moon : Object
    {
        private readonly TranslateTransform3D _orbitTransform = new TranslateTransform3D();
        private readonly RotateTransform3D _orbitRotation = new RotateTransform3D();
        private double orbitRadius;

        public Moon()
        {
            Radius = 1737f;
            Center = new Point3D(0, 0, 0);
            texture = "textures/moon.jpg";
            SelfRotation.Axis = new Vector3D(0, 0, 1);
            orbitRadius = 384467f + 6301f;

            TransformGroup.Children.Insert(0, _orbitRotation);
            TransformGroup.Children.Insert(0, _orbitTransform);

            UpdateOrbit(0);
        }

        public void UpdateOrbit(double angle)
        {
            _orbitTransform.OffsetX = orbitRadius * Math.Cos(angle * Math.PI / 180);
            _orbitTransform.OffsetY = orbitRadius * Math.Sin(angle * Math.PI / 180);

            _orbitRotation.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle);
        }
    }
}
