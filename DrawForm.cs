using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class DrawForm : Form
    {
        private Font fnt = new Font("Arial", 10);

        private int relativeX;          //X coordinate relative to the form
        private int relativeY;          //Y coordinate relative to the form
        private int initialX = 0;       //Initial X coordinate of the screenshot
        private int initialY = 0;       //Initial Y coordinate of the screenshot
        private int endX = 0;           //Destination X coordinate of the screenshot
        private int endY = 0;           //Destination Y coordinate of the screenshot
        private int shotWidth = 0;      //Width of the screenshot area
        private int shotHeight = 0;     //Height of the screenshot area
        private int mode = 0;

        private Form1 formis;           //Object holder for the main Form1
        private Timer mainTimer;        //Timer object that fires whenever this form is show and causes invalidates
        Timer hidetimer, shottimer;     //Timer that hides the form after it is no longer needed

        private bool drawanything;      //Indicates that graphics drawn on screen need to be hidden
        private bool exitWithoutScrshot;

        private bool mousePressed,released = false;

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        /// <summary>
        /// Constructor for DrawForm where the fullscreen picturebox is initialized
        /// </summary>
        /// <param name="f"></param>
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


            this.KeyPreview = true;
            this.KeyUp += DrawForm_KeyDown;
        }

        /// <summary>
        /// Setup function called when user has started the sceenshotting process.
        /// Choose from one of two screenshotting modes: instantenous or draw mode
        /// </summary>
        /// <param name="mode">Drawing mode: 0 is instantenous, 1 is draw mode</param>
        public void Setup(int mode)
        {
            pictureBox1.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + "background";

            this.Show();
            this.mode = mode;
            Console.WriteLine(mode);
            if (mode == 0)
            {
                initialX = System.Windows.Forms.Cursor.Position.X;
                initialY = System.Windows.Forms.Cursor.Position.Y;
                var relativePoint = this.PointToClient(Cursor.Position);
                relativeX = relativePoint.X;
                relativeY = relativePoint.Y;
                mousePressed = true;
            }
            InitTimer();
            drawanything = true;
            exitWithoutScrshot = false;
        }

        /// <summary>
        /// Function for taking a screenshot. 
        /// First crafts the name from the given parameters such as prefix, curNum and description
        /// Then creates a Bitmap stating from initialX,initialY and sets it's size to the parameters width and height. 
        /// Afterwards takes a screenshot of the area and saves it to the user defined folder.
        /// </summary>
        /// <param name="width">Width of the screenshot area</param>
        /// <param name="height">Height of the screenshot area</param>
        /// <param name="initialX">Initial mouse position X coordinate</param>
        /// <param name="initialY">Initial mouse position Y coordinate</param>
        public void TakeScreenShot(int width, int height, int initialX, int initialY)
        {
            string savepath = "";            //Path to the folder where images are saved
            string prefix = "";              //Holds the prefix that is prepended to the filename
            string description = "";         //Holds the description that is appended to the filename
            int startNum, curNum = 0;        //Track current naming number

            if (width > 0 && height > 0)
            {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    int length = 3;
                    savepath = formis.textBox1.Text;
                    prefix = (formis.textBox2.Text == "") ? "" : formis.textBox2.Text + "-";
                    startNum = Int32.Parse(formis.textBox5.Text);
                    String resultingNum;

                    if (startNum >= curNum)
                        curNum = startNum;

                    resultingNum = curNum.ToString().PadLeft(length, '0');

                    bool exist = Directory.EnumerateFiles(savepath + "\\", prefix + resultingNum + "*").Any();
                    Console.WriteLine(exist);

                    while (Directory.EnumerateFiles(savepath + "\\", prefix + resultingNum + "*").Any())
                    {
                        curNum++;
                        resultingNum = curNum.ToString().PadLeft(length, '0');
                    }

                    if (formis.checkBox1.Checked)
                    {
                        using (NamePrompt form = new NamePrompt())
                        {
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                description = form.screenshotName;
                                Console.WriteLine("Desc: " + description);
                            }
                        }
                    }

                    resultingNum = (description == "") ? resultingNum : resultingNum + "-";
                    pictureBox1.Invalidate();
                    pictureBox1.Update();
                    g.CopyFromScreen(initialX, initialY, 0, 0, new Size(bmp.Width, bmp.Height));
                    Console.WriteLine("Saving image to: " + savepath + "\\" + prefix + resultingNum + description + ".png");
                    bmp.Save(savepath + "\\" + prefix + resultingNum + description + ".png");  // saves the image
                    formis.pictureBox1.ImageLocation = savepath + "\\" + prefix + resultingNum + description + ".png";
                    formis.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    if (formis.checkBox2.Checked)
                        Clipboard.SetImage(bmp);

                    pictureBox1.ImageLocation = null;
                    this.Hide();
                    formis.isPressed = false;
                    formis.Opacity = 100; //Set the main form to be visible again
                }
            }
            else
            {
                pictureBox1.ImageLocation = null;
                this.Hide();
                formis.isPressed = false;
                formis.Opacity = 100; //Set the main form to be visible again
            }

        }

        //Esc to stop
        private void DrawForm_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.KeyValue);
            if (e.KeyValue == 27)
            {
                mousePressed = false;
                released = true;
                pictureBox1.Update();
                mainTimer.Stop();

                formis.isPressed = false;

                released = false;
                formis.isPressed = false;
                formis.Opacity = 100; //Set the main form to be visible again
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (mode == 1)
            {
                initialX = System.Windows.Forms.Cursor.Position.X;
                initialY = System.Windows.Forms.Cursor.Position.Y;
                
                mousePressed = true;
                var relativePoint = this.PointToClient(Cursor.Position);
                relativeX = relativePoint.X;
                relativeY = relativePoint.Y;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mode == 1)
            {
                drawanything = false;
                endX = System.Windows.Forms.Cursor.Position.X;
                endY = System.Windows.Forms.Cursor.Position.Y;
                CalculateDimensions();
                mousePressed = false;
                mainTimer.Stop();
                pictureBox1.Update();

                released = false;

                hidetimer = new Timer();
                hidetimer.Tick += new EventHandler(Hidetimer_Tick);
                hidetimer.Interval = 15; // in milliseconds
                hidetimer.Start();

            }
        }

        /// <summary>
        /// Calculates the screenshot width and height using initial and end XY coordinates
        /// </summary>
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

        /// <summary>
        /// Timer function that starts the redrawing process via Invalidate
        /// </summary>
        public void InitTimer()
        {
            mainTimer.Tick += new EventHandler(MainTimer_Tick);
            mainTimer.Interval = 8; // in milliseconds
            mainTimer.Start();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();

            if (mode == 0 && !released)
            {
                short keyState = GetAsyncKeyState(formis.hotkeyctrlkeycode);

                short ctrlkeyState = GetAsyncKeyState(formis.hotkeyletterkeycode);

                //Check if the MSB is set. If so, then the key is pressed.
                bool hotkeyScrnIsPressed = ((keyState >> 15) & 0x0001) == 0x0001;
                bool ctrlScrnIsPressed = ((ctrlkeyState >> 15) & 0x0001) == 0x0001;

                if (!hotkeyScrnIsPressed | !ctrlScrnIsPressed)
                {
                    drawanything = false;
                    endX = System.Windows.Forms.Cursor.Position.X;
                    endY = System.Windows.Forms.Cursor.Position.Y;
                    CalculateDimensions();
                    mousePressed = false;
                    released = true;
                    pictureBox1.Update();
                    mainTimer.Stop();

                    hidetimer = new Timer();
                    hidetimer.Tick += new EventHandler(Hidetimer_Tick);
                    hidetimer.Interval = 15; // in millisecondss
                    hidetimer.Start();
                }
            }
        }

        /// <summary>
        /// hideTimer tick function that hides the form in 15 milliseconds from when screenshotting process is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hidetimer_Tick(object sender, EventArgs e)
        {
            hidetimer.Stop();
            hidetimer.Dispose();
            released = false;
            if (!exitWithoutScrshot)
                TakeScreenShot(shotWidth, shotHeight, initialX, initialY);
        }

        /// <summary>
        /// Paint event where the crosshairs and data string are drawn onto the screen and then hidden when user initiates a screenshot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var relativePoint = this.PointToClient(Cursor.Position);
            Graphics g = e.Graphics;        // Create a local version of the graphics object for the PictureBox.

            if (drawanything)
            {
                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(relativePoint.X + 0, relativePoint.Y - 15, 60, 15));
                g.DrawString(relativePoint.X + "-" + relativePoint.Y, fnt, new SolidBrush(Color.Black), relativePoint.X + 0, relativePoint.Y - 15, System.Drawing.StringFormat.GenericDefault);

                if (mousePressed)
                {
                    g.FillRectangle(new SolidBrush(Color.White), new Rectangle(relativeX + 0, relativeY - 15, 60, 15));
                    g.DrawString(relativeX + "-" + relativeY, fnt, new SolidBrush(Color.Black), relativeX + 0, relativeY - 15, System.Drawing.StringFormat.GenericDefault);

                    shotWidth = Math.Abs(initialX - System.Windows.Forms.Cursor.Position.X);
                    shotHeight = Math.Abs(initialY - System.Windows.Forms.Cursor.Position.Y);
                    g.FillRectangle(new SolidBrush(Color.White), new Rectangle(relativePoint.X + 0, relativePoint.Y + 20, 85, 30));
                    g.DrawString("Width = " + shotWidth, fnt, new SolidBrush(Color.Black), relativePoint.X + 0, relativePoint.Y + 20, System.Drawing.StringFormat.GenericDefault);
                    g.DrawString("Height = " + shotHeight, fnt, new SolidBrush(Color.Black), relativePoint.X + 0, relativePoint.Y + 35, System.Drawing.StringFormat.GenericDefault);

                    g.DrawLine(System.Drawing.Pens.Red, relativeX, 0, relativeX, 5000);
                    g.DrawLine(System.Drawing.Pens.Red, 0, relativeY, 5000, relativeY);
                }

                g.DrawLine(System.Drawing.Pens.Red, relativePoint.X, 0, relativePoint.X, 5000);
                g.DrawLine(System.Drawing.Pens.Red, 0, relativePoint.Y, 5000, relativePoint.Y);
            }
        }
    }
}
