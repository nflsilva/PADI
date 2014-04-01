using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading.Tasks;

namespace MasterServer
{
    class MasterServerService : MarshalByRefObject, IMasterServer
    {

        #region pad int

        public PadInt IMasterServer.CreatePadInt(int uid)
        {
            return null;
        }

        public PadInt IMasterServer.AcessPadInt(int uid)
        {
            return null;
        }

        #endregion

        #region transactions
        public bool IMasterServer.TxBegin()
        {
            return false;
        }

        public bool IMasterServer.TxCommit()
        {
            return false;
        }

        public bool IMasterServer.TxAbort()
        {
            return false;
        }

        #endregion

        #region nodes

        public bool IMasterServer.Fail()
        {
            return false;
        }


        public bool IMasterServer.Freeze()
        {
            return false;
        }

        public bool IMasterServer.Recover()
        {
            return false;
        }
        #endregion

    }
}
