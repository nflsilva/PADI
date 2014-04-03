using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using MasterServer;


namespace SlaveServer
{
    static class SlaveServer
    {

        private static int SLAVE_SERVER_ID = 23;
        private static string MASTER_SERVER_NAME = "tcp://localhost:8086/MasterService";
        private static string SLAVE_SERVER_LOCAL = "tcp://localhost:8085/serverID-23";
        private static int SLAVE_PORT = 8085;

        private static TcpChannel channel;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SlaveUI());

            channel = new TcpChannel(SLAVE_PORT);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(SlaveServerService),
                SLAVE_SERVER_LOCAL,
                WellKnownObjectMode.Singleton);


            MasterServerService master = (MasterServerService)Activator.GetObject(
                typeof(MasterServerService),
                MASTER_SERVER_NAME);
            master.Register(SLAVE_SERVER_ID, SLAVE_SERVER_LOCAL);


        }

    }
}
