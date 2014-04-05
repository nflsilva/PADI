using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MasterServer
{
    public class MasterServerService : MarshalByRefObject, IMasterServer
    {

        private static int MAX_NUM_SERVERS = 2;
        private Dictionary<int, string> servers = new Dictionary<int, string>();
        private Dictionary<int, PadiInt> padiInts = new Dictionary<int, PadiInt>();
        private static MasterUI ui;

        public MasterServerService(MasterUI nui)
        {
            ui = nui;
        }

        #region pad int

        PadiInt IMasterServer.CreatePadiInt(int uid)
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

        PadiInt IMasterServer.AccessPadiInt(int uid)
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
        bool IMasterServer.TxBegin()
        {
            return false;
        }

        bool IMasterServer.TxCommit()
        {
            return false;
        }

        bool IMasterServer.TxAbort()
        {
            return false;
        }

        #endregion

        #region nodes
        bool IMasterServer.Status()
        {
            return false;

        }


        bool IMasterServer.Fail()
        {
            return false;
        }


        bool IMasterServer.Freeze()
        {
            return false;
        }

        bool IMasterServer.Recover()
        {
            return false;
        }

        bool IMasterServer.Register(int sid, string slocal)
        {
            if (servers.ContainsKey(sid))
            {
                return false;
            }
            servers.Add(sid, slocal);
            ui.Invoke(ui.cDelegate, "Registered server id: " + sid.ToString() + " located at: " + slocal);
            return true;
        }
        bool IMasterServer.Unregister(int sid)
        {
            if (servers.Remove(sid))
            {
                ui.Invoke(ui.cDelegate, "Removed server id: " + sid.ToString());
                return true;
            }
            return false;
        }

        #endregion

    }
}
