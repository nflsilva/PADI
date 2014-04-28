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
using System.Text.RegularExpressions;
using PADI_DSTM;


namespace SampleClientApp
{
    public partial class AppUI : Form
    {
        private Regex ID_PATTERN = new Regex("[0-9]+");
        private String ID_WARNING = "Invalid ID value! \r\nCan consist of numbers only. Cannot be empty";
        private Regex VALUE_PATTERN = new Regex("[0-9]+");
        private String VALUE_WARNING = "Invalid value! \r\nCan consist of numbers only. Cannot be empty";
        private Regex URI_PATTERN = new Regex("(tcp)(:)(\\/)(\\/)(localhost)(:)[0-9]+(\\/)(server-[0-9])");
        private String URI_WARNING = "Invalid URI. Format: tcp://localhost:xxxx/server-x)\r\nNB: May be found on interface of Master";

        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;

        public AppUI()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            startTxButton.Enabled = true;
            if (clientPortBox.Text == "" && masterPortBox.Text =="")
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to use default values? \r\n" +
                    "Client port: 8090\r\nMaster port: 8086", "PADI-DSTM", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    clientPortBox.Text = "8090";
                    masterPortBox.Text = "8086";
                    PadiDstm.Init();
                }
                else if (dialogResult == DialogResult.No)
                {
                    return;
                }
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
            if (!ID_PATTERN.IsMatch(createIDBox.Text))
            {
                MessageBox.Show(ID_WARNING);
                return;
            }
            PadInt pint = PadiDstm.CreatePadInt(Convert.ToInt32(createIDBox.Text));
            AppendTextBoxMethod(valuesTextBox, pint.GetUid() + " | " + pint.Read() + " | " + pint.GetVersion());
        }

        //access brings the object to Cache
        private void accessButton_Click(object sender, EventArgs e)
        {

            if (!ID_PATTERN.IsMatch(accessIDBox.Text))
            {
                MessageBox.Show(ID_WARNING);
                return;
            }
            PadInt pint = PadiDstm.AccessPadInt(Convert.ToInt32(accessIDBox.Text));
            if (pint == null)
            {
                AppendTextBoxMethod(dialogTextBox, "PadiInt does not exist!");

            }
            else
            {
                AppendTextBoxMethod(valuesTextBox, pint.GetUid() + " | " + pint.Read() + " | " + pint.GetVersion());
            }

        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            if (!ID_PATTERN.IsMatch(wID.Text))
            {
                MessageBox.Show(ID_WARNING);
                return;
            }
            if (!VALUE_PATTERN.IsMatch(wValueBox.Text))
            {
                MessageBox.Show(VALUE_WARNING);
                return;
            }
            try
            {
                PadInt pint = PadiDstm.WritePadInt(Convert.ToInt32(wID.Text), Convert.ToInt32(wValueBox.Text));
                AppendTextBoxMethod(valuesTextBox, pint.GetUid() + " | " + pint.Read() + " | " + pint.GetVersion());
            }
            catch (ArgumentNullException) {
                MessageBox.Show("Cannot write. Object is not in cache.");
            }
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
            try
            {
                PadiDstm.TxCommit();
            }
            catch (TxException)
            {
                MessageBox.Show("Could not commit");
            }
            
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
            if (!URI_PATTERN.IsMatch(ServerLocalBox.Text))
            {
                MessageBox.Show(URI_WARNING);
                return;
            }
            PadiDstm.Freeze(ServerLocalBox.Text);

        }

        private void RecoverButton_Click(object sender, EventArgs e)
        {
            if (!URI_PATTERN.IsMatch(ServerLocalBox.Text))
            {
                MessageBox.Show(URI_WARNING);
                return;
            }
            PadiDstm.Recover(ServerLocalBox.Text);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!URI_PATTERN.IsMatch(ServerLocalBox.Text))
            {
                MessageBox.Show(URI_WARNING);
                return;
            }
            PadiDstm.Fail(ServerLocalBox.Text);
        }

        private void statusButton_Click(object sender, EventArgs e)
        {
            if (!URI_PATTERN.IsMatch(ServerLocalBox.Text))
            {
                MessageBox.Show(URI_WARNING);
                return;
            }
            PadiDstm.Status();

        }
    }
}
