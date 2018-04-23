using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace TomogramVisualizer
{
    public class View
    {
        public static int Min = 0, Max = 120;

        public void SetupView(int w, int h)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
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

        public void DrawQuadr(int ln)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);

            for (int x = 0; x < Bin.X - 1; x++)
                for (int y = 0; y < Bin.Y - 1; y++)
                {
                    DrawRectGL(x, y, x, y + 1, x + 1, y + 1, x + 1, y, ln);
                }
              
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

        public void GenerateTextureImage(int ln)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i < Bin.X; i++)
                for (int j = 0; j < Bin.Y; j++)
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[i, j, ln]));
        }

        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);
            DrawTextureRectGL();
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
    }
}
