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
using System.Net.Sockets;
using Shared;

namespace SlaveServer
{
    public partial class SlaveUI : Form
    {

        private static int SLAVE_SERVER_ID = 1;
        private static int SLAVE_DEFAULT_PORT = 8085;
        private static int MASTER_DEFAULT_PORT = 8086;
        private static string INTRO_MSG = "Hello, Im a Slave Server!";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
        private static string SLAVE_SERVER_NAME = "server-" + SLAVE_SERVER_ID.ToString();
        private static string SLAVE_SERVER_LOCAL = "tcp://localhost:" + SLAVE_DEFAULT_PORT.ToString() + "/" + SLAVE_SERVER_NAME; 
      
        private static TcpChannel channel;
        private SlaveServerService sss;
        private IMasterServer master;


        private bool isRunning;
        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;

        
        public SlaveUI()
        {
            InitializeComponent();
            isRunning = false;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            mainPanel.Text = INTRO_MSG;
            masterPortBox.Text = MASTER_DEFAULT_PORT.ToString();
            slavePortBox.Text = SLAVE_DEFAULT_PORT.ToString();
            serverIDBox.Text = SLAVE_SERVER_ID.ToString();
        }

        public SlaveUI(IMasterServer master)
        {
            InitializeComponent();
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            this.master = master;
        }

        private void SlaveUI_Load(object sender, EventArgs e)
        {

        }

        public int GetServerId()
        {
            return SLAVE_SERVER_ID;
        }

        public void AppendTextBoxMethod(string text)
        {
            if (this.mainPanel.Text.Length == 0)
            {
                this.mainPanel.Text = text;
            }
            else
            {
                this.mainPanel.AppendText("\r\n" + text);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (CloseChannel())
                {
                    isRunning = false;
                    slavePortBox.Enabled = true;
                    masterPortBox.Enabled = true;
                    serverIDBox.Enabled = true;
                    startButton.Text = "Start";
                    AppendTextBoxMethod("Server stoped");
                }
                else
                {
                    AppendTextBoxMethod("Could not stop server");
                }
            }
            else
            {
                if (OpenChannel(Convert.ToInt32(slavePortBox.Text)) && RegisterOnMaster())
                {
                    isRunning = true;
                    slavePortBox.Enabled = false;
                    masterPortBox.Enabled = false;
                    serverIDBox.Enabled = false;
                    startButton.Text = "Stop";
                }
                else {
                    return;                
                }
            }

        }

        private bool OpenChannel(int port)
        {
            
            try
            {
                channel = new TcpChannel(port);
                ChannelServices.RegisterChannel(channel, true);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Error: Slave port already in use");
                return false;
            }
            SLAVE_SERVER_ID = Convert.ToInt32(serverIDBox.Text);
            SLAVE_SERVER_NAME = "server-" + SLAVE_SERVER_ID.ToString();
            SLAVE_DEFAULT_PORT = port;
            SLAVE_SERVER_LOCAL = "tcp://localhost:" + SLAVE_DEFAULT_PORT.ToString() + "/" + SLAVE_SERVER_NAME; 

            sss = new SlaveServerService(this);
            
            RemotingServices.Marshal(sss,
                SLAVE_SERVER_NAME,
                typeof(SlaveServerService));

            AppendTextBoxMethod("Server id: " + SLAVE_SERVER_ID + " is running on port: " + port.ToString());
            return true;

        }
        private bool CloseChannel()
        {
            master.Unregister(SLAVE_SERVER_ID);
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(sss);
            isRunning = false;
            return true;
        }
        private bool RegisterOnMaster()
        {
            bool response = false;
            master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);
            try
            {
                response = master.Register(SLAVE_SERVER_ID, SLAVE_SERVER_LOCAL);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Error: Couldnt find master server");
                return false;
            }
            finally {
                CloseChannel();
            }
            if (response)
            {
                AppendTextBoxMethod("Registered on master :)");
                return true;
            }

            AppendTextBoxMethod("Couldn't register on master :(. The choosen id is already taken.");
            CloseChannel();
            return false;
        }

    }
}
