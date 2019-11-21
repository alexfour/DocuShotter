using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*TODO 
 * Allow customization of hotkey
 * Hotkey to remove last screenshot
*/

namespace DocuShotter
{
    public partial class Form1 : Form
    {
        public bool isPressed = false;          //Track if global hotkey is currently down

        private int startNum, curNum = 0;        //Track current naming number

        private string savepath =  "";           //Path to the folder where images are saved
        private string prefix = "";              //Holds the prefix that is prepended to the filename
        private string description = "";         //Hold the description that is appended to the filename

        private DrawForm drawPane;               //Holds the DrawForm object that is used to take the screenshot

        private Timer delayTimer;               //Handles the delay set by the user

        [DllImport("user32.dll")]               //Loading a DLL for the RegisterHotkey function
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]               //Loading a DLL for the UnregisterHotKey function
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Constructor for the Form1 class.
        /// </summary>
        public Form1()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.KeyPreview = true;
            drawPane = new DrawForm(this);

            textBox1.Text = Properties.Settings.Default.myPath;
            textBox2.Text = Properties.Settings.Default.myPrefix;
            textBox3.Text = Properties.Settings.Default.myDelay;

            if (Properties.Settings.Default.myMode == 0)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            checkBox1.Checked = (Properties.Settings.Default.myPrompt) ? true : false; 

            Boolean success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0002 | 0x4000, 0x51);    //Set hotkey as 'b' and try to register a global hotkey
            if (success == true)
                Console.WriteLine("Hotkey registered");
            else
                Console.WriteLine("Error registering hotkey");
        }

        /// <summary>
        /// Screenshot button on the form. Performs the same as the hotkey feature.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            TakeBackgroundShot();
            if (radioButton1.Checked)
                drawPane.Setup(0);
            if (radioButton2.Checked)
                drawPane.Setup(1);
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

        /// <summary>
        /// Takes a screenshot of all monitors and saves it to the executable directory as "background"
        /// </summary>
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
                bmp.Save(AppDomain.CurrentDomain.BaseDirectory + "background");
            }
        }

        /// <summary>
        /// Listens for a global hotkey press.
        /// Once pressed initiates the screenshot procedure by calling DrawForm.Setup function
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && !isPressed)
            {
                isPressed = true;

                int delay = Int32.Parse(textBox3.Text);
                delayTimer = new Timer();
                delayTimer.Tick += new EventHandler(DelayTimer_Tick);
                delayTimer.Interval = delay*1000+1; // in milliseconds
                delayTimer.Start();
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Tick function for delayTimer that allows for delayed screenshot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            delayTimer.Stop();
            delayTimer.Dispose();

            TakeBackgroundShot();
            if (radioButton1.Checked)
                drawPane.Setup(0);
            if (radioButton2.Checked)
                drawPane.Setup(1);
        }
        
        /// <summary>
        /// Form closing function that handles unregistering the global hotkey and disposing of elements.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Boolean success = Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());    //Delete hotkey when form is closed
            if (success == true)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Error");

            
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "background");
            drawPane.Dispose();
            notifyIcon1.Dispose();

            Properties.Settings.Default.myPath = textBox1.Text;
            Properties.Settings.Default.myPrefix = textBox2.Text;
            Properties.Settings.Default.myDelay = textBox3.Text;
            Properties.Settings.Default.myMode = (radioButton1.Checked) ? 0 : 1;
            Properties.Settings.Default.myPrompt = (checkBox1.Checked) ? true : false;
            Properties.Settings.Default.Save();
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

        /// <summary>
        /// Button function for handling a FolderBrowserDialog to assist user in choosing a folder for their pictures.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
