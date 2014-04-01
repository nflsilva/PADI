using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Threading.Tasks;

namespace SlaveServer
{
    class SlaveServerService : MarshalByRefObject, ISlaveServer
    {

        #region pad int

        public PadInt ISlaveServer.CreatePadInt(int uid)
        {
            return null;
        }

        public PadInt ISlaveServer.AcessPadInt(int uid)
        {
            return null;
        }

        #endregion

        #region transactions
        public bool ISlaveServer.TxBegin()
        {
            return false;
        }

        public bool ISlaveServer.TxCommit()
        {
            return false;
        }

        public bool ISlaveServer.TxAbort()
        {
            return false;
        }

        #endregion

        #region nodes

        public bool ISlaveServer.Fail()
        {
            return false;
        }


        public bool ISlaveServer.Freeze()
        {
            return false;
        }

        public bool ISlaveServer.Recover()
        {
            return false;
        }
        #endregion



    }
}
