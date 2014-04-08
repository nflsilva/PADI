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


namespace SampleClientApp
{
    public partial class AppUI : Form
    {

        private static int APP_DEFAULT_PORT = 8090;
        private static int MASTER_DEFAULT_PORT = 8086;
        private static string INTRO_MSG = "Hello, welcome to PADI-DSTM!";
        private static string APP_SERVER_NAME = "ClientService";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
        private string SLAVE_SERVER_LOCAL;
        private static string APP_SERVER_LOCAL = "tcp://localhost:" + APP_DEFAULT_PORT.ToString() + "/" + APP_SERVER_NAME;

        private static TcpChannel channel;
        private IServer server;
        private Dictionary<int, PadiInt> cache;
        private int txNumber;

        private bool isRunning;
        private bool onTransaction;

        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;

        public AppUI()
        {
            InitializeComponent();
            isRunning = false;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            cache = new Dictionary<int, PadiInt>();
            dialogTextBox.Text = INTRO_MSG;
            clientPortBox.Text = APP_DEFAULT_PORT.ToString();
            masterPortBox.Text = MASTER_DEFAULT_PORT.ToString();

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (CloseChannel())
                {
                    isRunning = false;
                    clientPortBox.Enabled = true;
                    masterPortBox.Enabled = true;
                    accessButton.Enabled = false;
                    createButton.Enabled = false;
                    accessIDBox.Enabled = false;
                    createIDBox.Enabled = false;
                    wValueBox.Enabled = false;
                    wID.Enabled = false;
                    startTxButton.Enabled = false;
                    writeButton.Enabled = false;
                    connectButton.Text = "Connect";
                    AppendTextBoxMethod("You are disconnected from DSTM.");
                }
                else
                {
                    AppendTextBoxMethod("Could not stop client.");
                }
            }
            else
            {
                if (OpenChannel(Convert.ToInt32(clientPortBox.Text)))
                {
                    isRunning = true;
                    clientPortBox.Enabled = false;
                    masterPortBox.Enabled = false;
                    accessButton.Enabled = true;
                    createButton.Enabled = true;
                    accessIDBox.Enabled = true;
                    createIDBox.Enabled = true;
                    wValueBox.Enabled = true;
                    wID.Enabled = true;
                    startTxButton.Enabled = true;
                    writeButton.Enabled = true;
                    connectButton.Text = "Disconnect";
                }
            }

        }


        private bool OpenChannel(int port)
        {
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            AppendTextBoxMethod("Client is running on port: " + port.ToString());

            ConnectToSystem();

            return true;

        }
        private bool CloseChannel()
        {
            ChannelServices.UnregisterChannel(channel);
            isRunning = false;
            return true;
        }

        private bool ConnectToSystem()
        {

            MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
            IMasterServer master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);


            server = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                master.GetAvailableServer());

            AppendTextBoxMethod("Connected to DSTM System");

            return true;
        }

        private bool CommitChanges(int txNumber)
        {
            bool tryResp = false;
            foreach (PadiInt pint in cache.Values)
            {
                tryResp = server.TryWrite(txNumber, pint);
                AppendTextBoxMethod("TC: " + pint.GetUid());
            }
            if (tryResp)
            {
               return server.TryTxCommit(txNumber);
            }

            return false;
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
            PadiInt pint = server.CreatePadiInt(txNumber, Convert.ToInt32(createIDBox.Text));
            if (pint == null)
            {
                AppendTextBoxMethod("Create PadiInt> PadiInt id: " + createIDBox.Text + " already exists");
            }
            else
            {
                cache.Add(pint.GetUid(), pint);
                UpdatePadIntPanel();
            } 

        }

        //access brings the object to Cache
        private void accessButton_Click(object sender, EventArgs e)
        {
            PadiInt pint;
            if (cache.ContainsKey(Convert.ToInt32(accessIDBox.Text)))
            {
                pint = cache[Convert.ToInt32(accessIDBox.Text)];
            }
            else
            {
                pint = server.AccessPadiInt(txNumber, Convert.ToInt32(accessIDBox.Text));
            }

            if (pint != null)
            {
                if (cache.ContainsKey(pint.GetUid()))
                {
                    cache.Remove(pint.GetUid());
                }
                cache.Add(pint.GetUid(), pint);
                UpdatePadIntPanel(); 
            }
            else
            {
                AppendTextBoxMethod("Create PadiInt> PadiInt id: " + createIDBox.Text + " doesn't exists");
            }

        }
        private void UpdatePadIntPanel()
        {
            valuesTextBox.Text = "";
            foreach (KeyValuePair<int, PadiInt> entry in cache)
            {
                PadiInt pint = entry.Value;
                AppendTextBoxMethod(valuesTextBox, pint.GetUid().ToString() + " : " + pint.Read().ToString());
            }

        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            PadiInt pint = cache[Convert.ToInt32(wID.Text)];
            pint.Write(Convert.ToInt32(wValueBox.Text));
            UpdatePadIntPanel();
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            if (CommitChanges(txNumber))
            {
                AppendTextBoxMethod("Tx id: " + txNumber + " has been commited!");
            }
            else
            {
                AppendTextBoxMethod("Tx id: " + txNumber + " has been aborted!");
            }
            startTxButton.Enabled = true;
            commitButton.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txNumber = server.TxBegin();
            tXNumberLabel.Text = txNumber.ToString();
            startTxButton.Enabled = false;
            commitButton.Enabled = true;
        }

    }
}
