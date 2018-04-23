using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TomogramVisualizer
{
    public partial class Form1 : Form
    {
        View view = new View();
        bool loaded = false;
        int curLayer = 0;

        public Form1()
        {
            InitializeComponent();
            trackBarMin.Maximum = 5000;
            trackBarWidth.Minimum = 1;
            trackBarWidth.Maximum = 5000;
            trackBarMin.Value = View.Min;
            trackBarWidth.Value = View.Max-View.Min;
            radioButtonQuadr.Checked = true;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Bin.ReadBin(dialog.FileName);
                Start();
            }
        }

        void Start()
        {
            view.SetupView(glControl1.Width, glControl1.Height);
            loaded = true;
            glControl1.Invalidate();
            trackBar1.Maximum = Bin.Z-1;
            trackBar1.Value = curLayer;
        }

        bool needReload = false;
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
                if (radioButtonQuadr.Checked)    
                {
                    view.DrawQuadr(curLayer);
                    glControl1.SwapBuffers();
                }
            else if (radioButtonTexture.Checked)
                {
                    if (needReload)
                    {
                        view.GenerateTextureImage(curLayer);
                        view.Load2dTexture();
                        needReload = false;
                    }
                    view.DrawTexture();
                    glControl1.SwapBuffers();
                }
        }

        private void ParChanged()
        {
            glControl1.Invalidate();
            needReload = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            curLayer = trackBar1.Value;
            ParChanged();
        }

        private void trackBarMin_Scroll(object sender, EventArgs e)
        {
            View.Min = trackBarMin.Value;
            ParChanged();
        }

        private void trackBarWidth_Scroll(object sender, EventArgs e)
        {
            View.Max = trackBarMin.Value+trackBarWidth.Value;
            ParChanged();
        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                DisplayFPS();
                glControl1.Invalidate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += ApplicationIdle;
        }

        int FrameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        private void DisplayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("Tomogramm Visualizer (FPS={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

        private void radioButtonTexture_CheckedChanged(object sender, EventArgs e)
        {
            ParChanged();
        }

        private void radioButtonQuadr_CheckedChanged(object sender, EventArgs e)
        {
            ParChanged();
        }
    }
}
