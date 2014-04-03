using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterServer
{
    public partial class MasterUI : Form
    {

        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;

        public MasterUI()
        {
            InitializeComponent();
            cDelegate = new ChangeTextBox(ChangeTextBoxMethod);

        }

        public void ChangeTextBoxMethod(string text)
        {
            this.textBox2.Text = text;
        }

        private void MasterUI_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
