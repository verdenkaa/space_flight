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
                //ZoomExtentsWhenLoaded = true,
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


                // Добавление сферы
                AddSphere();
        }

        private void AddSphere()
        {
            // Создание сферы
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddSphere(new Point3D(0, 0, 0), 1.0, 40, 40);

            // Загрузка текстуры
            var material = new DiffuseMaterial(
                new ImageBrush
                {
                    ImageSource = new BitmapImage(
                        new Uri("E:/Projects/space_flight/Space Flight Code/materials/earth.jpg")),
                    Stretch = Stretch.Fill
                }
            );

            // Создание модели
            var sphere = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = material
            };

            // Добавление в сцену
            var modelGroup = new Model3DGroup();
            modelGroup.Children.Add(sphere);
            modelGroup.Children.Add(new AmbientLight(Colors.White));

            helixViewport.Children.Add(new ModelVisual3D { Content = modelGroup });
            helixViewport.ZoomExtents();
        }

    }
}
