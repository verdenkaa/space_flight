using HelixToolkit.Wpf;
using Space_Flight_Code.Space_Flight_Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms.DataVisualization.Charting;

namespace Space_Flight_Code
{
    public partial class Form1 : Form
    {
        private HelixViewport3D helixViewport;
        private Earth earth;
        private Moon moon;
        private Satelite satelite;
        private Physics Core;
        private DispatcherTimer animationTimer;
        private Object trackedObject;
        private bool isTracking;
        private int speed_anim = 1;

        private double KineticEnergy;
        private double PotentialEnergy;
        private double TotalEnergy;

        private double simTime = 0.0;
        private const int MAX_POINTS_ON_CHART = 5000;
        private const int ENERGY_PLOT_DECIMATION = 5;
        private int energyFrameCounter = 0;

        private const double ENERGY_WINDOW_SECONDS = 5000.0;



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
            helixViewport.ShowCoordinateSystem = true; // Отключает оси координат
            helixViewport.ShowCameraTarget = false;     // Отключает маркер центра вращения

        }

        private void InitializeObjects()
        {
            earth = new Earth();
            moon = new Moon();
            

            helixViewport.Children.Add(new ModelVisual3D { Content = earth.Draw() });
            helixViewport.Children.Add(new ModelVisual3D { Content = moon.Draw() });

            Core = new Physics();

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
            if (model == satelite.Model)
            {
                label1.Text = "Отслеживается: Спутник";
                return satelite;
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

            earth.UpdatePosition(speed_anim);
            moon.UpdatePosition(speed_anim);

            satelite.UpdatePosition(earth, moon, speed_anim);

           
            simTime += speed_anim;

            energyFrameCounter++;
            if (energyFrameCounter % ENERGY_PLOT_DECIMATION == 0)
            {
                var energies = Core.EvaluateEnergies(satelite, earth, moon);
                double kinMJ = energies.KineticJ / 1e6;
                double potMJ = energies.PotentialJ / 1e6;
                double totMJ = energies.TotalJ / 1e6;

                var sKE = chartEnergy.Series["Kinetic"];
                var sPE = chartEnergy.Series["Potential"];
                var sTE = chartEnergy.Series["Total"];
                var ca = chartEnergy.ChartAreas["EnergyArea"];

                sKE.Points.AddXY(simTime, kinMJ);
                sPE.Points.AddXY(simTime, potMJ);
                sTE.Points.AddXY(simTime, totMJ);

                double cutoff = simTime - ENERGY_WINDOW_SECONDS;
                if (cutoff < 0) cutoff = 0.0;

                Action<Series> trimSeries = (series) =>
                {
                    while (series.Points.Count > 0 && series.Points[0].XValue < cutoff)
                    {
                        series.Points.RemoveAt(0);
                        if (series.Points.Count <= 0) break;
                    }
                };

                trimSeries(sKE);
                trimSeries(sPE);
                trimSeries(sTE);

                ca.AxisX.Minimum = Math.Max(0.0, simTime - ENERGY_WINDOW_SECONDS);
                ca.AxisX.Maximum = Math.Max(ca.AxisX.Minimum + 1.0, simTime);

                ca.AxisY.Minimum = -40000;
                ca.AxisY.Maximum = 40000;
                //ca.RecalculateAxesScale();
            }


            if (Core.Is_Hit(satelite, earth, moon))
            {
                satelite.SetTrajectoryStyle();
                animationTimer.Stop();
                startbtn.Text = "Старт";
                System.Windows.Forms.MessageBox.Show("Столкновение!");
            }

            UpdateCameraTracking();

            helixViewport.InvalidateVisual();

            }


        



        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            speed_anim = trackBar1.Value;
            //animationTimer.Interval = TimeSpan.FromMilliseconds((trackBar1.Maximum - trackBar1.Value) + 1);
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


        private void button1_Click(object sender, EventArgs e)
        {


            satelite = new Satelite();

            double latitude = (double)numericUpDown1.Value * Math.PI / 180;
            double longitude = (double)numericUpDown2.Value * Math.PI / 180;
            int height = (int)numericUpDown9.Value;


            satelite.StartCoords(longitude, latitude, height);


            Vector3D Velocity = ReadVelocity();


            satelite.Mass = (double)numericUpDown6.Value;
            satelite.Speed = (double)numericUpDown3.Value;
            satelite.Velocity = Velocity * satelite.Speed;

            helixViewport.Children.Add(satelite.Trajectory);


            //helixViewport.Children.Add(new ModelVisual3D { Content = satelite.Draw() });

            button1.Enabled = false;
            button2.Enabled = true;
            startbtn.Enabled = true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            satelite = new Satelite();
            helixViewport.Children.RemoveAt(3);

            button1.Enabled = true;
            button2.Enabled = false;
            startbtn.Enabled = false;
            animationTimer.Stop();
            startbtn.Text = "Старт";
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

        private Vector3D ReadVelocity()
        {
            double X = (double)numericUpDown4.Value;
            double Y = (double)numericUpDown5.Value;
            double Z = (double)numericUpDown7.Value;

            Vector3D v = new Vector3D(X, Y, Z);
            v.Normalize();
            return v;    
        }

        private void button3_Click(object sender, EventArgs e)
        {
            earth.SelfRotation.Angle = 0;
            moon.SelfRotation.Angle = 0;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show($"Selected item: {comboBox4.SelectedItem}");
            string way = comboBox4.SelectedItem.ToString();
            
            switch (way)
            {

                case "Элиптическая орбита":
                    numericUpDown1.Value = 0; // latitude
                    numericUpDown2.Value = 0; // longtitude
                    numericUpDown9.Value = 1000; // height

                    numericUpDown6.Value = 500; // mass
                    numericUpDown3.Value = 10; // speed

                    numericUpDown4.Value = 0; // x
                    numericUpDown5.Value = 1; // y
                    numericUpDown7.Value = 0; // z

                    break;

                case "Орбита Молния":
                    numericUpDown1.Value = -63.4m;
                    numericUpDown2.Value = 0;


                    numericUpDown9.Value = 500;

                    numericUpDown6.Value = 500;

                    numericUpDown3.Value = 10.0m;

                    numericUpDown4.Value = -0.3m;
                    numericUpDown5.Value = -0.9m;
                    numericUpDown7.Value = -0.1m;

                    break;


                case "Полусинхронная орбита":
                    numericUpDown1.Value = 0;
                    numericUpDown2.Value = 0;

                    numericUpDown9.Value = 35726;

                    numericUpDown6.Value = 500;

                    numericUpDown3.Value = 3.075m;

                    numericUpDown4.Value = 0;
                    numericUpDown5.Value = -1;
                    numericUpDown7.Value = 0;

                    break;

                case "Эксцентричная орбита":
                    numericUpDown1.Value = 45.0m;
                    numericUpDown2.Value = 0;

                    numericUpDown9.Value = 300;

                    numericUpDown6.Value = 500;

                    numericUpDown3.Value = 11;

                    numericUpDown4.Value = 0.5m;
                    numericUpDown5.Value = 0.9m;
                    numericUpDown7.Value = 0.3m;

                    break;

                case "НОО с наклоном 45°":
                    numericUpDown1.Value = 45.0m;
                    numericUpDown2.Value = 0;

                    numericUpDown9.Value = 400;

                    numericUpDown6.Value = 500;

                    numericUpDown3.Value = 8.5m;

                    numericUpDown4.Value = 0.5m;
                    numericUpDown5.Value = 0.5m;
                    numericUpDown7.Value = 0.7m;

                    break;

                case "Облет Луны":
                    // Старт с низкой орбиты
                    numericUpDown1.Value = 0;
                    numericUpDown2.Value = 90;

                    numericUpDown9.Value = 300;

                    numericUpDown6.Value = 500;

                    numericUpDown3.Value = 11.54m;

                    numericUpDown4.Value = -1;
                    numericUpDown5.Value = -0.5m;
                    numericUpDown7.Value = 0;

                    break;

            }



        }

    }


}
