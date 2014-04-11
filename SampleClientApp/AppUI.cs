using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using PADI_DSTM;


namespace SampleClientApp
{
    public partial class AppUI : Form
    {


        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;

        public AppUI()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            startTxButton.Enabled = true;
            if (clientPortBox.Text == "")
            {
                PadiDstm.Init();
            }
            else
            {
                PadiDstm.Init(Convert.ToInt32(clientPortBox.Text));
            }
        }

        public void AppendTextBoxMethod(string text)
        {
            if (dialogTextBox.Text.Length == 0)
            {
                dialogTextBox.Text = text;
            }
            else
            {
                dialogTextBox.AppendText("\r\n" + text);
            }
        }
        public void AppendTextBoxMethod(TextBox box, string text)
        {
            if (box.Text.Length == 0)
            {
                box.Text = text;
            }
            else
            {
                box.AppendText("\r\n" + text);
            }
        }

        //created the object on local Cache
        private void createButton_Click(object sender, EventArgs e)
        {
            PadiInt pint = PadiDstm.CreatePadiInt(Convert.ToInt32(createIDBox.Text));
            AppendTextBoxMethod(valuesTextBox, pint.GetUid() + " | " + pint.Read() + " | " + pint.GetVersion());
        }

        //access brings the object to Cache
        private void accessButton_Click(object sender, EventArgs e)
        {
            PadiInt pint = PadiDstm.AccessPadiInt(Convert.ToInt32(accessIDBox.Text));
            AppendTextBoxMethod(valuesTextBox, pint.GetUid() + " | " + pint.Read() + " | " + pint.GetVersion());

        }

        private void writeButton_Click(object sender, EventArgs e)
        {
 
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            commitButton.Enabled = false;
            startTxButton.Enabled = true;

            wID.Enabled = false;
            wID.Text = "";
            writeButton.Enabled = false;

            wValueBox.Enabled = false;
            wValueBox.Text = "";

            createButton.Enabled = false;
            createIDBox.Enabled = false;
            createIDBox.Text = "";

            accessButton.Enabled = false;
            accessIDBox.Enabled = false;
            accessIDBox.Text = "";

            PadiDstm.TxCommit();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            commitButton.Enabled = true;
            startTxButton.Enabled = true;

            wID.Enabled = true;
            wID.Text = "";
            writeButton.Enabled = true;

            wValueBox.Enabled = true;
            wValueBox.Text = "";

            createButton.Enabled = true;
            createIDBox.Enabled = true;
            createIDBox.Text = "";

            accessButton.Enabled = true;
            accessIDBox.Enabled = true;
            accessIDBox.Text = "";

            PadiDstm.TxBegin();

        }

        private void FreezeButton_Click(object sender, EventArgs e)
        {
            PadiDstm.Freeze(ServerLocalBox.Text);

        }

        private void RecoverButton_Click(object sender, EventArgs e)
        {
            PadiDstm.Recover(ServerLocalBox.Text);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            PadiDstm.Fail(ServerLocalBox.Text);
        }

        private void statusButton_Click(object sender, EventArgs e)
        {
            PadiDstm.Status();

        }
    }
}
