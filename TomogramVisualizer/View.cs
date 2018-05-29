using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;

namespace TomogramVisualizer
{
    public class View
    {
        public static int Min = 0, Max = 120;
        public int N=0;

        public void SetupView(int w, int h)
        {
            N = Bin.X;////
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-Bin.X, Bin.X, -Bin.Y, Bin.Y, -N, N);////
            GL.Viewport(0, 0, w, h);
        }

        public Color TransferFunction(int intensity)
        {
            int val = Clamp((intensity - Min) * 255 / (Max - Min), 0, 255);
            return Color.FromArgb(255, val, val, val);
        }

        public int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        public List<double> F(double u, double v)////
        {
            List<double> l = new List<double>();
            double r = Math.Min(Math.Min(Bin.X, Bin.Y), N) * 0.7;
            l.Add(Math.Cos(u) * Math.Sin(v) * r);
            l.Add(Math.Cos(v) * r);
            l.Add(Math.Sin(u) * Math.Sin(v) * r);
            return l;
        }

        public void DrawSphere()////
        {
            double UResolution =10, VResolution = 10;
            double startU = 0, startV = 0;
            double endU = Math.PI * 2;
            double endV = Math.PI;
            double stepU = (endU - startU) / UResolution; // step size between U-points on the grid 
            double stepV = (endV - startV) / VResolution; // step size between V-points on the grid 
            for (int i = 0; i < UResolution; i++)
            {
                for (double j = 0; j < VResolution; j++)
                {
                    double u = i * stepU + startU;
                    double v = j * stepV + startV;
                    double un = (i + 1 == UResolution) ? endU : (i + 1) * stepU + startU;
                    double vn = (j + 1 == VResolution) ? endV : (j + 1) * stepV + startV;
                    List<double> p0 = F(u, v);
                    List<double> p1 = F(u, vn);
                    List<double> p2 = F(un, v);
                    List<double> p3 = F(un, vn);
                    DrawTextureRectGL3d(p0, p1, p2, p3);
                }
            }
        }


        public void DrawQuadr(int ln)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);

            ////DrawSphere();
            //DrawRectGL((int)0.1*Bin.X, (int)0.1 * Bin.Y, (int)0.1 * Bin.X, (int)0.8 * Bin.Y, 
            //    (int)0.8 * Bin.X, (int)0.8 * Bin.Y, (int)0.8 * Bin.X, (int)0.1 * Bin.Y, 4);

            GL.End();
        }

        Bitmap textureImage;
        int VBOtexture;
        public void Load2dTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            ErrorCode er = GL.GetError();


        }

        public void GenerateTextureImage(int ln)////
        {
            //textureImage = new Bitmap(Bin.X, Bin.Y);
            //for (int i = 0; i < Bin.X; i++)
            //    for (int j = 0; j < Bin.Y; j++)
            //        textureImage.SetPixel(i, j, TransferFunction(Bin.array[i, j, ln]));
            textureImage = Bin.bitmap;
        }

        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);
            DrawSphere();////
            GL.End();

            GL.Disable(EnableCap.Texture2D);

        }

        private void DrawRectGL(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, int ln)
        {
            GL.Color3(TransferFunction(Bin.array[x1, y1, ln]));
            GL.Vertex2(x1, y1);

            GL.Color3(TransferFunction(Bin.array[x2, y2, ln]));
            GL.Vertex2(x2, y2);

            GL.Color3(TransferFunction(Bin.array[x3, y3, ln]));
            GL.Vertex2(x3, y3);

            GL.Color3(TransferFunction(Bin.array[x4, y4, ln]));
            GL.Vertex2(x4, y4);
        }


        private void DrawTextureRectGL()
        {
            GL.Color3(Color.White);

            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);

            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);

            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);

            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);
        }

        private void DrawTextureRectGL3d(List<double> p0, List<double> p1, List<double> p2, List<double> p3)////
        {
            GL.Color3(Color.White);

            GL.TexCoord2(0f, 0f);
            GL.Vertex3(p0[0], p0[1], p0[2]);

            GL.TexCoord2(0f, 1f);
            GL.Vertex3(p1[0], p1[1], p1[2]);

            GL.TexCoord2(1f, 1f);
            GL.Vertex3(p3[0], p3[1], p3[2]);

            GL.TexCoord2(1f, 0f);
            GL.Vertex3(p2[0], p2[1], p2[2]);
        }
    }
}
