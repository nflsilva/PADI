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

        private static int MAX_NUM_SERVERS = 2;
        private static SlaveUI ui;
        private static MasterServerService master;
        private Dictionary<int, PadiInt> padiInts = new Dictionary<int, PadiInt>();


        public SlaveServerService(SlaveUI nui)
        {
            ui = nui;
        }

        #region pad int

        PadiInt ISlaveServer.CreatePadInt(int uid)
        {
            if ((uid % MAX_NUM_SERVERS) == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Create PadiInt>  PadiInt id: " + uid.ToString() + " already exists!");
                    return null;
                }
                else
                {
                    PadiInt pint = new PadiInt(uid);
                    padiInts.Add(uid, pint);
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created!");
                    return pint;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                return null;
            }
        }

        PadiInt ISlaveServer.AccessPadInt(int uid)
        {
            if ((uid % MAX_NUM_SERVERS) == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was requested.");
                    return padiInts[uid];
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was not found.");
                    return null;
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                return null;
            }
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
        #endregion


    }
}
