using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using MasterServer;
using System.Threading.Tasks;

namespace SlaveServer
{

    class SlaveServerService : MarshalByRefObject, ISlaveServer
    {
        private static SlaveUI ui;
        private static MasterServerService master;
        private Dictionary<int, PadInt> padiints;


        public SlaveServerService(SlaveUI nui)
        {
            ui = nui;
        }

        #region pad int

        PadInt ISlaveServer.CreatePadInt(int uid)
        {
            return null;
        }

        PadInt ISlaveServer.AccessPadInt(int uid)
        {
            return null;
        }

        #endregion

        #region transactions
        bool ISlaveServer.TxBegin()
        {
            return false;
        }

        bool ISlaveServer.TxCommit()
        {
            return false;
        }

        bool ISlaveServer.TxAbort()
        {
            return false;
        }

        #endregion

        #region nodes

        bool ISlaveServer.Status()
        {
            return true;
        }

        bool ISlaveServer.Fail()
        {
            return false;
        }


        bool ISlaveServer.Freeze()
        {
            return false;
        }

        bool ISlaveServer.Recover()
        {
            return false;
        }

        void imAlive()
        {
            //TODO: User later on for replication
        }
        #endregion


    }
}
