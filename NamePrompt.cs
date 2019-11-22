using System;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class NamePrompt : Form
    {
        public string screenshotName;

        public NamePrompt()
        {
            InitializeComponent();
            //Location = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            this.AcceptButton = button1;
            button1.DialogResult = DialogResult.OK;
            TopMost = true;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            screenshotName = textBox1.Text;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Opacity = 0;
            this.Close();
        }

        private void NamePrompt_Load(object sender, EventArgs e)
        {
            this.SetDesktopLocation(System.Windows.Forms.Cursor.Position.X - this.Width/2 - 50, System.Windows.Forms.Cursor.Position.Y - this.Height/2 - 18);
        }
    }
}
