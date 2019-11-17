using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*TODO 
 * Handle element disposing
 * Make a tranformation to screenshot
 * Allow user to set naming/storing/etc. preferences
 * Change default hotkey from b button
 * Documentation
*/

namespace DocuShotter
{
    public partial class Form1 : Form
    {
        private bool isPressed = false; //Track if global hotkey is currently down

        private Timer buttonTimer;

        private int initialX = 0;       //Initial X coordinate of the screenshot
        private int initialY = 0;       //Initial Y coordinate of the screenshot
        private int endX = 0;           //Destination X coordinate of the screenshot
        private int endY = 0;           //Destination Y coordinate of the screenshot
        private int shotWidth = 0;      //Width of the screenshot area
        private int shotHeight = 0;     //Height of the screenshot area

        List<PhotoForm> screenArray = new List<PhotoForm>();
        DrawForm drawPane;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public Form1()
        {
            this.DoubleBuffered = true;
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
            PrepareForms();
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

        private void PrepareForms()
        {
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                PhotoForm newform = new PhotoForm(i);
                newform.Show();
                screenArray.Add(newform);
            }
            initialX = System.Windows.Forms.Cursor.Position.X;
            initialY = System.Windows.Forms.Cursor.Position.Y;
            drawPane = new DrawForm();
            drawPane.Show();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && !isPressed)
            {
                //Console.Write("pressed" + isPressed);
                isPressed = true;
                InitTimer();
                PrepareForms();
            }
            base.WndProc(ref m);
        }

  
        public void InitTimer()
        {
            Console.WriteLine("inittimer");

            buttonTimer = new Timer();
            buttonTimer.Tick += new EventHandler(ButtonTimer_Tick);
            buttonTimer.Interval = 16; // in miliseconds
            buttonTimer.Start();
        }

        private void ButtonTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("timertick");
            short keyState = GetAsyncKeyState(0x42);

            //Check if the MSB is set. If so, then the key is pressed.
            bool prntScrnIsPressed = ((keyState >> 15) & 0x0001) == 0x0001;

            if (!prntScrnIsPressed)
            {
                Console.WriteLine("released" + isPressed);//TODO Execute client code...
                //myBrush.Dispose();
                //formGraphics.Dispose();

                buttonTimer.Stop();
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

                for (int i=0; i<screenArray.Count;i++)
                {
                    screenArray[i].Dispose();
                }
                drawPane.Dispose();
                //PhotoFormVar.Dispose();
            }
        }

        //Delete hotkey when form is closed
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Boolean success = Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());//Set hotkey as 'b'
            if (success == true)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Error");

            //formGraphics.Dispose();
        }
    }
}
