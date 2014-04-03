using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MasterServer
{
    public static class MasterServer
    {

        private static string MASTER_SERVER_NAME = "tcp://localhost:8086/MasterService";
        private static int MASTER_PORT = 8086;

        private static TcpChannel channel;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MasterUI masterUI = new MasterUI();
            Application.Run(masterUI);

            channel = new TcpChannel(MASTER_PORT);
            ChannelServices.RegisterChannel(channel, true); 

            MasterServerService mss = new MasterServerService(masterUI);

            RemotingServices.Marshal(mss,
                "MasterService",
                typeof(MasterServerService));


        }
    }
}
