using HelixToolkit.Wpf;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows;
using System.Reflection;
using System.Windows.Forms;

namespace Space_Flight_Code
{
    public class Object
    {
        public double Radius;
        public Point3D Center;
        public string texture;
        public Model3DGroup Model;
        public Transform3DGroup TransformGroup = new Transform3DGroup();
        public AxisAngleRotation3D SelfRotation = new AxisAngleRotation3D();
        public RotateTransform3D RotationTransform;
        public AxisAngleRotation3D RotationAngleOrbit = new AxisAngleRotation3D();

        public Object()
        {
            RotationTransform = new RotateTransform3D(SelfRotation);
            TransformGroup.Children.Add(RotationTransform);
            SelfRotation.Axis = new Vector3D(0, 0, 1);
        }
        public Point3D GetWorldPosition()
        {
            return TransformGroup.Transform(Center);
        }

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
                Material = material,
                BackMaterial = material
            };


            Model = new Model3DGroup();
            Model.Children.Add(sphere);
            Model.Children.Add(new AmbientLight(Colors.White));
            Model.Transform = TransformGroup;

            return Model;
        }

       
    }

    public class Satelite : Object
    {
        public Vector3D Move;

        public Satelite()
        {

        }

        public new Model3DGroup Draw()
        {
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddArrow(Center, Center + Move, 100);

            var arrow = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = new DiffuseMaterial(Brushes.Red)
            };


            Model = new Model3DGroup();
            Model.Children.Add(arrow);
            Model.Children.Add(new AmbientLight(Colors.White));
            Model.Transform = TransformGroup;

            return Model;
        }

        public void StartCoords(double longitude, double latitude)
        {
            Point3D StartCoords = new Point3D(-6031f, 0, 0);
            Point3D NewCoords = new Point3D(0, 0, 0);

            NewCoords.X = -6031f * (float)(Math.Cos(latitude) * Math.Cos(longitude));
            NewCoords.Y = -6031f * (float)(Math.Cos(latitude) * Math.Sin(longitude));
            NewCoords.Z = 6031f * (float)Math.Sin(latitude);

            Center = NewCoords;
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
            Center = new Point3D(0, 384467f, 0);
            texture = "textures/moon.jpg";

        }
    }



}