using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpGL;

namespace Space_Flight_Code
{
    public partial class Form1 : Form
    {

        private OpenGL _gl;
        private float _angle = 0f;
        private float _angleX = 0f;
        private float _angleY = 0f;
        private Point _lastMousePos;

        public Form1()
        {
            InitializeComponent();
            openGLControl1.DrawFPS = true;
            openGLControl1.OpenGLInitialized += openGLControl1_OpenGLInitialized;
            openGLControl1.OpenGLDraw += openGLControl1_OpenGLDraw;
            openGLControl1.Resized += openGLControl1_Resized;
        }

        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            var gl = openGLControl1.OpenGL;
            gl.Enable(OpenGL.GL_DEPTH_TEST); // Включить буфер глубины
        }

        private void openGLControl1_Resized(object sender, EventArgs e)
        {
            var gl = openGLControl1.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45.0f, (float)openGLControl1.Width / openGLControl1.Height, 0.1f, 100.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs args)
        {
            var gl = openGLControl1.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            // Настройка камеры
            gl.Translate(0f, 0f, -5f); // Отодвигаем камеру назад
            gl.Rotate(_angleX, 1.0f, 0.0f, 0.0f); // Поворот по X
            gl.Rotate(_angleY, 0.0f, 1.0f, 0.0f); // Поворот по Y

            // Отрисовка куба
            DrawCube(gl);

            _angle += 1f; // Анимация вращения
            gl.Flush();
        }

        private void DrawCube(OpenGL gl)
        {
            gl.Begin(OpenGL.GL_QUADS);

            // Передняя грань (Z = 1)
            gl.Color(1.0f, 0.0f, 0.0f); // Красный
            gl.Vertex(-0.5f, -0.5f, 0.5f);
            gl.Vertex(0.5f, -0.5f, 0.5f);
            gl.Vertex(0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, 0.5f, 0.5f);

            // Задняя грань (Z = -1)
            gl.Color(0.0f, 1.0f, 0.0f); // Зеленый
            gl.Vertex(-0.5f, -0.5f, -0.5f);
            gl.Vertex(-0.5f, 0.5f, -0.5f);
            gl.Vertex(0.5f, 0.5f, -0.5f);
            gl.Vertex(0.5f, -0.5f, -0.5f);

            // Верхняя грань (Y = 1)
            gl.Color(0.0f, 0.0f, 1.0f); // Синий
            gl.Vertex(-0.5f, 0.5f, -0.5f);
            gl.Vertex(-0.5f, 0.5f, 0.5f);
            gl.Vertex(0.5f, 0.5f, 0.5f);
            gl.Vertex(0.5f, 0.5f, -0.5f);

            // Остальные грани (нижняя, левая, правая) добавляются аналогично...

            gl.End();
        }

        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float deltaX = e.X - _lastMousePos.X;
                float deltaY = e.Y - _lastMousePos.Y;
                _angleY += deltaX * 0.5f;
                _angleX += deltaY * 0.5f;
                openGLControl1.Invalidate(); // Перерисовать
            }
            _lastMousePos = e.Location;
        }
    }
}
