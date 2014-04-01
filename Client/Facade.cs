using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace Client
{
    public class Facade
    {
        private TcpChannel channel;
        private ClientReceiver cReceiver;
        private IMasterServer master;
        private ISlaveServer slave;

        private static string MASTER_URL = "";
        private static string FACADE_RECEIVER = "";

        public Facade(int port)
        {

            channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            cReceiver = new ClientReceiver();

            RemotingServices.Marshal(cReceiver,
                FACADE_RECEIVER,
                typeof(ClientReceiver));

            master = (IMasterServer)Activator.GetObject(
                typeof(IMasterServer),
                MASTER_URL);

        }

        #region pad int 

        public bool Init()
        {
            return false;
        }

        public PadInt CreatePadInt(int uid)
        {
            return null;
        }

        public PadInt AcessPadInt(int uid)
        {
            return null;
        }

        #endregion

        #region transactions
        public bool TxBegin()
        {
            return false;
        }

        public bool TxCommit()
        {
            return false;
        }

        public bool TxAbort()
        {
            return false;
        }

        #endregion

        #region nodes

        public bool Fail()
        {
            return false;
        }


        public bool Freeze()
        {
            return false;
        }

        public bool Recover()
        {
            return false;
        }
        #endregion

    }
}
