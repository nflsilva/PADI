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

        private static int MAX_NUM_SERVERS = 3;
        private Dictionary<int, string> servers;
        private Dictionary<int, PadiInt> padiInts;
        private Dictionary<int, List<PadiInt>> transactions;
        private static MasterUI ui;

        public MasterServerService(MasterUI nui)
        {
            ui = nui;
            padiInts = new Dictionary<int, PadiInt>();
            servers = new Dictionary<int, string>();
            transactions = new Dictionary<int, List<PadiInt>>();
        }

        #region pad int

        Response IMasterServer.CreatePadiInt(int txNumber, int uid)
        {
            if (!transactions.ContainsKey(txNumber))
            {
                transactions.Add(txNumber, new List<PadiInt>());
            }

            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Create PadiInt>  PadiInt id: " + uid.ToString() + " already exists!");
                    return new Response(false, null, null);
                }
                else
                {
                    PadiInt pint = new PadiInt(uid);
                    transactions[txNumber].Add(pint);
                    ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " was created on transaction " + txNumber +" !");
                    return new Response(false, null, pint);
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Create PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                ISlaveServer server = (ISlaveServer)Activator.GetObject(
                typeof(ISlaveServer),
                servers[targetServerID]);
                server.JoinTx(txNumber);
                return new Response(true, servers[targetServerID], null);
            }
        }

        //This function corresponds to a read(); the txNumber will be here only for future use, in case of a change, but for an
        //optimistic aprouch, this isn't used
        Response IMasterServer.AccessPadiInt(int txNumber, int uid)
        {
            int targetServerID = uid % MAX_NUM_SERVERS;
            if (targetServerID == ui.GetServerId())
            {
                if (padiInts.ContainsKey(uid))
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was requested.");
                    return new Response(false, null, padiInts[uid]);
                }
                else
                {
                    ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " was not found.");
                    return new Response(false, null, null);
                }
            }
            else
            {
                ui.Invoke(ui.cDelegate, "Access PadiInt> PadiInt id: " + uid.ToString() + " isn't in this server.");
                return new Response(true, servers[targetServerID], null);
            }
        }

        Response IMasterServer.TryWrite(int txNumber, PadiInt padiInt)
        {
            return null;
        }

        #endregion

        #region transactions
        int IMasterServer.TxBegin()
        {
            return 0;
        }

        bool IMasterServer.TxCommit(int txNumber)
        {
            foreach (PadiInt pint in transactions[txNumber])
            {
                padiInts.Add(pint.GetUid(), pint);
            }
            ui.Invoke(ui.cDelegate, "TxCommit> Tx id: " + txNumber + " has been commited!");
            return true;
        }

        bool IMasterServer.TxAbort(int txNumber)
        {
            transactions.Remove(txNumber);
            ui.Invoke(ui.cDelegate, "TxAbort> Tx id: " + txNumber + " has been aborted!");
            return true;
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
