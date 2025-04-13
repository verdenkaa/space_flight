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
using System.Text.RegularExpressions;
using System.Reflection;

namespace Space_Flight_Code
{
    public partial class Form1 : Form
    {
        private HelixViewport3D helixViewport;
        private Earth earth;
        private Moon moon;
        private Satelite satelite;
        private DispatcherTimer animationTimer;
        private Object trackedObject;
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
            helixViewport.Camera = new PerspectiveCamera();
            helixViewport.MouseDown += Viewport_MouseDown;
        }


        private void InitializeViewport()
        {
            helixViewport = new HelixViewport3D()
            {
                ZoomExtentsWhenLoaded = true,
                Background = System.Windows.Media.Brushes.Black
            };

            var host = new ElementHost
            {
                Parent = panel1,
                Dock = DockStyle.Fill,
                Child = helixViewport
            };


            helixViewport.ShowCameraInfo = false;       // Отключает координаты камеры
            helixViewport.ShowViewCube = false;         // Отключает куб ориентации
            helixViewport.ShowFrameRate = true;        // Отключает FPS-счетчик
            helixViewport.ShowCoordinateSystem = false; // Отключает оси координат
            helixViewport.ShowCameraTarget = false;     // Отключает маркер центра вращения

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
                    UpdateCameraTracking();
                }
            }
        }

        private Object FindObjectByModel(Model3DGroup model)
        {
            if (model == earth.Model)
            {
                label1.Text = "Отслеживается: Земля";
                return earth;
            }
            if (model == moon.Model)
            {
                label1.Text = "Отслеживается: Луна";
                return moon;
            }
            return null;
        }

        private void UpdateCameraTracking()
        {
            if (!isTracking) return;

            helixViewport.Camera.LookAt(trackedObject.GetWorldPosition(), 0);

        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            const double earthRotationSpeed = 0.01; // Градусов/сек
            const double moonRotationSpeed = 0.01;

            earth.SelfRotation.Angle += earthRotationSpeed % 360;
            moon.SelfRotation.Angle += moonRotationSpeed % 360;

            UpdateCameraTracking();

            helixViewport.InvalidateVisual();
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
            label1.Text = "Ничего не отслеживается";
            helixViewport.ZoomExtents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            satelite = new Satelite();

            double latitude = (double)numericUpDown1.Value * Math.PI / 180;
            double longitude = (double)numericUpDown2.Value * Math.PI / 180;

            satelite.StartCoords(longitude, latitude);
            satelite.Move = new Vector3D(satelite.Center.X + 10, satelite.Center.Y + 10, satelite.Center.Z + 10);
            helixViewport.Children.Add(new ModelVisual3D { Content = satelite.Draw() });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                isTracking = false;
                label1.Text = "Ничего не отслеживается";
                helixViewport.ZoomExtents();
                trackedObject = null;
            } else if (comboBox1.SelectedIndex == 1)
                {
                    label1.Text = "Отслеживается: Земля";
                    trackedObject = earth;
                }
                else if (comboBox1.SelectedIndex == 2)
                {
                    label1.Text = "Отслеживается: Луна";
                    trackedObject = moon;
                }
                else if (comboBox1.SelectedIndex == 3)
                {
                    label1.Text = "Отслеживается: Спутник";
                    trackedObject = satelite;
                }

            if (trackedObject != null)
            {
                isTracking = true;
                UpdateCameraTracking();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }


}
