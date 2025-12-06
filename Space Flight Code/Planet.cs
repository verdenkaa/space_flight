using HelixToolkit.Wpf;
using Space_Flight_Code.Space_Flight_Code;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;

namespace Space_Flight_Code
{
    public class Object
    {
        public double Radius;
        public double Mass;
        public Point3D Center;
        public string texture;
        public Model3DGroup Model;
        public const double RotationSpeed = 0.001;
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

        public void UpdatePosition(int speed_anim)
        {
            SelfRotation.Angle += RotationSpeed * speed_anim % 360;
        }


    }

public class Satelite : Object
{
    public Vector3D Velocity;
    public double Speed;
    //public double TimeStep = 1;
    private Physics Core;

    private TubeVisual3D trajectoryTube;
    private List<Point3D> points = new List<Point3D>();
    private const int MAX_POINTS = 10000;

    public Satelite()
    {
        InitializeTrajectory();
    }

    private void InitializeTrajectory()
    {
        trajectoryTube = new TubeVisual3D
        {
            Diameter = 100,
            ThetaDiv = 3,
            Path = new Point3DCollection(),
            Material = Materials.Green,
            BackMaterial = Materials.Green
        };

        Core = new Physics();
    }

    public TubeVisual3D DrawTrajectory()
    {
        return trajectoryTube;
    }

    public void StartCoords(double longitude, double latitude, int height)
    {
        Point3D StartCoords = new Point3D(-6431f - height, 0, 0);
        Point3D NewCoords = new Point3D(0, 0, 0);

        NewCoords.X = StartCoords.X * (float)(Math.Cos(latitude) * Math.Cos(longitude));
        NewCoords.Y = StartCoords.X * (float)(Math.Cos(latitude) * Math.Sin(longitude));
        NewCoords.Z = -StartCoords.X * (float)Math.Sin(latitude);

            Center = NewCoords;

        AddPointToTrajectory(Center);
    }

    public void UpdatePosition(Earth earth, Moon moon, int speed_anim)
    {
            for (int i = 0; i < speed_anim; i++)
            {
                (Center, Velocity) = Core.GravityEvaluate(this, earth, moon);
            }

        AddPointToTrajectory(Center);
    }

    private void AddPointToTrajectory(Point3D newPoint)
    {
        points.Add(newPoint);
        
        if (points.Count > MAX_POINTS)
        {
            points.RemoveAt(0);
        }
        
        // Обновляем путь траектории
        trajectoryTube.Path = new Point3DCollection(points);
    }



    // Метод для обновления внешнего вида траектории
    public void SetTrajectoryStyle()
    {
            //trajectoryTube.Diameter = diameter;
            Color red = System.Windows.Media.Color.FromRgb(255, 0, 0);
            trajectoryTube.Material = new DiffuseMaterial(new SolidColorBrush(red));
        trajectoryTube.BackMaterial = new DiffuseMaterial(new SolidColorBrush(red));
    }

    // Свойство для доступа к траектории извне
    public TubeVisual3D Trajectory => trajectoryTube;
}

    public class Earth : Object
    {
        public Earth()
        {
            Radius = 6301f;
            Mass = 5.972e24;
            Center = new Point3D(0, 0, 0);
            texture = "textures/earth.jpg";
        }
    }

    public class Moon : Object
    {
        public Moon()
        {
            Radius = 1737f;
            Mass = 7.342e22;
            Center = new Point3D(0, 384467f, 0); // Начальная позиция
            texture = "textures/moon.jpg";
        }
    }



}