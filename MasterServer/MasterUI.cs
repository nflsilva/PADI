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
using System.Text.RegularExpressions;

namespace MasterServer
{
    public partial class MasterUI : Form
    {
        private static int MASTER_SERVER_ID = 0;
        private static string INTRO_MSG = "Hello, Im Master Server!";
        private static string MASTER_SERVER_NAME = "MasterService";
        private static string MASTER_PORT_WARNING = "Incorrect input. Only numbers allowed.";
        private static Regex MASTER_PORT_REGEX = new Regex("[0-9]+");
        private static int MASTER_DEFAULT_PORT = 8086;


        private bool isRunning;
        private static TcpChannel channel;
        private static MasterServerService mss;
  

        public delegate void ChangeTextBox(string text);
        public delegate void ChangePadIntRange(int min, int max);
        public ChangeTextBox cDelegate;
        public ChangePadIntRange pDelegate;



        public MasterUI()
        {
            InitializeComponent();        
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            pDelegate = new ChangePadIntRange(ChangeInterval);
            isRunning = false;
            mainPanel.Text = INTRO_MSG;
            portBox.Text = MASTER_DEFAULT_PORT.ToString();
        }

        public int GetServerId()
        {
            return MASTER_SERVER_ID;
        }

        public void AppendTextBoxMethod(string text)
        {
            if (mainPanel.Text.Length == 0)
            {
                mainPanel.Text = text;
            }
            else {
                mainPanel.AppendText("\r\n" + text);
            }
        }
        public void ChangeInterval(int min, int max)
        {
            this.maxPadIntBox.Text = max.ToString();
            this.minPadIntBox.Text = min.ToString();
        }

        private void MasterUI_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (CloseChannel())
                {
                    isRunning = false;
                    portBox.Enabled = true;
                    StartButton.Text = "Start";
                    AppendTextBoxMethod("Server stoped");
                }
                else
                {
                    AppendTextBoxMethod("Could not stop server");
                }
            }
            else
            {
                if (!MASTER_PORT_REGEX.IsMatch(portBox.Text))
                {
                    MessageBox.Show(MASTER_PORT_WARNING);
                    return;
                }
                if (OpenChannel(Convert.ToInt32(portBox.Text)))
                {
                    isRunning = true;
                    portBox.Enabled = false;
                    StartButton.Text = "Stop";
                    AppendTextBoxMethod("Server is running on port: " + portBox.Text);
                }
            }

        }
        private bool OpenChannel(int port)
        {
            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            mss = new MasterServerService(this);

            RemotingServices.Marshal(mss,
                MASTER_SERVER_NAME,
                typeof(MasterServerService));
            isRunning = true;
            return true;
        }
        private bool CloseChannel()
        {
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(mss);
            isRunning = false;
            return true;

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void minPadIntBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void maxPadIntBox_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
