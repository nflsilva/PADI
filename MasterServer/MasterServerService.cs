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

        private Dictionary<int, string> servers = new Dictionary<int, string>();
        private static MasterUI ui;

        public MasterServerService(MasterUI nui)
        {
            ui = nui;
        }

        #region pad int

        PadInt IMasterServer.CreatePadiInt(int uid)
        {
            return null;
        }

        PadInt IMasterServer.AccessPadiInt(int uid)
        {
            return null;
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
