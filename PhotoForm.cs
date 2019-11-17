using System.Drawing;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class PhotoForm : Form
    {
        public PhotoForm(int arrayMmonitor)
        {
            Bitmap bmp = new Bitmap(Screen.AllScreens[arrayMmonitor].Bounds.Width, Screen.AllScreens[arrayMmonitor].Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Screen.AllScreens[arrayMmonitor].Bounds.X, Screen.AllScreens[arrayMmonitor].Bounds.Y, 0, 0, Screen.AllScreens[arrayMmonitor].Bounds.Size);
                bmp.Save("monitor" + arrayMmonitor);  // saves the image
            }
            InitializeComponent();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            StartPosition = FormStartPosition.Manual;
            Location = Screen.AllScreens[arrayMmonitor].WorkingArea.Location;
            Bounds = Screen.AllScreens[arrayMmonitor].Bounds;

            pictureBox1.ImageLocation = "monitor" + arrayMmonitor;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            TopMost = true; // This will bring your window in front of all other windows including the taskbar

            this.TransparencyKey = Color.Turquoise;
            this.BackColor = Color.Turquoise; 
        }
    }
}
