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
using Shared;

namespace MasterServer
{
    public partial class MasterUI : Form
    {
        private static int MASTER_SERVER_ID = 0;
        private static string INTRO_MSG = "Hello, Im Master Server!";
        private static string MASTER_SERVER_NAME = "Server";
        private static string MASTER_PORT_WARNING = "Incorrect input. Only numbers allowed.";
        private static Regex MASTER_PORT_REGEX = new Regex("[0-9]+");
        private static int MASTER_DEFAULT_PORT = 2000;


        private bool isRunning;
        private static TcpChannel channel;
        private static MasterServerService mss;
  

        public delegate void ChangeTextBox(string text);
        public delegate void ChangePadIntRange(int min, int max);
        public delegate void UpdatePadInts(List<PadInt> pInts);
        public delegate void UpdateRepInts(List<PadInt> pInts);
        public ChangeTextBox cDelegate;
        public ChangePadIntRange pDelegate;
        public UpdatePadInts intDelegate;
        public UpdateRepInts repDelegate;



        public MasterUI()
        {
            InitializeComponent();        
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            pDelegate = new ChangePadIntRange(ChangeInterval);
            intDelegate = new UpdatePadInts(UpdateIntBox);
            repDelegate = new UpdateRepInts(UpdateRepBox);
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
        public void UpdateIntBox(List<PadInt> pInts)
        {
            intBox.Text = "";
            foreach (PadInt p in pInts)
            {
                if (this.intBox.Text.Length == 0)
                {
                    this.intBox.Text = p.GetUid() + "|" + p.Read() + "|" + p.GetVersion();
                }
                else
                {
                    this.intBox.AppendText("\r\n" + p.GetUid() + "|" + p.Read() + "|" + p.GetVersion());
                }
            }
        }
        public void UpdateRepBox(List<PadInt> pInts)
        {
            repBox.Text = "";
            foreach (PadInt p in pInts)
            {
                if (this.repBox.Text.Length == 0)
                {
                    this.repBox.Text = p.GetUid() + "|" + p.Read() + "|" + p.GetVersion();
                }
                else
                {
                    this.repBox.AppendText("\r\n" + p.GetUid() + "|" + p.Read() + "|" + p.GetVersion());
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (CloseChannel())
                {
                    isRunning = false;
                    mss.pingRunning = false;
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
                    mss.pingRunning = true;
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
    }
}
