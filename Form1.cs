using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class Form1 : Form
    {

        private bool isPressed = false; //Track if global hotkey is currently down

        private int initialX = 0;       //Initial X coordinate of the screenshot
        private int initialY = 0;       //Initial Y coordinate of the screenshot
        private int endX = 0;           //Destination X coordinate of the screenshot
        private int endY = 0;           //Destination Y coordinate of the screenshot
        private int shotWidth = 0;      //Width of the screenshot area
        private int shotHeight = 0;     //Height of the screenshot area

        System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
        System.Drawing.Graphics formGraphics,gra;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;

            Boolean success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0000, 0x42);    //Set hotkey as 'b' and try to register a global hotkey
            if (success == true)
                Console.WriteLine("Hotkey registered");
            else
                Console.WriteLine("Error registering hotkey");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = System.Windows.Forms.Cursor.Position.X + " " + System.Windows.Forms.Cursor.Position.Y;
        }

        /// <summary>
        /// Main function for taking the screenshot. Creates a Bitmap and sets it's size to the parameters width and height. 
        /// Afterwards takes a screenshot of the area and saves it.
        /// </summary>
        /// <param name="width">Width of the screenshot area</param>
        /// <param name="height">Height of the screenshot area</param>
        private void TakeScreenShot(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(initialX, initialY, 0, 0, new Size(bmp.Width, bmp.Height));
                    bmp.Save("screenshot.png");  // saves the image
                    pictureBox1.ImageLocation = "screenshot.png";
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && !isPressed)
            {
                initialX = System.Windows.Forms.Cursor.Position.X;
                initialY = System.Windows.Forms.Cursor.Position.Y;
                Console.Write("pressed" + isPressed);
                isPressed = true;
                InitTimer();
                //MessageBox.Show("Catched");//You can replace this statement with your desired response to the Hotkey.
            }
            base.WndProc(ref m);
        }

        //Delete hotkey when form is closed
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Boolean success = Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());//Set hotkey as 'b'
            if (success == true)
                MessageBox.Show("Success");
            else
                MessageBox.Show("Error");
        }

        private Timer timer2;
        public void InitTimer()
        {
            Console.WriteLine("inittimer");

            timer2 = new Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 16; // in miliseconds
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("timertick");
            short keyState = GetAsyncKeyState(0x42);

            //Check if the MSB is set. If so, then the key is pressed.
            bool prntScrnIsPressed = ((keyState >> 15) & 0x0001) == 0x0001;

            if (prntScrnIsPressed)
            {
                //IntPtr desktopPtr = GetDC(IntPtr.Zero);
                //Graphics gra = Graphics.FromHdc(desktopPtr);
                //SolidBrush b = new SolidBrush(Color.White);
                //gra.FillRectangle(b, new Rectangle(initialX, initialY, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                //formGraphics.FillRectangle(myBrush, new Rectangle(initialX, initialY, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                Console.WriteLine("pressed still");
                //gra.Dispose();
                //ReleaseDC(IntPtr.Zero, desktopPtr);
            }
            else
            {
                Console.WriteLine("released" + isPressed);//TODO Execute client code...
                timer2.Stop();
                isPressed = false;
                endX = System.Windows.Forms.Cursor.Position.X;
                endY = System.Windows.Forms.Cursor.Position.Y;

                //Swap X and Y values to allow screenshotting both ways
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
                TakeScreenShot(shotWidth, shotHeight);
            }
        }
    }
}
