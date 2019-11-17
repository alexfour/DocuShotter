using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocuShotter
{
    public partial class NamePrompt : Form
    {
        public string screenshotName;

        public NamePrompt()
        {
            InitializeComponent();
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
            this.Close();
        }
    }
}
