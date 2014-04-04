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

namespace MasterServer
{
    public partial class MasterUI : Form
    {
        private static string INTRO_MSG = "Hello, Im Master Server!";
        private static string MASTER_SERVER_NAME = "MasterService";
        private static int MASTER_DEFAULT_PORT = 8086;


        private bool isRunning;
        private static TcpChannel channel;
        private static MasterServerService mss;
  

        public delegate void ChangeTextBox(string text);
        public ChangeTextBox cDelegate;


        public MasterUI()
        {
            InitializeComponent();        
            cDelegate = new ChangeTextBox(AppendTextBoxMethod);
            isRunning = false;
            mainPanel.Text = INTRO_MSG;
            portBox.Text = MASTER_DEFAULT_PORT.ToString();
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
            ChannelServices.RegisterChannel(channel, true);
            mss = new MasterServerService(this);

            RemotingServices.Marshal(mss,
                MASTER_SERVER_NAME,
                typeof(MasterServerService));

            return true;
        }
        private bool CloseChannel()
        {
            ChannelServices.UnregisterChannel(channel);
            RemotingServices.Disconnect(mss);
            return true;

        }

    }
}
