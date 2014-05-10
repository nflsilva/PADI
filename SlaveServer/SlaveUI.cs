﻿using System;
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
        private static int SLAVE_DEFAULT_PORT = 8085;
        private static int MASTER_DEFAULT_PORT = 8086;
        private static string INTRO_MSG = "Hello, Im a Slave Server!";
        private static string MASTER_SERVER_LOCAL = "tcp://localhost:" + MASTER_DEFAULT_PORT.ToString() + "/MasterService";
        private static string SLAVE_SERVER_NAME = "Server";
        private static string SLAVE_SERVER_LOCAL = "tcp://localhost:" + SLAVE_DEFAULT_PORT.ToString();
        private static string WARNING = "Incorrect input. Only numbers allowed.";
        private static string SLAVE_ON_MASTER_WARNING = "Slave and master cannot be on the same port!";
        private static Regex PORT_REGEX = new Regex("[0-9]+");
        private static Regex ID_REGEX = new Regex("[0-9]+");
      
        private static TcpChannel channel;
        private SlaveServerService sss;
        private IMasterServer master;


        private bool isRunning;
        public delegate void ChangeTextBox(string text);
        public delegate void ChangePadIntRange(int min, int max);
        public ChangeTextBox cDelegate;
        public ChangePadIntRange pDelegate;

        public SlaveUI()
        {
            InitializeComponent();
            isRunning = false;
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            pDelegate = new ChangePadIntRange(ChangeInterval);
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

        public void ChangeInterval(int min, int max)
        {
            this.maxPadIntBox.Text = max.ToString();
            this.minPadIntBox.Text = min.ToString();
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
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(sss);
            isRunning = false;
            sss.pingRunning = false;
            return true;
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
                this.serverIDBox.Text = response[0];
                IServer sserver = (IServer)Activator.GetObject(
                    typeof(IServer),
                    response[1]);
                int[] range = sserver.Split();
                master.SplitRange(SLAVE_SERVER_ID, response[1]);
                sss.SetPadIntRange(range[0], range[1]);
                this.ChangeInterval(range[0], range[1]);

                sss.SetNextServer(sserver.EnterRing(SLAVE_SERVER_LOCAL));
                master.RegisterNext(SLAVE_SERVER_ID, sss.GetNextServer());
                return true;
            }
            CloseChannel();
            return false;
        }

    }
}
