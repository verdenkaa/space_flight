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

namespace Space_Flight_Code
{
    public partial class Form1 : Form
    {
        private HelixViewport3D helixViewport;

        public Form1()
        {
            InitializeComponent();


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
            helixViewport.Camera.LookAt();


            // Добавление сферы
            Earth earth = new Earth();
            Moon moon = new Moon();

            helixViewport.Children.Add(new ModelVisual3D { Content = earth.Draw() });
            helixViewport.Children.Add(new ModelVisual3D { Content = moon.Draw() });


            helixViewport.ZoomExtents();

        }

    }

    public class Object
    {
        public double Radius;
        public Point3D Center;
        public string texture;

        public Model3DGroup Draw()
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
        }
    }

    public class Moon : Object
    {
        public Moon()
        {
            Radius = 1737f;
            Center = new Point3D(0, 384467f + 6301f, 0);
            texture = "textures/moon.jpg";
        }
    }
}
