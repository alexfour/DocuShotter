using System;
using System.Drawing;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class DrawForm : Form
    {
        private Font fnt = new Font("Arial", 10);

        private int relativeX, relativeY;

        private int initialX = 0;       //Initial X coordinate of the screenshot
        private int initialY = 0;       //Initial Y coordinate of the screenshot
        private int endX = 0;           //Destination X coordinate of the screenshot
        private int endY = 0;           //Destination Y coordinate of the screenshot
        private int shotWidth = 0;      //Width of the screenshot area
        private int shotHeight = 0;     //Height of the screenshot area

        private Form1 formis;

        private Timer mainTimer;
        Timer hidetimer;

        private bool mousePressed = false;

        public DrawForm(Form1 f)
        {
            formis = f;
            mainTimer = new Timer();
            this.DoubleBuffered = true;
            InitializeComponent();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            
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

            this.TransparencyKey = Color.Turquoise; //Make background transparent
            this.BackColor = Color.Turquoise;

            pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            TopMost = true; // This will bring your window in front of all other windows including the taskbar
        }

        public void Setup()
        {
            pictureBox1.ImageLocation = "multiple";
            this.Show();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            initialX = System.Windows.Forms.Cursor.Position.X;
            initialY = System.Windows.Forms.Cursor.Position.Y;
            InitTimer();
            mousePressed = true;
            var relativePoint = this.PointToClient(Cursor.Position);
            relativeX = relativePoint.X;
            relativeY = relativePoint.Y;
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            endX = System.Windows.Forms.Cursor.Position.X;
            endY = System.Windows.Forms.Cursor.Position.Y;

            CalculateDimensions();

            mousePressed = false;
            pictureBox1.ImageLocation = null;
            mainTimer.Stop();
            formis.SetValues(shotWidth, shotHeight, initialX, initialY);
            formis.isPressed = false;
            hidetimer = new Timer();
            hidetimer.Tick += new EventHandler(Hidetimer_Tick);
            hidetimer.Interval = 15; // in miliseconds
            hidetimer.Start();
        }

        private void Hidetimer_Tick(object sender, EventArgs e)
        {
            this.Hide();
            hidetimer.Stop();
            hidetimer.Dispose();
        }

        private void CalculateDimensions()
        {
            if (endX < initialX)
            {
                int oldinitialX = initialX;
                initialX = endX;
                endX = oldinitialX;
            }

            if (endY < initialY)
            {
                int oldinitialY = initialY;
                initialY = endY;
                endY = oldinitialY;
            }

            shotWidth = Math.Abs(initialX - endX);
            shotHeight = Math.Abs(initialY - endY);
        }

        public void InitTimer()
        {
            mainTimer.Tick += new EventHandler(mainTimer_Tick);
            mainTimer.Interval = 8; // in miliseconds
            mainTimer.Start();
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var relativePoint = this.PointToClient(Cursor.Position);
            // Create a local version of the graphics object for the PictureBox.
            Graphics g = e.Graphics;

            if (mousePressed)
            {
                g.DrawLine(System.Drawing.Pens.Red, relativeX, 0, relativeX, 5000);
                g.DrawLine(System.Drawing.Pens.Red, 0, relativeY, 5000, relativeY);

                g.DrawLine(System.Drawing.Pens.Red, relativePoint.X, 0, relativePoint.X, 5000);
                g.DrawLine(System.Drawing.Pens.Red, 0, relativePoint.Y, 5000, relativePoint.Y);
            }
        }
    }
}
