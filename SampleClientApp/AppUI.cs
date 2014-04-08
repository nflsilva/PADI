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


        private bool isRunning;
        private bool usingMaster;

        public delegate void ChangeTextBox(string text);
        public delegate void ChangeServer(string local);
        public ChangeTextBox cDelegate;
        public ChangeServer sDelegate;

        public AppUI()
        {
            InitializeComponent();
            isRunning = false;
            usingMaster = true;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            sDelegate = new ChangeServer(ChangeServerMethod);
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
                    wIDValue.Enabled = false;
                    wID.Enabled = false;
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
                    wIDValue.Enabled = true;
                    wID.Enabled = true;
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
            Response tryResp;
            bool comResp;
            foreach (PadiInt pint in cache.Values)
            {
                tryResp = server.TryWrite(txNumber, pint);
            }

            comResp = server.TxCommit(txNumber);

            return true;
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
            Response resp;
            resp = server.CreatePadiInt(0, Convert.ToInt32(createIDBox.Text));

            if (!resp.IsChangeServer())
            {

                PadiInt pint = resp.GetPadiInt();
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
            else
            {
                usingMaster = false;
                AppendTextBoxMethod("Create PadiInt> Changing Server");
                SLAVE_SERVER_LOCAL = resp.GetLocal();
                server = (IServer)Activator.GetObject(
                    typeof(IServer),
                    SLAVE_SERVER_LOCAL); 
                this.createButton_Click(sender, e);
            }

        }


        //access brings the object to Cache
        private void accessButton_Click(object sender, EventArgs e)
        {
            Response resp;
            resp = server.AccessPadiInt(0, Convert.ToInt32(accessIDBox.Text));
           

            PadiInt pint = resp.GetPadiInt();
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
        private void ChangeServerMethod(string local)
        {
            AppendTextBoxMethod("Changed server");
            changeServer = true;
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
            bool resp;
            int txNumber;
            if (usingMaster)
            {
                txNumber = master.TxBegin();
                resp = CommitChanges(txNumber);
            }
            else
            {
                txNumber = server.TxBegin();
                resp = CommitChanges(txNumber);
            }
            if (resp)
            {
                AppendTextBoxMethod("Tx id: " + txNumber + " has been commited!");
            }
            else
            {
                AppendTextBoxMethod("Tx id: " + txNumber + " has been aborted!");
            }
             

        }

    }
}
