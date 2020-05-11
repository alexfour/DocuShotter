using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/*TODO 
 * Hotkey to remove last screenshot
*/

namespace DocuShotter
{
    public partial class Form1 : Form
    {
        public bool isPressed, hidden;          //Track if global hotkey is currently down, track if Form1 is hidden

        private int startNum, curNum = 0;        //Track current naming number

        private string savepath =  "";           //Path to the folder where images are saved
        private string prefix = "";              //Holds the prefix that is prepended to the filename
        private string description = "";         //Holds the description that is appended to the filename
        private string hotkeyctrl;
        private string hotkeyletter;
        public int hotkeyctrlkeycode;
        public int hotkeyletterkeycode;

        private DrawForm drawPane;               //Holds the DrawForm object that is used to take the screenshot

        private Timer delayTimer;               //Handles the delay set by the user

        [DllImport("user32.dll")]               //Loading a DLL for the RegisterHotkey function
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]               //Loading a DLL for the UnregisterHotKey function
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

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

            button3.Text = Properties.Settings.Default.myHide;
            hidden = (button3.Text == "<") ? false : true;
            ChangeSize();

            if (Properties.Settings.Default.myMode == 0)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            checkBox1.Checked = Properties.Settings.Default.myPrompt ? true : false;
            checkBox2.Checked = Properties.Settings.Default.myClipboard ? true : false;

            hotkeyctrl = Properties.Settings.Default.hotkeyControl;
            hotkeyletter = Properties.Settings.Default.hotkeyLetter;
            hotkeyctrlkeycode = Properties.Settings.Default.hotkeyControlValue;
            hotkeyletterkeycode = Properties.Settings.Default.hotkeyLetterValue;

            textBox6.Text = hotkeyctrl + " + " + hotkeyletter;
            ChangeHotkey();
            label4.Select();
        }

        private void ChangeHotkey()
        {          
            Boolean success = Form1.UnregisterHotKey(this.Handle, this.GetType().GetHashCode());    //Delete hotkey when form is closed
            if (success == true)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Error");

            if (hotkeyctrlkeycode == 17)
                success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0002 | 0x4000, hotkeyletterkeycode);    //Control
            else if (hotkeyctrlkeycode == 16)
                success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0004 | 0x4000, hotkeyletterkeycode);    //Shift
            else if (hotkeyctrlkeycode == 18)
                success = Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 0x0001 | 0x4000, hotkeyletterkeycode);    //Alt

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
            Opacity = 0;//Hide the main form from the screenshot
            TakeBackgroundShot();
            drawPane.Setup(1);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            hidden = (button3.Text == "<") ? true : false;
            ChangeSize();
        }

        /// <summary>
        /// Change the form size depending on the hidden boolean
        /// </summary>
        private void ChangeSize()
        {
            if (hidden)
            {
                this.MinimumSize = new Size(201,489);
                this.Width = 201;
                pictureBox1.Visible = false;
                button3.Text = ">";
            }
            else
            {
                this.MinimumSize = new Size(816, 489);
                this.Width = 816;
                pictureBox1.Visible = true;
                button3.Text = "<";
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

            Opacity = 0;//Hide the main form from the screenshot
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
            Properties.Settings.Default.myHide = button3.Text;
            Properties.Settings.Default.myMode = (radioButton1.Checked) ? 0 : 1;
            Properties.Settings.Default.myPrompt = (checkBox1.Checked) ? true : false;
            Properties.Settings.Default.myClipboard = (checkBox2.Checked) ? true : false;
            Properties.Settings.Default.hotkeyControl = hotkeyctrl;
            Properties.Settings.Default.hotkeyLetter = hotkeyletter;
            Properties.Settings.Default.hotkeyControlValue = hotkeyctrlkeycode;
            Properties.Settings.Default.hotkeyLetterValue = hotkeyletterkeycode;
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

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.github.com/alexfour"); 
        }

        private void TextBox6_KeyDown(object sender, KeyEventArgs e)
        {
            textBox6.Text = "";
            if (char.IsControl((char)e.KeyCode))
            {
                if (e.KeyCode == Keys.ControlKey)
                {
                    hotkeyctrl = "Ctrl";
                    hotkeyctrlkeycode = e.KeyValue;
                }

                if (e.KeyCode == Keys.ShiftKey)
                {
                    hotkeyctrl = "Shift";
                    hotkeyctrlkeycode = e.KeyValue;
                }

                if (e.KeyCode == Keys.Menu)
                {
                    hotkeyctrl = "Alt";
                    hotkeyctrlkeycode = e.KeyValue;
                }
            }
            if (char.IsLetterOrDigit((char)e.KeyCode))
            {
                hotkeyletter = e.KeyCode.ToString().ToUpper();
                hotkeyletterkeycode = e.KeyValue;
            }
            e.Handled = true;
            textBox6.Text = hotkeyctrl + " + " + hotkeyletter;
        }

        private void TextBox6_TextChanged(object sender, EventArgs e)
        {
            HideCaret(textBox6.Handle);
        }

        private void TextBox6_MouseDown(object sender, MouseEventArgs e)
        {
            textBox6.BackColor = Color.Honeydew;
            HideCaret(textBox6.Handle);
        }

        private void TextBox6_Leave(object sender, EventArgs e)
        {
            textBox6.BackColor = Color.White;
            ChangeHotkey();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            label4.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Allow all radio buttons to be "tabbable"
            foreach (var item in this.groupBox1.Controls)
            {
                if (item.GetType() == typeof(RadioButton))
                    ((RadioButton)item).TabStopChanged += new System.EventHandler(TabStopChanged);
            }
        }

        private void TabStopChanged(object sender, EventArgs e)
        {
            ((RadioButton)sender).TabStop = true;
        }

        private void TextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !((char.IsDigit((char)e.KeyChar))|(char.IsControl((char)e.KeyChar)));
        }

        private void TextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !((char.IsDigit((char)e.KeyChar)) | (char.IsControl((char)e.KeyChar)));
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, pictureBox1.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid); //Add a lightgrey border to the picturebox
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
        private void TextBox1_MouseClick(object sender, MouseEventArgs e)
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
