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
    static class MasterServer
    {

        private static string MASTER_SERVER_NAME = "";
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
            Application.Run(new MasterUI());

            channel = new TcpChannel(MASTER_PORT);
            ChannelServices.RegisterChannel(channel, true); 

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(MasterServerService),
                MASTER_SERVER_NAME,
                WellKnownObjectMode.Singleton);
        }
    }
}
