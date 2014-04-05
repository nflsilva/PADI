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
        private ISlaveServer slave;
        private IMasterServer master;
        private AppService appService;


        private bool isRunning;
        private bool changeServer;
        private bool usingMaster;

        public delegate void ChangeTextBox(string text);
        public delegate void ChangeServer(string local);
        public ChangeTextBox cDelegate;
        public ChangeServer sDelegate;

        public AppUI()
        {
            InitializeComponent();
            isRunning = false;
            changeServer = false;
            usingMaster = true;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            sDelegate = new ChangeServer(ChangeServerMethod);
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
                    connectButton.Text = "Disconnect";
                }
            }

        }


        private bool OpenChannel(int port)
        {
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            appService = new AppService(this);

            RemotingServices.Marshal(appService,
                APP_SERVER_NAME,
                typeof(AppService));

            AppendTextBoxMethod("Client is running on port: " + port.ToString());

            ConnectToMaster();

            return true;

        }
        private bool CloseChannel()
        {
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(appService);
            isRunning = false;
            return true;
        }

        private bool ConnectToMaster()
        {

            MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
            master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);

            AppendTextBoxMethod("Connected to DSTM System");

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

        private void createButton_Click(object sender, EventArgs e)
        {
            PadiInt pint;
            if (usingMaster)
            {
                pint = master.CreatePadiInt(Convert.ToInt32(createIDBox.Text), APP_SERVER_LOCAL);
            }
            else
            {
                pint = slave.CreatePadInt(Convert.ToInt32(createIDBox.Text));
            }

            if (pint != null)
            {
                AppendTextBoxMethod(valuesTextBox, pint.GetUid().ToString() + " : " + pint.Read().ToString());
            }
            else if(changeServer)
            {
                this.createButton_Click(sender, e);
            }
            else
            {
                AppendTextBoxMethod("Create PadiInt> PadiInt id: " + createIDBox.Text + " already exists");
            }
                
        }

        private void accessButton_Click(object sender, EventArgs e)
        {
            PadiInt pint = master.AccessPadiInt(Convert.ToInt32(accessIDBox.Text), APP_SERVER_LOCAL);

            if (pint != null)
            {
                AppendTextBoxMethod(valuesTextBox, pint.GetUid().ToString() + " : " + pint.Read().ToString());
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
            SLAVE_SERVER_LOCAL = local;
            slave = (ISlaveServer)Activator.GetObject(
                typeof(ISlaveServer),
                SLAVE_SERVER_LOCAL); 
        }

    }
}
