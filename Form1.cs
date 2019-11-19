using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*TODO 
 * Handle element disposing
 * Change default hotkey from b button
 * Dispose tray icon
 * PhotoForm close remove monitorX images
 * Hotkey to remove last screenshot
 * Documentation
*/

namespace DocuShotter
{
    public partial class Form1 : Form
    {
        public bool isPressed = false;                  //Track if global hotkey is currently down

        private int startNum, curNum = 0;               //Track current naming number

        private string savepath, prefix, description= "";

        private DrawForm drawPane;

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
            drawPane = new DrawForm(this);

            Boolean success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0002 | 0x4000, 0x51);    //Set hotkey as 'b' and try to register a global hotkey
            if (success == true)
                Console.WriteLine("Hotkey registered");
            else
                Console.WriteLine("Error registering hotkey");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            TakeBackgroundShot();
            drawPane.Setup();
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
            if (width > 0 && height > 0)
            {
                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    int length = 3;
                    savepath = textBox1.Text;
                    prefix = (textBox2.Text == "")? "" : textBox2.Text + "-";
                    startNum = Int32.Parse(textBox5.Text);
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


                    if (checkBox1.Checked)
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

                    g.CopyFromScreen(initialX, initialY, 0, 0, new Size(bmp.Width, bmp.Height));
                    Console.WriteLine("Saving image to: " + savepath + "\\" + prefix + resultingNum + description + ".png");
                    bmp.Save(savepath + "\\" + prefix + resultingNum + description + ".png");  // saves the image
                    pictureBox1.ImageLocation = savepath + "\\" + prefix + resultingNum + description + ".png";
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void TakeBackgroundShot()
        {
            // Determine the size of the "virtual screen", which includes all monitors.
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Create a bitmap of the appropriate size to receive the screenshot.
            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            {
                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                }

                // Do something with the Bitmap here, like save it to a file:
                bmp.Save("background");
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && !isPressed)
            {
                isPressed = true;
                TakeBackgroundShot();
                drawPane.Setup();
            }
            base.WndProc(ref m);
        }

        //Delete hotkey when form is closed
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Boolean success = Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());//Set hotkey as 'b'
            if (success == true)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Error");

            drawPane.Dispose();
            notifyIcon1.Dispose();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void NotifyIcon1_MouseDown(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = folderBrowserDialog1;
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }
    }
}
