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

namespace SlaveServer
{
    public partial class SlaveUI : Form
    {

        private static int SLAVE_SERVER_ID = 23;
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
                    startButton.Text = "Stop";
                }
            }

        }

        private bool OpenChannel(int port)
        {
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            sss = new SlaveServerService(this);

            RemotingServices.Marshal(sss,
                SLAVE_SERVER_NAME,
                typeof(SlaveServerService));

            AppendTextBoxMethod("Server running on port: " + port.ToString());
            return true;

        }
        private bool CloseChannel()
        {
            master.Unregister(SLAVE_SERVER_ID);
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(sss);
            return true;
        }

        private bool RegisterOnMaster()
        {
            master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);

            if (master.Register(SLAVE_SERVER_ID, SLAVE_SERVER_LOCAL))
            {
                AppendTextBoxMethod("Registered on master :)");
                return true;
            }
                AppendTextBoxMethod("Couldn't register on master :(");
            return false;
        }

    }
}
