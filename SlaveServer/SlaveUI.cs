using System;
using System.Collections;
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
using System.Text.RegularExpressions;

namespace SlaveServer
{
    public partial class SlaveUI : Form
    {

        private static int SLAVE_SERVER_ID = 1;
        private static int SLAVE_DEFAULT_PORT = 2001;
        private static int MASTER_DEFAULT_PORT = 2000;
        private static string INTRO_MSG = "Hello, Im a Slave Server!";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/Server";
        private static string SLAVE_SERVER_NAME = "Server";
        private static string SLAVE_SERVER_LOCAL = "tcp://localhost:" + SLAVE_DEFAULT_PORT.ToString();
        private static string WARNING = "Incorrect input. Only numbers allowed.";
        private static string SLAVE_ON_MASTER_WARNING = "Slave and master cannot be on the same port!";
        private static Regex PORT_REGEX = new Regex("[0-9]+");
        private static Regex ID_REGEX = new Regex("[0-9]+");

        private static int TCP_PORT_TIMEOUT = 5000;
      
        private static TcpChannel channel;
        private SlaveServerService sss;
        private IMasterServer master;


        private bool isRunning;

        public bool recovered;

        public delegate void ChangeTextBox(string text);
        public delegate void UpdatePadInts(List<PadInt> pInts);
        public delegate void UpdateRepInts(List<PadInt> pInts);
        public delegate void UpdateInterval(int[] inter);
        public delegate void UpdateID(int ID);

        public delegate void Reset();

        public ChangeTextBox cDelegate;
        public UpdatePadInts intDelegate;
        public UpdateRepInts repDelegate;

        public UpdateInterval interDelegate;
        public UpdateID idDelegate;

        public Reset resetDelegate;

        public SlaveUI()
        {
            InitializeComponent();
            isRunning = false;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            intDelegate = new UpdatePadInts(UpdateIntBox);
            repDelegate = new UpdateRepInts(UpdateRepBox);
            resetDelegate = new Reset(ResetServer);
            mainPanel.Text = INTRO_MSG;
            masterPortBox.Text = MASTER_DEFAULT_PORT.ToString();
            slavePortBox.Text = SLAVE_DEFAULT_PORT.ToString();
            serverIDBox.Text = SLAVE_SERVER_ID.ToString();
        }

        public SlaveUI(IMasterServer master)
        {
            InitializeComponent();
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            intDelegate = new UpdatePadInts(UpdateIntBox);
            repDelegate = new UpdateRepInts(UpdateRepBox);
            interDelegate = new UpdateInterval(ChangeInterval);
            idDelegate = new UpdateID(ChangeId);
            this.master = master;
        }

        public int GetServerId()
        {
            return SLAVE_SERVER_ID;
        }
        public string GetServerLocal()
        {
            return SLAVE_SERVER_LOCAL + "/Server";
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
        public void ChangeInterval(int[] inter)
        {
            this.maxPadIntBox.Text = inter[0].ToString();
            this.minPadIntBox.Text = inter[1].ToString();
        }
        public void ChangeId(int ID)
        {
            this.serverIDBox.Text = ID.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (CloseChannel())
                {
                    isRunning = false;
                    sss.stopPingThread = true;
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
                if (!PORT_REGEX.IsMatch(slavePortBox.Text) || !PORT_REGEX.IsMatch(masterPortBox.Text) || !ID_REGEX.IsMatch(serverIDBox.Text))
                {
                    MessageBox.Show(WARNING);
                    return;
                }
                if (slavePortBox.Text == masterPortBox.Text) {
                    MessageBox.Show(SLAVE_ON_MASTER_WARNING);
                    return;
                }
                if (OpenChannel(Convert.ToInt32(slavePortBox.Text)) && RegisterOnMaster())
                {
                    isRunning = true;
                    slavePortBox.Enabled = false;
                    masterPortBox.Enabled = false;
                    serverIDBox.Enabled = false;
                    sss.stopPingThread = false;
                    sss.StartThread();
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
                IDictionary props = new Hashtable();
                props["port"] = port;
                props["timeout"] = TCP_PORT_TIMEOUT;
                channel = new TcpChannel(props, null, null);
                ChannelServices.RegisterChannel(channel, false);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Error: Slave port already in use");
                return false;
            }
            SLAVE_DEFAULT_PORT = port;
            SLAVE_SERVER_LOCAL = "tcp://localhost:" + SLAVE_DEFAULT_PORT.ToString(); 

            sss = new SlaveServerService(this);
            
            RemotingServices.Marshal(sss,
                SLAVE_SERVER_NAME,
                typeof(SlaveServerService));

            AppendTextBoxMethod("Server is running on port: " + port.ToString());
            return true;

        }
        private bool CloseChannel()
        {
            master.Unregister(SLAVE_SERVER_ID);
            RemotingServices.Disconnect(sss);
            ChannelServices.UnregisterChannel(channel);
            RegisterOnMaster();
            sss.StartThread();
            return true;
        }
        public void ResetServer()
        {
            RemotingServices.Disconnect(sss);
            ChannelServices.UnregisterChannel(channel);
            Application.Exit();
        }
        private bool RegisterOnMaster()
        {
            string[] response;
            master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_SERVER_LOCAL);
            try
            {
                response = master.Register(SLAVE_SERVER_LOCAL);
            }
            catch (SocketException)
            {
                System.Windows.Forms.MessageBox.Show("Error: Couldnt find master server");
                return false;
            }
            if (Convert.ToInt32(response[0])>0)
            {
                SLAVE_SERVER_ID = Convert.ToInt32(response[0]);
                //this.Invoke(this.idDelegate, SLAVE_SERVER_ID);
                IServer sserver = (IServer)Activator.GetObject(
                    typeof(IServer),
                    response[1]);
                int[] range = sserver.Split();
                master.SplitRange(SLAVE_SERVER_ID, response[1]);
                sss.SetPadIntRange(range[0], range[1]);

                //this.Invoke(this.interDelegate, range);

                sss.SetNextServer(sserver.EnterRing(SLAVE_SERVER_LOCAL));
                master.RegisterNext(SLAVE_SERVER_ID, sss.GetNextServer());

                List<PadInt> toAdd = sserver.GetSplitedObjects();
                sss.AddPadInts(toAdd);

                IServer ns = (IServer)Activator.GetObject(
                    typeof(IServer),
                    sss.GetNextServer());

                ns.ReplicateList(toAdd, false);
                return true;
            }
            CloseChannel();
            return false;
        }
    }
}
