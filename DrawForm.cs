using System;
using System.Drawing;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class DrawForm : Form
    {
        private Timer timer2;
        private Font fnt = new Font("Arial", 10);
        private int initialX, initialY;

        public DrawForm()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            InitTimer();
            
            Rectangle r = new Rectangle();
            foreach (Screen s in Screen.AllScreens)
            {
                //if (s != Screen.AllScreens[0]) // Blackout only the secondary screens
                r = Rectangle.Union(r, s.Bounds);
            }
            Top = r.Top;
            Left = r.Left;
            Width = r.Width;
            Height = r.Height;
            
            TopMost = true; // This will bring your window in front of all other windows including the taskbar

            this.TransparencyKey = Color.Turquoise; //Make background transparent
            this.BackColor = Color.Turquoise;

            var relativePoint = this.PointToClient(Cursor.Position);
            initialX = relativePoint.X;
            initialY = relativePoint.Y;
        }

        public void InitTimer()
        {
            timer2 = new Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 8; // in miliseconds
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Console.WriteLine(initialX + "-DRAWFORM1-" + initialY);
            var relativePoint = this.PointToClient(Cursor.Position);
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;

            // Draw a string on the PictureBox.
            //g.DrawString("This is a diagonal line drawn on the control" + relativePoint.X + "-" + relativePoint.Y, fnt, System.Drawing.Brushes.Black, new Point(500, 500));
            // Draw a line in the PictureBox.
            //g.DrawLine(System.Drawing.Pens.Red, pictureBox1.Left, pictureBox1.Top, pictureBox1.Right, pictureBox1.Bottom);

            g.DrawLine(System.Drawing.Pens.Red, initialX-1, 0, initialX-1, 5000);
            g.DrawLine(System.Drawing.Pens.Red, 0, initialY-1, 5000, initialY-1);

            g.DrawLine(System.Drawing.Pens.Red, relativePoint.X, 0, relativePoint.X, 5000);
            g.DrawLine(System.Drawing.Pens.Red, 0, relativePoint.Y, 5000, relativePoint.Y);
        }
    }
}
